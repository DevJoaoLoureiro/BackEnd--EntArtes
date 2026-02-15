
namespace EscolaDanca.Models;

public class DesmarcacaoAula
{
    public int Id { get; set; }
    public int SessaoAulaId { get; set; }
    public int AlunoId { get; set; }
    public int CriadoPorUtilizadorId { get; set; }
    public string Motivo { get; set; } = "";
    public string Estado { get; set; } = "PENDENTE";
    public DateTime CriadoEm { get; set; }

    public SessaoAula? SessaoAula { get; set; }
    public Aluno? Aluno { get; set; }
}
