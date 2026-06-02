using System.Text;
using System.Text.Json;

namespace EscolaDanca.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _cfg;
    private readonly HttpClient _http;

    public EmailService(IConfiguration cfg, HttpClient http)
    {
        _cfg = cfg;
        _http = http;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var apiKey = _cfg["Brevo:ApiKey"];
        var fromEmail = _cfg["Brevo:FromEmail"];
        var fromName = _cfg["Brevo:FromName"] ?? "Escola de Dança";

        var payload = new
        {
            sender = new
            {
                email = fromEmail,
                name = fromName
            },
            to = new[]
            {
                new { email = to }
            },
            subject,
            htmlContent = body.Replace("\n", "<br>")
        };

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.brevo.com/v3/smtp/email"
        );

        request.Headers.Add("api-key", apiKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Brevo erro: {error}");
        }
    }
}