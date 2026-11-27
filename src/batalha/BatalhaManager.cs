using Godot;

public partial class BatalhaManager : CanvasLayer
{
	[Export] public Node2D Jogador;
	[Export] public Label LabelPlacar; // agora só o nome do professor
	[Export] public ProgressBar BarraProfessor;
	[Export] public AnimatedSprite2D AnimJogador;
	[Export] public AnimatedSprite2D AnimProfessor;
	[Export] public ProgressBar BarraJogador;
	[Export] public Label LabelEnunciado;
	[Export] public Label LabelFeedback;
	[Export] public Button[] BotoesAlternativas;
	[Export] public Button BotaoSair;

	[Export] public int AcertosParaVencer = 10;
	[Export] public int ErrosPermitidos = 3;
	private string _nomeProfessor;
	private Questao _questaoAtual;
	private int _vidaProfessor;
	private int _vidaJogador;

	public override void _Ready()
	{
		Visible = true;

		if (LabelEnunciado == null || LabelFeedback == null || BotaoSair == null)
		{
			GD.PrintErr("BatalhaManager: ligue LabelEnunciado, LabelFeedback e BotaoSair no Inspector.");
			return;
		}

		if (BotoesAlternativas == null || BotoesAlternativas.Length == 0)
		{
			GD.PrintErr("BatalhaManager: o array BotoesAlternativas está vazio no Inspector.");
			return;
		}

		if (BarraProfessor == null || BarraJogador == null)
			GD.PrintErr("BatalhaManager: ligue BarraProfessor e BarraJogador no Inspector.");

		BotaoSair.Visible = false;
		BotaoSair.Pressed += FecharBatalha;

		for (int i = 0; i < BotoesAlternativas.Length; i++)
		{
			int indice = i;
			BotoesAlternativas[i].Pressed += () => AoClicarAlternativa(indice);
		}

		var gm = GameManager.Instance;
		if (gm == null)
		{
			GD.PrintErr("BatalhaManager: GameManager não encontrado. Registre-o em Project Settings > Globals > Autoload.");
			return;
		}

		if (gm.ConsumirBatalhaPendente())
			IniciarBatalha();
		else
			GD.Print("BatalhaManager: nenhuma batalha pendente.");
	}

	public void IniciarBatalha()
	{
		_nomeProfessor = GameManager.Instance.NomeProfessor;
		_vidaProfessor = AcertosParaVencer;
		_vidaJogador = ErrosPermitidos;

		if (LabelPlacar != null)
			LabelPlacar.Text = _nomeProfessor;

		if (BarraProfessor != null)
		{
			BarraProfessor.MaxValue = AcertosParaVencer;
			BarraProfessor.Value = AcertosParaVencer;
		}

		if (BarraJogador != null)
		{
			BarraJogador.MaxValue = ErrosPermitidos;
			BarraJogador.Value = ErrosPermitidos;
		}

		Tocar(AnimJogador, "idle");
		Tocar(AnimProfessor, "idle");

		if (Jogador != null)
			Jogador.SetPhysicsProcess(false);

		BotaoSair.Visible = false;
		LabelFeedback.Text = "";
		Visible = true;

		CarregarProximaPergunta();
	}

	private void CarregarProximaPergunta()
	{
		_questaoAtual = GameManager.Instance.ProximaQuestao();

		if (_questaoAtual == null)
		{
			EncerrarBatalha("Acabaram as questões! Empate técnico.");
			return;
		}

		Tocar(AnimJogador, "idle");
		Tocar(AnimProfessor, "idle");

		LabelFeedback.Text = "";
		LabelEnunciado.Text = $"[{_questaoAtual.Dificuldade}] {_questaoAtual.Enunciado}";
		DefinirBotoesAtivos(true);

		for (int i = 0; i < BotoesAlternativas.Length; i++)
		{
			if (i < _questaoAtual.Alternativas.Length)
			{
				BotoesAlternativas[i].Text = _questaoAtual.Alternativas[i];
				BotoesAlternativas[i].Visible = true;
			}
			else
			{
				BotoesAlternativas[i].Visible = false;
			}
		}
	}

	private async void AoClicarAlternativa(int indiceEscolhido)
	{
		if (_questaoAtual == null) return;

		DefinirBotoesAtivos(false);

		if (indiceEscolhido == _questaoAtual.IndiceCorreta)
		{
			_vidaProfessor--;
			LabelFeedback.Text = "Acertou!";
			AnimarBarra(BarraProfessor, _vidaProfessor);
			Tocar(AnimProfessor, "dano");
		}
		else
		{
			_vidaJogador--;
			LabelFeedback.Text = "Errou!";
			AnimarBarra(BarraJogador, _vidaJogador);
			Tocar(AnimJogador, "falha");
		}

		await ToSignal(GetTree().CreateTimer(1.2), SceneTreeTimer.SignalName.Timeout);

		if (_vidaProfessor <= 0)
			EncerrarBatalha($"Vitória! Você derrotou {_nomeProfessor}!");
		else if (_vidaJogador <= 0)
			EncerrarBatalha("Derrota! Revise a camada e tente novamente.");
		else
			CarregarProximaPergunta();
	}

	private void AnimarBarra(ProgressBar barra, int valor)
	{
		if (barra == null) return;
		CreateTween().TweenProperty(barra, "value", (double)valor, 0.3);
	}

	private void EncerrarBatalha(string mensagemFinal)
	{
		LabelEnunciado.Text = mensagemFinal;
		LabelFeedback.Text = "";
		foreach (var botao in BotoesAlternativas)
			botao.Visible = false;

		BotaoSair.Visible = true;
	}

	private void FecharBatalha()
	{
		string cena = GameManager.Instance?.CenaRetorno;

		if (!string.IsNullOrEmpty(cena))
			GetTree().ChangeSceneToFile(cena);
		else
			Visible = false;
	}

	private void Tocar(AnimatedSprite2D anim, string nome)
	{
		if (anim?.SpriteFrames != null && anim.SpriteFrames.HasAnimation(nome))
			anim.Play(nome);
	}

	private void DefinirBotoesAtivos(bool ativo)
	{
		foreach (var botao in BotoesAlternativas)
			botao.Disabled = !ativo;
	}
}
