using System.Net;
using System.Net.Mail;

namespace EscolaDanca.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _cfg;

    public EmailService(IConfiguration cfg)
    {
        _cfg = cfg;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var smtpHost = _cfg["Email:SmtpHost"];
        var smtpPort = int.Parse(_cfg["Email:SmtpPort"]!);
        var smtpUser = _cfg["Email:Username"];
        var smtpPass = _cfg["Email:Password"];
        var from = _cfg["Email:From"];

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        using var mail = new MailMessage(from!, to, subject, body);
        await client.SendMailAsync(mail);
    }
}