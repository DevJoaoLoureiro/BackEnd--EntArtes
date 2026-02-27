using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaDanca.Models;

[Table("turma_professores")]
public class TurmaProfessor
{
    [Column("id")]
    public int Id { get; set; }

    [Column("turma_id")]
    public int TurmaId { get; set; }

    [Column("professor_utilizador_id")]
    public int ProfessorUtilizadorId { get; set; }

    [ForeignKey(nameof(TurmaId))]
    public Turma? Turma { get; set; }

    [ForeignKey(nameof(ProfessorUtilizadorId))]
    public Utilizador? Professor { get; set; }
}
