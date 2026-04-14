using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaDanca.Models;

[Table("estudios")]
public class Estudio
{
    [Column("id")]
    public int Id { get; set; }

    [Column("nome")]
    public string Nome { get; set; } = "";

    [Column("capacidade")]
    public int? Capacidade { get; set; }
}