using System.ComponentModel.DataAnnotations.Schema;
namespace EscolaDanca.Models;



[Table("turmas")]
public class Turma
{

    [Column("id")]
    public int Id { get; set; }

    [Column("nome")]
    public string Nome { get; set; } = "";

    [Column("nivel")]
    public string? Nivel { get; set; }

    [Column("Capacidade")]

    public int Capacidade { get; set; } = 0;

    [Column("ativa")]
    public bool Ativa { get; set; } = true;

    [Column("criado_em")]
    public DateTime CriadoEm { get; set; }
}
