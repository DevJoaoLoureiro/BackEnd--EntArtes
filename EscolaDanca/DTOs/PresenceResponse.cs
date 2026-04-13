namespace EscolaDanca.DTOs;

public class PresencaResponse
{
    public int AlunoId { get; set; }

    public string NomeAluno { get; set; }

    public bool Presente { get; set; }

    public DateTime? MarcadoEm { get; set; }
}