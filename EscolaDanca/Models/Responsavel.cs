using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaDanca.Models;

[Table("responsaveis")]
public class Responsavel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nome")]
    public string Nome { get; set; } = "";

    [Column("telefone")]
    public string? Telefone { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("morada")]
    public string? Morada { get; set; }

    [Column("nif")]
    public string? Nif { get; set; }

    [Column("criado_em")]
    public DateTime ? CriadoEm { get; set; }

    [Column("utilizador_id")]
    public int? UtilizadorId { get; set; }

    [ForeignKey("UtilizadorId")]
    public Utilizador? Utilizador { get; set; }
}