using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaDanca.Models;

[Table("turma_alunos")]
public class TurmaAluno
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("turma_id")]
    public int TurmaId { get; set; }

    [Column("aluno_id")]
    public int AlunoId { get; set; }

    [Column("adicionado_em")]
    public DateTime AdicionadoEm { get; set; }

    [ForeignKey("TurmaId")]
    public Turma? Turma { get; set; }


    [ForeignKey("AlunoId")]
    public Aluno? Aluno { get; set; }
}
