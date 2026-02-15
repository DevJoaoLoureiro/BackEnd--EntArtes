namespace EscolaDanca.Models;

public class Turma
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string? Nivel { get; set; }
    public bool Ativa { get; set; } = true;
    public DateTime CriadoEm { get; set; }
}
