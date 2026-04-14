using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EscolaDanca.Models;

[Table("tipo_aula")]
public class TipoAula
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nome")]
    public string Nome { get; set; } = null!;

    [Column("descricao")]
    public string? Descricao { get; set; }

    [Column("duracao_padrao")]
    public int? DuracaoPadrao { get; set; }

    // navigation opcional
    public ICollection<SessaoAula> Sessoes { get; set; } = new List<SessaoAula>();
}