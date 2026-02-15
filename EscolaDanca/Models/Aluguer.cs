

namespace EscolaDanca.Models;

public class Aluguer
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public int ResponsavelId { get; set; }
    public int? AlunoId { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
    public string Estado { get; set; } = "PEDIDO";
    public DateTime CriadoEm { get; set; }

    public InventarioItem? Item { get; set; }
}
