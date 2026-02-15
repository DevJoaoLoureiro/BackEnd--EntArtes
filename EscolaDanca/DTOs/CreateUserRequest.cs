namespace EscolaDanca.DTOs;

public class CriarUtilizadorRequest
{
    public string Nome { get; set; } = "";
    public string Username { get; set; } = "";
    public string? Email { get; set; }
    public string Password { get; set; } = "";
    public string Perfil { get; set; } = "ENCARREGADO"; // ADMIN | SUPER_ADMIN | PROFESSOR | ENCARREGADO
}
