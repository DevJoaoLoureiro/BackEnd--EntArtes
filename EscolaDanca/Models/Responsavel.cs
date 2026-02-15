namespace EscolaDanca.Models;

public class Responsavel
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Morada { get; set; }
    public string? Nif { get; set; }
    public DateTime CriadoEm { get; set; }
}
