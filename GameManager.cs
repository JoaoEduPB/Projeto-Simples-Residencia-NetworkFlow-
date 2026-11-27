using Godot;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public readonly record struct ResultadoResposta(bool Acertou, int IndiceCorreta, int Dano);

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	private const string CaminhoQuestoes = "res://src/dados/questoes.json";

	private Dictionary<string, List<Questao>> _banco = new();
	private List<Questao> _baralho = new();

	public string NomeProfessor { get; private set; }
	public string CamadaAtual { get; private set; }
	public int VidaMaximaProfessor { get; private set; }
	public int VidaProfessor { get; private set; }
	public Questao QuestaoAtual { get; private set; }
	public int Acertos { get; private set; }
	public int Erros { get; private set; }

	// NOVO: controle da batalha pendente entre cenas
	public bool BatalhaPendente { get; private set; }
	public string CenaRetorno { get; private set; }

	public bool ProfessorDerrotado => VidaProfessor <= 0;
	public bool SemQuestoes => _baralho.Count == 0;

	public override void _Ready()
	{
		Instance = this;
		CarregarBanco();
	}

	private void CarregarBanco()
	{
		if (!FileAccess.FileExists(CaminhoQuestoes))
		{
			GD.PrintErr($"Arquivo não encontrado: {CaminhoQuestoes}");
			return;
		}

		var opcoes = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			Converters = { new JsonStringEnumConverter() }
		};

		string json = FileAccess.GetFileAsString(CaminhoQuestoes);
		_banco = JsonSerializer.Deserialize<Dictionary<string, List<Questao>>>(json, opcoes) ?? new();
	}

	// ALTERADO: recebe também a cena para onde voltar e marca a batalha como pendente
	public bool IniciarBatalha(string nomeProfessor, string camada, int vidaMaxima, string cenaRetorno = null)
	{
		if (!_banco.TryGetValue(camada, out var questoes))
		{
			GD.PrintErr($"Camada '{camada}' não encontrada no JSON.");
			return false;
		}

		NomeProfessor = nomeProfessor;
		CamadaAtual = camada;
		VidaMaximaProfessor = vidaMaxima;
		VidaProfessor = vidaMaxima;
		Acertos = 0;
		Erros = 0;
		QuestaoAtual = null;
		_baralho = new List<Questao>(questoes);

		CenaRetorno = cenaRetorno;   // NOVO
		BatalhaPendente = true;      // NOVO
		return true;
	}

	// NOVO: o BatalhaManager chama isso ao carregar; devolve true uma única vez
	public bool ConsumirBatalhaPendente()
	{
		bool pendente = BatalhaPendente;
		BatalhaPendente = false;
		return pendente;
	}

	// Sorteia sem repetir. Retorna null se acabaram as questões.
	public Questao ProximaQuestao()
	{
		if (_baralho.Count == 0)
		{
			QuestaoAtual = null;
			return null;
		}

		int i = GD.RandRange(0, _baralho.Count - 1);
		QuestaoAtual = _baralho[i];
		_baralho.RemoveAt(i);
		return QuestaoAtual;
	}

	public ResultadoResposta Responder(int indiceEscolhido)
	{
		if (QuestaoAtual == null)
			return new ResultadoResposta(false, -1, 0);

		bool acertou = indiceEscolhido == QuestaoAtual.IndiceCorreta;
		int dano = 0;

		if (acertou)
		{
			dano = DanoPorDificuldade(QuestaoAtual.Dificuldade);
			VidaProfessor = Mathf.Max(0, VidaProfessor - dano);
			Acertos++;
		}
		else
		{
			Erros++;
		}

		return new ResultadoResposta(acertou, QuestaoAtual.IndiceCorreta, dano);
	}

	private static int DanoPorDificuldade(Dificuldade d) => d switch
	{
		Dificuldade.Facil => 10,
		Dificuldade.Medio => 15,
		Dificuldade.Dificil => 20,
		Dificuldade.Impossivel => 30,
		_ => 10
	};
}
