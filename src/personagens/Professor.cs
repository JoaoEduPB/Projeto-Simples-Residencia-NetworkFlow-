using Godot;

public partial class Professor : Area2D
{
	[Export]
	public string NomeProfessor { get; set; } = "Prof. Marcus";

	[Export]
	public string CamadaChave { get; set; } = "Fisica";

	[Export]
	public int VidaMaxima { get; set; } = 100;

	[Export(PropertyHint.File, "*.tscn")]
	public string CenaDestino { get; set; } = "res://src/fases/CenaBatalha.tscn";

	private bool _jogadorPerto = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	public override void _Process(double delta)
	{
		if (!_jogadorPerto || !Input.IsActionJustPressed("ui_accept"))
			return;

		if (GameManager.Instance == null)
		{
			GD.PrintErr("GameManager não encontrado. Registre-o em Project Settings > Globals > Autoload.");
			return;
		}

		string cenaAtual = GetTree().CurrentScene.SceneFilePath;

		if (!GameManager.Instance.IniciarBatalha(NomeProfessor, CamadaChave, VidaMaxima, cenaAtual))
			return;

		_jogadorPerto = false;
		GD.Print($"Batalha iniciada com {NomeProfessor}!");

		var erro = GetTree().ChangeSceneToFile(CenaDestino);
		if (erro != Error.Ok)
			GD.PrintErr($"Falha ao trocar de cena: {erro}");
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Protoboard)
		{
			_jogadorPerto = true;
			GD.Print("Encostou no prof! Aperte Enter ou Espaço.");
		}
	}

	private void OnBodyExited(Node2D body)
	{
		if (body is Protoboard)
		{
			_jogadorPerto = false;
			GD.Print("Saiu de perto do prof.");
		}
	}
}
