namespace EscolaDanca.DTOs;

public class EventoRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string? Local { get; set; }
    public bool Publico { get; set; } = true;
}
