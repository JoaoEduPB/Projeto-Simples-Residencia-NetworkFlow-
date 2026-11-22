public enum Dificuldade
{
    Facil,
    Medio,
    Dificil,
    Impossivel
}

public class Questao
{
    public string Enunciado { get; set; } = "";
    public string[] Alternativas { get; set; } = System.Array.Empty<string>();
    public int IndiceCorreta { get; set; }
    public Dificuldade Dificuldade { get; set; }
}