using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("marketplace_alugueres")]
public class MarketplaceAluguer
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("anuncio_id")]
    public int AnuncioId { get; set; }

    [Column("aluno_id")]
    public int? AlunoId { get; set; }

    [Column("alugado_por_utilizador_id")]
    public int AlugadoPorUtilizadorId { get; set; }

    [Column("data_inicio")]
    public DateTime DataInicio { get; set; }

    [Column("data_fim_prevista")]
    public DateTime? DataFimPrevista { get; set; }

    [Column("data_devolucao")]
    public DateTime? DataDevolucao { get; set; }

    [Column("valor")]
    public decimal Valor { get; set; }

    [Column("estado")]
    public string Estado { get; set; } = "ATIVO"; // ATIVO, DEVOLVIDO, CANCELADO

    [Column("criado_em")]
    public DateTime CriadoEm { get; set; }
}