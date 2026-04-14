namespace EscolaDanca.DTOs;

public class RegistoPorConviteRequest
{
    public string Token { get; set; } = null!;
    public string Nome { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;

    public List<EducandoDto>? Educandos { get; set; }
}

public class EducandoDto
{
    public string Nome { get; set; } = null!;
    public DateTime? DataNascimento { get; set; }
}