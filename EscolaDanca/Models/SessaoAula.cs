using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaDanca.Models;

[Table("sessoes_aula")]
public class SessaoAula
{
    [Column("id")]
    public int Id { get; set; }

    [Column("professor_utilizador_id")]
    public int ProfessorUtilizadorId { get; set; }

    [Column("tipo_aula")]
    public string TipoAula { get; set; } = "AULA";

    [Column("Titulo")]
    public string Titulo { get; set; } = "";

    [Column("Descricao")]
    public string? Descricao { get; set; }

    [Column("inicio")]
    public DateTime Inicio { get; set; }

    [Column("fim")]
    public DateTime Fim { get; set; }

    [Column("criado_por_utilizador_id")]
    public int CriadoPorUtilizadorId { get; set; }

    [Column("criado_em")]
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ProfessorUtilizadorId))]
    public Utilizador? Professor { get; set; }

    [ForeignKey(nameof(CriadoPorUtilizadorId))]
    public Utilizador? CriadoPor { get; set; }

    public ICollection<ConfirmacaoPresenca> Confirmacoes { get; set; } = new List<ConfirmacaoPresenca>();
    public ICollection<Presenca> Presencas { get; set; } = new List<Presenca>();
}