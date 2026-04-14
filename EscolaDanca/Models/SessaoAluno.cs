using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaDanca.Models;

[Table("sessao_alunos")]
public class SessaoAluno
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("sessao_aula_id")]
    public int SessaoAulaId { get; set; }

    [Column("aluno_id")]
    public int AlunoId { get; set; }

    [Column("adicionado_por_utilizador_id")]
    public int AdicionadoPorUtilizadorId { get; set; }

    [Column("adicionado_em")]
    public DateTime AdicionadoEm { get; set; }

    [ForeignKey("SessaoAulaId")]
    public SessaoAula? Sessao { get; set; }

    [ForeignKey("AlunoId")]
    public Aluno? Aluno { get; set; }

    [ForeignKey("AdicionadoPorUtilizadorId")]
    public Utilizador? AdicionadoPor { get; set; }
}