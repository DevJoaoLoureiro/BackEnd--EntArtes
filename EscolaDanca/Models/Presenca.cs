
namespace EscolaDanca.Models;

public class Presenca
{
    public int Id { get; set; }
    public int SessaoAulaId { get; set; }
    public int AlunoId { get; set; }
    public bool Presente { get; set; }   // 👈 TEM de existir

    public int MarcadoPorUtilizadorId { get; set; }
    public DateTime MarcadoEm { get; set; }
    public string? Observacoes { get; set; }

    public SessaoAula? SessaoAula { get; set; }
    public Aluno? Aluno { get; set; }
}
