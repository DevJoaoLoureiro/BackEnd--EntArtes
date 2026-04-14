

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaDanca.Models;

[Table("aluno_responsaveis")]
public class AlunoResponsavel
{

    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("aluno_id")]
    public int AlunoId { get; set; }

    [Column("responsavel_id")]
    public int ResponsavelId { get; set; }

    [Column("parentesco")]
    public string? Parentesco { get; set; }

    [Column("is_principal")]
    public bool IsPrincipal { get; set; }
    [Column("criado_em")]
    public DateTime CriadoEm { get; set; }

    [ForeignKey("AlunoId")]
    public Aluno? Aluno { get; set; }

    [ForeignKey("ResponsavelId")]
    public Responsavel? Responsavel { get; set; }
}