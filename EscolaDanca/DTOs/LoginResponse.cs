namespace EscolaDanca.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = "";
    public object User { get; set; } = default!;
}
