namespace EscolaDanca.DTOs;

public class CreateTurmaDto
{
    public string Nome { get; set; } = null!;
    public int? ProfessorUtilizadorId { get; set; }
    public int? TipoAulaId { get; set; }
}