namespace EscolaDanca.Models;

public class MarketplaceInteresse
{
    public int Id { get; set; }
    public int AnuncioId { get; set; }
    public int ResponsavelId { get; set; }
    public string Mensagem { get; set; } = "";
    public string Estado { get; set; } = "ENVIADO";
    public DateTime CriadoEm { get; set; }
}
