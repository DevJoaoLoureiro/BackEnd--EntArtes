

using EscolaDanca.Api.Models;

namespace EscolaDanca.Models;

public class TurmaProfessor
{
    public int Id { get; set; }
    public int TurmaId { get; set; }
    public int ProfessorUtilizadorId { get; set; }

    public Turma? Turma { get; set; }
    public Utilizador? Professor { get; set; }
}
