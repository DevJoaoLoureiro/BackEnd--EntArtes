using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

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

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html")
        {
            Text = body
        };

        using var client = new SmtpClient();

        Console.WriteLine($"SMTP HOST USADO = {smtpHost}");
        Console.WriteLine($"SMTP PORT USADO = {smtpPort}");
        Console.WriteLine($"SMTP USER USADO = {smtpUser}");

        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(smtpUser, smtpPass);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}