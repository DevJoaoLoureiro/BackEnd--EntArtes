using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaDanca.Models;

[Table("sessoes_aula")]
public class SessaoAula
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("professor_utilizador_id")]
    public int? ProfessorUtilizadorId { get; set; }

    [Column("inicio")]
    public DateTime Inicio { get; set; }

    [Column("fim")]
    public DateTime Fim { get; set; }

    [Column("estado")]
    public string? Estado { get; set; }

    [Column("criado_por_utilizador_id")]
    public int CriadoPorUtilizadorId { get; set; }

    [Column("criado_em")]
    public DateTime? CriadoEm { get; set; }

    [Column("tipo_aula_id")]
    public int? TipoAulaId { get; set; }

    [Column("estudio_id")]
    public int? EstudioId { get; set; }

    [Column("max_alunos")]
    public int? MaxAlunos { get; set; }


    [Column("sumario")]
    public string? Sumario { get; set; }

    [Column("foi_dada")]
    public bool FoiDada { get; set; }

    [Column("inscricao_aberta")]
    public bool InscricaoAberta { get; set; }

    [Column("preco_coaching")]
    public decimal? PrecoCoaching { get; set; }

    [Column("motivo_falta_professor")]
    public string? MotivoFaltaProfessor { get; set; }

    // =====================
    // NAVIGATION PROPERTIES
    // =====================

    [ForeignKey("ProfessorUtilizadorId")]
    public Utilizador? Professor { get; set; }

    [ForeignKey("CriadoPorUtilizadorId")]
    public Utilizador? CriadoPor { get; set; }

    [ForeignKey("TipoAulaId")]
    public TipoAula? TipoAula { get; set; }

    [ForeignKey("EstudioId")]
    public Estudio? Estudio { get; set; }

    [Column("turma_id")]
    public int? TurmaId { get; set; }

    [ForeignKey("TurmaId")]
    public Turma? Turma { get; set; }
    public ICollection<Presenca> Presencas { get; set; } = new List<Presenca>();
    public ICollection<ConfirmacaoPresenca> Confirmacoes { get; set; } = new List<ConfirmacaoPresenca>();
}