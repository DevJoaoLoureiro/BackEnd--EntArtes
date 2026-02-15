using System.ComponentModel.DataAnnotations.Schema;
namespace EscolaDanca.Models;


[Table("alunos")]
public class Aluno
{

    [Column("id")]
    public int Id { get; set; }

    [Column("nome")]
    public string Nome { get; set; } = "";
    [Column("data_nascimento")]
    public DateOnly DataNascimento { get; set; }
    [Column("telefone")]
    public string? Telefone { get; set; }
    [Column("email")]
    public string? Email { get; set; }
    [Column("ativo")]
    public bool Ativo { get; set; } = true;
    [Column("criado_em")]
    public DateTime CriadoEm { get; set; }
}


