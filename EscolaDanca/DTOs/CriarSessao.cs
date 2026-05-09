namespace EscolaDanca.DTOs;

public class CreateSessao
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }

    public int? ProfessorUtilizadorId { get; set; }
    public int? TipoAulaId { get; set; }
    public int? EstudioId { get; set; }
    public int? MaxAlunos { get; set; }
    public int? TurmaId { get; set; }

    public bool InscricaoAberta { get; set; }

    public decimal? PrecoCoaching { get; set; }

    public string? Sumario { get; set; }
}