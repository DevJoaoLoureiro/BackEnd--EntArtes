using System.ComponentModel.DataAnnotations.Schema;
namespace EscolaDanca.Models;


[Table("confirmacoes_presenca")]

public class ConfirmacaoPresenca
{

    [Column("id")]
    public int Id { get; set; }

    [Column("sessao_aula_id")]
    public int SessaoAulaId { get; set; }
    [Column("aluno_id")]
    public int AlunoId { get; set; }


    [Column("vai")]
    public bool? Vai { get; set; } // null = pendente

    [Column("respondido_por_utilizador_id")]
    public int RespondidoPorUtilizadorId { get; set; }

    [Column("respondido_em")]
    public DateTime RespondidoEm { get; set; }


    [Column("criado_em")] 
    public DateTime CriadoEm { get; set; }

    public Aluno Aluno { get; set; }

}