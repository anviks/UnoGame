using System.Net;
using System.Net.Mail;
using Domain.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Domain.Services;

public class EmailService
{
    private readonly EmailConfig _config;
    private readonly SmtpClient _smtp;
    private readonly string _templatePath;

    public EmailService(EmailConfig config, string templatePath)
    {
        _templatePath = templatePath;
        _config = config;
        _smtp = new SmtpClient(_config.Host, _config.Port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_config.Username, _config.Password)
        };
    }

    public async Task SendAsync(string to, string subject, string link)
    {
        var body = (await File.ReadAllTextAsync(_templatePath)).Replace("{{MAGIC_LINK}}", link);
        var mail = new MailMessage(_config.Username, to, subject, body)
        {
            IsBodyHtml = true
        };
        await _smtp.SendMailAsync(mail);
    }
}