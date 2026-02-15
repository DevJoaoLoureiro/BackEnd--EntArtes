using EscolaDanca.Models;

namespace EscolaDanca.Api.Models;

public class Notificacao
{
    public int Id { get; set; }
    public int UtilizadorId { get; set; }
    public string Tipo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Mensagem { get; set; } = "";
    public string? Link { get; set; }
    public bool Lida { get; set; } = false;
    public DateTime CriadaEm { get; set; }

    public Utilizador? Utilizador { get; set; }
}
