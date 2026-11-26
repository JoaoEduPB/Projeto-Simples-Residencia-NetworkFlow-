using Godot;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class Professor : Area2D
{
	[Signal]
	public delegate void BatalhaIniciadaEventHandler(Professor professor);

	[Export]
	public string NomeProfessor { get; set; } = "Prof. Marcus";

	[Export]
	public string CamadaChave { get; set; } = "Fisica";

	[Export]
	public int VidaMaxima { get; set; } = 100;
	
	[Export(PropertyHint.File, "*.tscn")]
	public string CenaDestino { get; set; } = "res://src/fases/CenaBatalha.tscn";

	private List<Questao> _todasQuestoesDaCamada = new();
	private List<Questao> _baralhoDisponivel = new();
	private bool _jogadorPerto = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
		CarregarQuestoesDoJson("res://src/dados/questoes.json");
		GD.Print($"Professor pronto. Monitoring={Monitoring}, Mask={CollisionMask}, CenaDestino='{CenaDestino}'");
	}

	public override void _Process(double delta)
{
	if (_jogadorPerto && Input.IsActionJustPressed("ui_accept"))
	{
		_jogadorPerto = false;
		PrepararNovaBatalha();
		GD.Print($"Batalha iniciada com {NomeProfessor}! Questões prontas: {_baralhoDisponivel.Count}");
		EmitSignal(SignalName.BatalhaIniciada, this);

		var erro = GetTree().ChangeSceneToFile(CenaDestino);
		if (erro != Error.Ok)
		{
			GD.PrintErr($"Falha ao trocar de cena: {erro}");
		}
	}
}

	private void CarregarQuestoesDoJson(string caminhoArquivo)
	{
		if (!FileAccess.FileExists(caminhoArquivo))
		{
			GD.PrintErr($"Arquivo não encontrado: {caminhoArquivo}");
			return;
		}

		string conteudoJson = FileAccess.GetFileAsString(caminhoArquivo);
		var opcoes = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			Converters = { new JsonStringEnumConverter() }
		};

		var bancoCompleto = JsonSerializer.Deserialize<Dictionary<string, List<Questao>>>(conteudoJson, opcoes);

		if (bancoCompleto != null && bancoCompleto.TryGetValue(CamadaChave, out var questoes))
		{
			_todasQuestoesDaCamada = questoes;
		}
		else
		{
			GD.PrintErr($"Camada '{CamadaChave}' não encontrada no JSON.");
		}
	}

	public void PrepararNovaBatalha()
	{
		_baralhoDisponivel = new List<Questao>(_todasQuestoesDaCamada);
	}

	public Questao SortearProximaQuestao()
	{
		if (_baralhoDisponivel.Count == 0)
		{
			return null;
		}

		int indiceAleatorio = GD.RandRange(0, _baralhoDisponivel.Count - 1);
		Questao escolhida = _baralhoDisponivel[indiceAleatorio];
		_baralhoDisponivel.RemoveAt(indiceAleatorio);

		return escolhida;
	}

	private void OnBodyEntered(Node2D body)
	{
		GD.Print($"Entrou: {body.GetPath()} | grupos: {string.Join(", ", body.GetGroups())} | pai: {body.GetParent()?.Name} | grupos do pai: {string.Join(", ", body.GetParent()?.GetGroups() ?? new())}");

		if (body is CharacterBody2D)
		{
			_jogadorPerto = true;
			GD.Print("Encostou no prof! Apertar Enter ou Espaço");
		}
	}


	private void OnBodyExited(Node2D body)
	{
		if (body.IsInGroup("Player"))
		{
			_jogadorPerto = false;
			GD.Print("Saiu de perto do prof.");
		}
	}
}
