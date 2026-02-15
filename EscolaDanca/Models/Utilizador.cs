using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaDanca.Models;

[Table("utilizadores")]
public class Utilizador
{
    [Column("id")]
    public int Id { get; set; }

    [Column("nome")]
    public string Nome { get; set; } = "";

    [Column("username")]
    public string Username { get; set; } = "";

    [Column("email")]
    public string? Email { get; set; }

    [Column("password_hash")]
    public string PasswordHash { get; set; } = "";

    [Column("perfil")]
    public string Perfil { get; set; } = "";

    [Column("ativo")]
    public bool Ativo { get; set; }

    [Column("criado_em")]
    public DateTime CriadoEm { get; set; }
}
