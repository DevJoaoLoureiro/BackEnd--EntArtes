using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("inventario_movimentos")]
public class InventarioMovimento
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("item_id")]
    public int ItemId { get; set; }

    [Column("tipo")]
    public string Tipo { get; set; } = null!; 

    [Column("quantidade")]
    public int Quantidade { get; set; }

    [Column("aluno_id")]
    public int? AlunoId { get; set; }

    [Column("criado_por_utilizador_id")]
    public int CriadoPorUtilizadorId { get; set; }

    [Column("criado_em")]
    public DateTime CriadoEm { get; set; }

    [Column("data_prevista_devolucao")]
    public DateTime? DataPrevistaDevolucao { get; set; }

    [Column("data_devolucao")]
    public DateTime? DataDevolucao { get; set; }

    [Column("valor")]
    public decimal? Valor { get; set; }

    [Column("estado")]
    public string? Estado { get; set; } // ATIVO, DEVOLVIDO, CANCELADO

    [Column("descricao")]
    public string? Descricao { get; set; }
}