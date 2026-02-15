

namespace EscolaDanca.Models;

public class AlunoResponsavel
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public int ResponsavelId { get; set; }
    public string? Parentesco { get; set; }
    public bool IsPrincipal { get; set; }
    public DateTime CriadoEm { get; set; }

    public Aluno? Aluno { get; set; }
    public Responsavel? Responsavel { get; set; }
}
