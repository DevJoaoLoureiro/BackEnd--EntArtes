namespace EscolaDanca.Models;

public class Pagamento
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public string Referencia { get; set; } = "";
    public decimal Valor { get; set; }
    public string Estado { get; set; } = "PENDENTE";
    public DateOnly? DataPagamento { get; set; }
    public DateTime CriadoEm { get; set; }
}
