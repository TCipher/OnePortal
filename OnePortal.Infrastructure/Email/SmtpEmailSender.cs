using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using OnePortal.Application.Abstractions;
using System.Net.Mail;

public class SmtpOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool UseStartTls { get; set; } = true; 
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string From { get; set; } = ""; 
    public string FromName { get; set; } = "OnePortal";
}

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _o;
    public SmtpEmailSender(IOptions<SmtpOptions> o) => _o = o.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_o.FromName, _o.From));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;
        msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync(
       _o.Host,
       _o.Port,
       MailKit.Security.SecureSocketOptions.StartTls,
       ct
   );
        if (!string.IsNullOrEmpty(_o.User)) await client.AuthenticateAsync(_o.User, _o.Password, ct);
        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);
    }
}
