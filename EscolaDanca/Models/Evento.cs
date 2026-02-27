using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaDanca.Models;

[Table("eventos")]
public class Evento
{

    [Column("id")]
    public int Id { get; set; }

    [Column("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [Column("descricao")]
    public string? Descricao { get; set; }

    [Column("data_inicio")]
    public DateTime DataInicio { get; set; }

    [Column("data_fim")]
    public DateTime? DataFim { get; set; }

    [Column("local")]
    public string? Local { get; set; }

    [Column("publico")]
    public bool Publico { get; set; } = true;


    [Column("criado_por_utilizador_id")]
    [ForeignKey("CriadoPorUtilizador")]
    public int CriadoPorUtilizadorId { get; set; }

    public Utilizador? CriadoPorUtilizador { get; set; }

    [Column("criado_em")]
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

   
}
