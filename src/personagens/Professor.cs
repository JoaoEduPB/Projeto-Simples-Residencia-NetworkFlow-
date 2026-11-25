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

	private List<Questao> _todasQuestoesDaCamada = new();
	private List<Questao> _baralhoDisponivel = new();
	private bool _jogadorPerto = false;

	public override void _Ready()
	{
		AreaEntered += OnAreaEntered;
		AreaExited += OnAreaExited;
		CarregarQuestoesDoJson("res://src/dados/questoes.json");
	}

	public override void _Process(double delta)
	{
		if (_jogadorPerto && Input.IsActionJustPressed("ui_accept"))
		{
			PrepararNovaBatalha();
			GD.Print($"Batalha iniciada com {NomeProfessor}! Questões prontas: {_baralhoDisponivel.Count}");
			EmitSignal(SignalName.BatalhaIniciada, this);
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

	private void OnAreaEntered(Area2D area)
	{
		if (area.IsInGroup("Player"))
		{
			_jogadorPerto = true;
			GD.Print("Encostou no prof! Apertar Enter ou Espaço");
		}
			
	}

	private void OnAreaExited(Area2D area)
	{
		if (area.IsInGroup("Player"))
		{
			_jogadorPerto = false;
			GD.Print("Saiu de perto do prof.");
		}
	}
}
