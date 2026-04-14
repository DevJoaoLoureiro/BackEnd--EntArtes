using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaDanca.Models;

[Table("convites_utilizador")] // Nome da tabela em minúsculas
public class ConviteUtilizador
{
    [Column("id")]
    public int Id { get; set; }

    [Column("email")]
    public string Email { get; set; } = null!;

    [Column("perfil")]
    public string Perfil { get; set; } = null!;

    [Column("token")]
    public string Token { get; set; } = null!;

    [Column("expira_em")]
    public DateTime ExpiraEm { get; set; }

    [Column("usado")]
    public bool Usado { get; set; } = false;

    [Column("criado_em")]
    public DateTime criado_em { get; set; } = DateTime.UtcNow;

    [Column("criado_por_utilizador_id")]
    public int CriadoPorUtilizadorId { get; set; }
}