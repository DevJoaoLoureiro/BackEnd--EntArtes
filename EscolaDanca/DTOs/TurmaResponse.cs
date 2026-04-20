namespace EscolaDanca.DTOs;

public class TurmaResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string? Nivel { get; set; }
    public int Capacidade { get; set; }
    public int? ProfessorUtilizadorId { get; set; }
    public string? ProfessorNome { get; set; }
    public bool Ativa { get; set; }
}