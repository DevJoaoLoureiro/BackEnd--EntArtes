using EscolaDanca.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("evento_inscricoes")]
public class EventoInscricao
{
    [Column("id")]
    public int Id { get; set; }

    [Column("evento_id")]
    public int EventoId { get; set; }

    [Column("aluno_id")]
    public int AlunoId { get; set; }

    [Column("inscrito_por_utilizador_id")]

    public int InscritoPorUtilizadorId { get; set; }

    [Column("inscrito_em")]
    public DateTime InscritoEm { get; set; }

    [Column("avaliacao")]
    public int? Avaliacao { get; set; } // 0-5

    public Evento Evento { get; set; } = null!;
    public Aluno Aluno { get; set; } = null!;

    public Utilizador InscritoPorUtilizador { get; set; }
}