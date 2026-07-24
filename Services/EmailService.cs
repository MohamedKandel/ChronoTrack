using System.Net;
using System.Net.Mail;
using Encryption;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public void SendEmail(string to, string subject, string body)
    {
        var email = _config["EmailSettings:FromEmail"];
        var password = _config["EmailSettings:Password"];
        var smtpClient = new SmtpClient(_config["EmailSettings:SmtpServer"])
        {
            Port = int.Parse(_config["EmailSettings:Port"]),
            Credentials = new NetworkCredential(EncryptionUtils.decrypt(email,"Maa%QS7Ejx5k43h3") 
            , EncryptionUtils.decrypt(password,"Maa%QS7Ejx5k43h3")),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(EncryptionUtils.decrypt(email,"Maa%QS7Ejx5k43h3")
            , "ChronoTrack"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(to);

        smtpClient.Send(message);
    }
}