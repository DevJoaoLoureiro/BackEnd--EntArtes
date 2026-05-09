using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("inventario_itens")]
public class InventarioItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nome")]
    public string Nome { get; set; } = null!;

    [Column("categoria")]
    public string? Categoria { get; set; }

    [Column("tamanho")]
    public string? Tamanho { get; set; }

    [Column("quantidade_total")]
    public int QuantidadeTotal { get; set; }

    [Column("quantidade_disponivel")]
    public int QuantidadeDisponivel { get; set; }

    [Column("preco_aluguer")]
    public decimal? PrecoAluguer { get; set; }

    [Column("ativo")]
    public bool Ativo { get; set; }

    [Column("criado_em")]
    public DateTime CriadoEm { get; set; }

    [Column("localizacao")]
    public string? Localizacao { get; set; }

    [Column("imagem_url")]
    public string? ImagemUrl { get; set; }

}