using ProjetoFinal.Services;
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // Pegar configurações do appsettings
        var smtpHost = _configuration["Email:SmtpHost"];
        var smtpUser = _configuration["Email:SmtpUser"];
        var smtpPass = _configuration["Email:SmtpPass"];
        var fromName = _configuration["Email:FromName"];
        var smtpPortStr = _configuration["Email:SmtpPort"];

        // Validar configurações
        if (string.IsNullOrWhiteSpace(smtpHost))
            throw new InvalidOperationException("SMTP host não configurado.");

        if (string.IsNullOrWhiteSpace(smtpUser))
            throw new InvalidOperationException("SMTP user não configurado.");

        if (string.IsNullOrWhiteSpace(smtpPass))
            throw new InvalidOperationException("SMTP password não configurado.");

        if (string.IsNullOrWhiteSpace(fromName))
            fromName = "Ginásio Fit Control"; // valor padrão

        if (!int.TryParse(smtpPortStr, out var smtpPort))
            smtpPort = 587; // valor padrão se não configurado

        // Criar cliente SMTP
        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        // Criar email
        using var mail = new MailMessage()
        {
            From = new MailAddress(smtpUser, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(to);

        // Enviar email com tratamento de exceções
        try
        {
            await client.SendMailAsync(mail);
        }
        catch (SmtpException ex)
        {
            //Falha ao enviar email
            throw new InvalidOperationException("Falha ao enviar email.", ex);
        }
    }
}
