using EscolaDanca.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("evento_professores")]
public class EventoProfessor
{

    [Column("id")]
    public int Id { get; set; }


    [Column("evento_id")]
    public int EventoId { get; set; }


    [Column("professor_utilizador_id")]
    public int ProfessorUtilizadorId { get; set; }

    public Evento Evento { get; set; } = null!;


    [ForeignKey("ProfessorUtilizadorId")]
    public Utilizador Professor { get; set; } = null!;
}