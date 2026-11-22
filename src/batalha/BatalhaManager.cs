using Godot;

public partial class BatalhaManager : CanvasLayer
{
    [Export] public Node2D Jogador;
    [Export] public Label LabelPlacar;
    [Export] public TextureRect RetratoJogador;
    [Export] public Texture2D TexturaJogadorFalha;
    [Export] public Label LabelEnunciado;
    [Export] public Label LabelFeedback;
    [Export] public Button[] BotoesAlternativas;
    [Export] public Button BotaoSair;

    private Texture2D _texturaJogadorNormal;
    private Professor _professorAtual;
    private Questao _questaoAtual;
    private int _vidaProfessor;
    private int _vidaJogador = 100;
    private int _combo = 0;

    public override void _Ready()
    {
        Visible = false; // Começa escondida durante a exploração
        BotaoSair.Visible = false;

        if (RetratoJogador != null)
            _texturaJogadorNormal = RetratoJogador.Texture;

        for (int i = 0; i < BotoesAlternativas.Length; i++)
        {
            int indice = i;
            BotoesAlternativas[i].Pressed += () => AoClicarAlternativa(indice);
        }

        BotaoSair.Pressed += FecharBatalha;
    }

    public void IniciarBatalha(Professor professor)
    {
        _professorAtual = professor;
        _vidaProfessor = professor.VidaMaxima;
        _vidaJogador = 100;
        _combo = 0;

        if (RetratoJogador != null && _texturaJogadorNormal != null)
            RetratoJogador.Texture = _texturaJogadorNormal;

        if (Jogador != null)
            Jogador.SetPhysicsProcess(false); // Trava o Protoboard enquanto responde

        BotaoSair.Visible = false;
        LabelFeedback.Text = $"Desafio iniciado contra {professor.NomeProfessor}!";
        Visible = true;

        CarregarProximaPergunta();
    }

    private void CarregarProximaPergunta()
    {
        AtualizarPlacar();
        _questaoAtual = _professorAtual.SortearProximaQuestao();

        if (_questaoAtual == null)
        {
            EncerrarBatalha("Fim das questões do banco! Empate técnico.");
            return;
        }

        if (RetratoJogador != null && _texturaJogadorNormal != null)
            RetratoJogador.Texture = _texturaJogadorNormal;

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
            _combo++;
            int danoNoProfessor = 15 + (_combo * 5);
            _vidaProfessor = Mathf.Max(0, _vidaProfessor - danoNoProfessor);
            LabelFeedback.Text = $"Acertou! Combo {_combo}x (-{danoNoProfessor} HP no professor)";
        }
        else
        {
            _combo = 0;
            int danoNoJogador = _questaoAtual.Dificuldade switch
            {
                Dificuldade.Facil => 10,
                Dificuldade.Medio => 15,
                Dificuldade.Dificil => 25,
                Dificuldade.Impossivel => 35,
                _ => 15
            };
            _vidaJogador = Mathf.Max(0, _vidaJogador - danoNoJogador);

            if (RetratoJogador != null && TexturaJogadorFalha != null)
                RetratoJogador.Texture = TexturaJogadorFalha;

            string respostaCerta = _questaoAtual.Alternativas[_questaoAtual.IndiceCorreta];
            LabelFeedback.Text = $"Errou! (-{danoNoJogador} HP) | Correta: {respostaCerta}";
        }

        AtualizarPlacar();

        // Pausa de 1.2s para ler o resultado antes da próxima pergunta
        await ToSignal(GetTree().CreateTimer(1.2), SceneTreeTimer.SignalName.Timeout);

        if (_vidaProfessor <= 0)
        {
            EncerrarBatalha($"Vitória! Você derrotou {_professorAtual.NomeProfessor}!");
        }
        else if (_vidaJogador <= 0)
        {
            EncerrarBatalha("Derrota! Revise a camada e tente novamente.");
        }
        else
        {
            CarregarProximaPergunta();
        }
    }

    private void AtualizarPlacar()
    {
        LabelPlacar.Text =
            $"{_professorAtual.NomeProfessor}: {_vidaProfessor} HP   |   Protoboard: {_vidaJogador} HP   |   Combo: {_combo}x";
    }

    private void EncerrarBatalha(string mensagemFinal)
    {
        LabelEnunciado.Text = mensagemFinal;
        foreach (var botao in BotoesAlternativas)
            botao.Visible = false;

        BotaoSair.Visible = true;
    }

    private void FecharBatalha()
    {
        Visible = false;
        if (Jogador != null)
            Jogador.SetPhysicsProcess(true); // Libera o movimento do Protoboard
    }

    private void DefinirBotoesAtivos(bool ativo)
    {
        foreach (var botao in BotoesAlternativas)
            botao.Disabled = !ativo;
    }
}