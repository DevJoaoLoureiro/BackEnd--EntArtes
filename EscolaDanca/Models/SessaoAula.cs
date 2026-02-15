

namespace EscolaDanca.Models;

public class SessaoAula
{
    public int Id { get; set; }
    public int TurmaId { get; set; }
    public int? ProfessorUtilizadorId { get; set; }
    public string TipoAula { get; set; } = "";
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public string Estado { get; set; } = "AGENDADA";
    public int CriadoPorUtilizadorId { get; set; }
    public DateTime CriadoEm { get; set; }

    public Turma? Turma { get; set; }
}
