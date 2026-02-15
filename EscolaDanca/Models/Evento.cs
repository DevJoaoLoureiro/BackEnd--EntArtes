namespace EscolaDanca.Models;

public class Evento
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string? Descricao { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Local { get; set; }
    public bool Publico { get; set; } = true;
    public int CriadoPorUtilizadorId { get; set; }
    public DateTime CriadoEm { get; set; }
}
