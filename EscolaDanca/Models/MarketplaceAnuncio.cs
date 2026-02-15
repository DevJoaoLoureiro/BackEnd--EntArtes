namespace EscolaDanca.Models;

public class MarketplaceAnuncio
{
    public int Id { get; set; }
    public int ResponsavelId { get; set; }
    public string Titulo { get; set; } = "";
    public string? Descricao { get; set; }
    public decimal Preco { get; set; }
    public string? Categoria { get; set; }
    public string? Tamanho { get; set; }
    public string Estado { get; set; } = "ATIVO";
    public DateTime CriadoEm { get; set; }
}
