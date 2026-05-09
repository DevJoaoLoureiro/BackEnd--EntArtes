namespace EscolaDanca.DTOs;

public class CriarPagamentoRequest
{
    public int AlunoId { get; set; }
    public decimal Valor { get; set; }
    public string? Tipo { get; set; }
    public string? Descricao { get; set; }
    public int? SessaoId { get; set; }
    public int? Mes { get; set; }
    public int? Ano { get; set; }
}