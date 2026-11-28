using Godot;

public partial class BatalhaManager : CanvasLayer
{
	[Export] public Node2D Jogador;
	[Export] public Label LabelPlacar;
	[Export] public ProgressBar BarraProfessor;
	[Export] public ProgressBar BarraJogador;
	[Export] public TextureRect RetratoJogador;
	[Export] public Texture2D TexturaJogadorFalha;
	[Export] public Label LabelEnunciado;
	[Export] public Label LabelFeedback;
	[Export] public Label LabelTempo;
	[Export] public Label LabelProgresso;
	[Export] public Control FundoBaixo;
	[Export] public Node AnimacaoJogador;
	[Export] public string NomeAnimacaoNormal = "normal";
	[Export] public string NomeAnimacaoErro = "erro";
	[Export] public Node AnimacaoProfessor;
	[Export] public string NomeAnimacaoProfessorNormal = "normal";
	[Export] public string NomeAnimacaoProfessorErro = "erro";
	[Export] public Button[] BotoesAlternativas;
	[Export] public Button BotaoSair;

	[Export] public int AcertosParaVencer = 10;
	[Export] public int ErrosPermitidos = 3;
	[Export] public float TempoPorPergunta = 15f;

	private Texture2D _texturaJogadorNormal;
	private string _nomeProfessor;
	private Questao _questaoAtual;
	private int _vidaProfessor;
	private int _vidaJogador;

	private float _tempoRestante;
	private bool _contando;
	private int _numeroPergunta;

	private int TotalPerguntas => AcertosParaVencer;

	private static readonly Color CorAcerto = new Color("#3ddc84");
	private static readonly Color CorErro = new Color("#ff4d4d");

	public override void _Ready()
	{
		Visible = false;
		GD.Print("BatalhaManager: _Ready executado (versão com diagnóstico de animação).");

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

		if (LabelTempo == null || LabelProgresso == null)
			GD.PrintErr("BatalhaManager: ligue LabelTempo e LabelProgresso no Inspector.");

		if (AnimacaoJogador == null)
			GD.PrintErr("BatalhaManager: ligue AnimacaoJogador (AnimatedSprite2D ou AnimationPlayer) no Inspector.");

		if (AnimacaoProfessor == null)
			GD.PrintErr("BatalhaManager: ligue AnimacaoProfessor (AnimatedSprite2D ou AnimationPlayer) no Inspector.");

		BotaoSair.Visible = false;
		BotaoSair.Pressed += FecharBatalha;

		if (RetratoJogador != null)
			_texturaJogadorNormal = RetratoJogador.Texture;

		for (int i = 0; i < BotoesAlternativas.Length; i++)
		{
			int indice = i;
			BotoesAlternativas[i].Pressed += () => ResolverResposta(indice);
		}

		EstilizarBotoes();
		EstilizarFundo();
		EstilizarFeedback();

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

	public override void _Process(double delta)
	{
		if (!_contando) return;

		_tempoRestante -= (float)delta;

		if (_tempoRestante <= 0f)
		{
			_tempoRestante = 0f;
			AtualizarTempo();
			ResolverResposta(-1);
			return;
		}

		AtualizarTempo();
	}

	public void IniciarBatalha()
	{
		_nomeProfessor = GameManager.Instance.NomeProfessor;
		_vidaProfessor = AcertosParaVencer;
		_vidaJogador = ErrosPermitidos;
		_numeroPergunta = 0;
		_contando = false;

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

		if (RetratoJogador != null && _texturaJogadorNormal != null)
			RetratoJogador.Texture = _texturaJogadorNormal;

		IsolarAnimacao(AnimacaoJogador);
		IsolarAnimacao(AnimacaoProfessor);
		TocarAnimacaoJogador(NomeAnimacaoNormal);
		TocarAnimacaoProfessor(NomeAnimacaoProfessorNormal);

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

		if (RetratoJogador != null && _texturaJogadorNormal != null)
			RetratoJogador.Texture = _texturaJogadorNormal;

		TocarAnimacaoJogador(NomeAnimacaoNormal);
		TocarAnimacaoProfessor(NomeAnimacaoProfessorNormal);

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

		_numeroPergunta++;
		if (LabelProgresso != null)
			LabelProgresso.Text = $"{Mathf.Min(_numeroPergunta, TotalPerguntas)}/{TotalPerguntas}";

		_tempoRestante = TempoPorPergunta;
		_contando = true;
		AtualizarTempo();
	}

	private async void ResolverResposta(int indiceEscolhido)
	{
		if (_questaoAtual == null || !_contando) return;

		_contando = false;
		DefinirBotoesAtivos(false);

		if (indiceEscolhido == _questaoAtual.IndiceCorreta)
		{
			_vidaProfessor--;
			LabelFeedback.Text = "Acertou!";
			LabelFeedback.AddThemeColorOverride("font_color", CorAcerto);
			AnimarBarra(BarraProfessor, _vidaProfessor);
		}
		else
		{
			_vidaJogador--;
			LabelFeedback.Text = indiceEscolhido == -1 ? "Tempo esgotado!" : "Errou!";
			LabelFeedback.AddThemeColorOverride("font_color", CorErro);
			TocarAnimacaoJogador(NomeAnimacaoErro);
			TocarAnimacaoProfessor(NomeAnimacaoProfessorErro);
			AnimarBarra(BarraJogador, _vidaJogador);

			if (RetratoJogador != null && TexturaJogadorFalha != null)
				RetratoJogador.Texture = TexturaJogadorFalha;
		}

		await ToSignal(GetTree().CreateTimer(1.2), SceneTreeTimer.SignalName.Timeout);

		if (_vidaProfessor <= 0)
			EncerrarBatalha($"Vitória! Você derrotou {_nomeProfessor}!");
		else if (_vidaJogador <= 0)
			EncerrarBatalha("Derrota! Revise a camada e tente novamente.");
		else
			CarregarProximaPergunta();
	}

	private void AtualizarTempo()
	{
		if (LabelTempo == null) return;

		LabelTempo.Text = $"{Mathf.CeilToInt(_tempoRestante):D2}s";
		LabelTempo.AddThemeColorOverride("font_color",
			_tempoRestante <= 5f ? Colors.Red : Colors.White);
	}

	private void AnimarBarra(ProgressBar barra, int valor)
	{
		if (barra == null) return;
		CreateTween().TweenProperty(barra, "value", (double)valor, 0.3);
	}

	private void EncerrarBatalha(string mensagemFinal)
	{
		_contando = false;
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

	private void DefinirBotoesAtivos(bool ativo)
	{
		foreach (var botao in BotoesAlternativas)
			botao.Disabled = !ativo;
	}

	private static StyleBoxFlat CriarEstilo(Color cor)
	{
		var estilo = new StyleBoxFlat { BgColor = cor };
		estilo.SetCornerRadiusAll(8);
		estilo.SetContentMarginAll(12);
		return estilo;
	}

	private static void AplicarEstiloBotao(Button botao, string normal, string hover, string pressed, string disabled)
	{
		botao.AddThemeStyleboxOverride("normal", CriarEstilo(new Color(normal)));
		botao.AddThemeStyleboxOverride("hover", CriarEstilo(new Color(hover)));
		botao.AddThemeStyleboxOverride("pressed", CriarEstilo(new Color(pressed)));
		botao.AddThemeStyleboxOverride("disabled", CriarEstilo(new Color(disabled)));
	}

	private void EstilizarBotoes()
	{
		foreach (var botao in BotoesAlternativas)
			AplicarEstiloBotao(botao, "#3b4a6b", "#4d5f8a", "#2c3850", "#2a3040");

		if (BotaoSair != null)
			AplicarEstiloBotao(BotaoSair, "#2e7d4f", "#3a9962", "#236340", "#1f3d2d");
	}

	private void EstilizarFundo()
	{
		if (FundoBaixo == null)
		{
			GD.PrintErr("BatalhaManager: ligue FundoBaixo no Inspector.");
			return;
		}

		var fundo = new StyleBoxFlat { BgColor = new Color("#1a1f2e") };
		fundo.SetCornerRadiusAll(0);
		fundo.SetContentMarginAll(16);

		if (FundoBaixo is ColorRect retangulo)
			retangulo.Color = new Color("#1a1f2e");
		else
			FundoBaixo.AddThemeStyleboxOverride("panel", fundo);
	}

	private void IsolarAnimacao(Node alvo)
	{
		if (alvo?.GetParent() is Node pai && pai != this)
		{
			pai.SetProcess(false);
			pai.SetPhysicsProcess(false);
			pai.SetProcessInput(false);
			pai.SetProcessUnhandledInput(false);
		}
	}

	private void EstilizarFeedback()
	{
		if (LabelFeedback == null) return;

		LabelFeedback.AddThemeConstantOverride("outline_size", 8);
		LabelFeedback.AddThemeColorOverride("font_outline_color", Colors.White);
	}

	private void TocarAnimacaoJogador(string nome) => TocarAnimacao(AnimacaoJogador, nome, "AnimacaoJogador");

	private void TocarAnimacaoProfessor(string nome) => TocarAnimacao(AnimacaoProfessor, nome, "AnimacaoProfessor");

	private void TocarAnimacao(Node alvo, string nome, string campo)
	{
		switch (alvo)
		{
			case AnimatedSprite2D sprite:
				if (sprite.SpriteFrames == null || !sprite.SpriteFrames.HasAnimation(nome))
				{
					GD.PrintErr($"BatalhaManager: a animação '{nome}' não existe no AnimatedSprite2D '{sprite.Name}' ({campo}).");
					return;
				}
				GD.Print($"BatalhaManager: {campo} tocando '{nome}' ({sprite.SpriteFrames.GetFrameCount(nome)} quadros).");
				sprite.Play(nome);
				break;

			case AnimationPlayer player:
				if (!player.HasAnimation(nome))
				{
					GD.PrintErr($"BatalhaManager: a animação '{nome}' não existe no AnimationPlayer '{player.Name}' ({campo}).");
					return;
				}
				GD.Print($"BatalhaManager: {campo} tocando '{nome}'.");
				player.Play(nome);
				break;

			case null:
				GD.PrintErr($"BatalhaManager: {campo} não está ligado no Inspector.");
				break;

			default:
				GD.PrintErr($"BatalhaManager: {campo} é do tipo {alvo.GetType().Name}, use AnimatedSprite2D ou AnimationPlayer.");
				break;
		}
	}
}
