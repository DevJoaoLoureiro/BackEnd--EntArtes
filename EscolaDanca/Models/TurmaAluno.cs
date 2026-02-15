

namespace EscolaDanca.Models;

public class TurmaAluno
{
    public int Id { get; set; }
    public int TurmaId { get; set; }
    public int AlunoId { get; set; }
    public DateTime DataInscricao { get; set; }
    public bool Ativo { get; set; } = true;

    public Turma? Turma { get; set; }
    public Aluno? Aluno { get; set; }
}
