using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaDanca.Models;

[Table("presencas")]
public class Presenca
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("sessao_aula_id")]
    public int SessaoAulaId { get; set; }

    [Column("aluno_id")]
    public int AlunoId { get; set; }

    [Column("marcado_por_utilizador_id")]
    public int MarcadoPorUtilizadorId { get; set; }

    [Column("marcado_em")]
    public DateTime? MarcadoEm { get; set; }

    [Column("presente")]
    public bool Presente { get; set; }

    [ForeignKey("SessaoAulaId")]
    public SessaoAula? Sessao { get; set; }

    [ForeignKey("AlunoId")]
    public Aluno? Aluno { get; set; }

    [ForeignKey("MarcadoPorUtilizadorId")]
    public Utilizador? MarcadoPor { get; set; }


    [Column("validado")]
    public bool? Validado { get; set; }

    [Column("validado_por_utilizador_id")]
    public int? ValidadoPorUtilizadorId { get; set; }

    [Column("validado_em")]
    public DateTime? ValidadoEm { get; set; }

    [ForeignKey("ValidadoPorUtilizadorId")]
    public Utilizador? ValidadoPor { get; set; }
}