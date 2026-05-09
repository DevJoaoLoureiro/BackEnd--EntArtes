using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("marketplace_anuncios")]
public class MarketplaceAnuncio
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("titulo")]
    public string Titulo { get; set; } = null!;

    [Column("descricao")]
    public string? Descricao { get; set; }

    [Column("categoria")]
    public string? Categoria { get; set; }

    [Column("tamanho")]
    public string? Tamanho { get; set; }

    [Column("preco_aluguer")]
    public decimal PrecoAluguer { get; set; }

    [Column("caucao")]
    public decimal? Caucao { get; set; }

    [Column("estado")]
    public string Estado { get; set; } = "PENDENTE"; // PENDENTE, APROVADO, etc

    [Column("ativo")]
    public bool Ativo { get; set; }

    [Column("dono_utilizador_id")]
    public int DonoUtilizadorId { get; set; }

    [Column("criado_em")]
    public DateTime CriadoEm { get; set; }
}