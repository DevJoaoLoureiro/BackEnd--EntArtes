using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaDanca.Models;

[Table("pagamentos")]
public class Pagamento
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("aluno_id")]
    public int AlunoId { get; set; }

    [ForeignKey(nameof(AlunoId))]
    public Aluno? Aluno { get; set; }

    [Column("referencia")]
    [StringLength(100)]
    public string? Referencia { get; set; }

    [Column("valor", TypeName = "decimal(10,2)")]
    public decimal Valor { get; set; }

    [Column("estado")]
    [StringLength(30)]
    public string Estado { get; set; } = "PENDENTE";

    [Column("data_pagamento")]
    public DateTime? DataPagamento { get; set; }

    [Column("criado_em")]
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // ligação opcional ao coaching/sessão
    [Column("sessao_id")]
    public int? SessaoId { get; set; }

    [ForeignKey(nameof(SessaoId))]
    public SessaoAula? SessaoAula { get; set; }

    // MENSALIDADE / COACHING
    [Column("tipo")]
    [StringLength(30)]
    public string? Tipo { get; set; }

    [Column("descricao")]
    [StringLength(255)]
    public string? Descricao { get; set; }

    // usado para mensalidades
    [Column("mes")]
    public int? Mes { get; set; }

    [Column("ano")]
    public int? Ano { get; set; }
}