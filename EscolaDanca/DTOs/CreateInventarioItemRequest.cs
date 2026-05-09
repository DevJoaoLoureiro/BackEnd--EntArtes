


namespace EscolaDanca.DTOs;

public class CreateInventarioItemRequest
{
    public string Nome { get; set; } = null!;
    public string? Categoria { get; set; }
    public string? Tamanho { get; set; }
    public int QuantidadeTotal { get; set; }
    public int QuantidadeDisponivel { get; set; }
    public decimal? PrecoAluguer { get; set; }
    public string? Localizacao { get; set; }
    public IFormFile? Imagem { get; set; }
}