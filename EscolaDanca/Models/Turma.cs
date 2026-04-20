

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaDanca.Models;

[Table("turmas")]
public class Turma
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nome")]
    public string Nome { get; set; } = null!;

    [Column("professor_utilizador_id")]
    public int? ProfessorUtilizadorId { get; set; }

    [Column("tipo_aula_id")]
    public int? TipoAulaId { get; set; }

    [Column("ativa")]
    public bool Ativa { get; set; }

    [Column("criada_em")]
    public DateTime CriadaEm { get; set; }

    [ForeignKey("ProfessorUtilizadorId")]
    public Utilizador? Professor { get; set; }

    [ForeignKey("TipoAulaId")]
    public TipoAula? TipoAula { get; set; }

    public ICollection<TurmaAluno> TurmaAlunos { get; set; } = new List<TurmaAluno>();
}