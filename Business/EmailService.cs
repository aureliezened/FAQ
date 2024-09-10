using Business;
using MailKit.Net.Smtp;
using MimeKit;
using System.Net.Mail;
using System.Threading.Tasks;
using Common.Configuration;
using Microsoft.Extensions.Options;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendOtpToEmail(string email, string otp)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("ChatBot App", "no-reply@chatbotapp.com")); // Sender email
        message.To.Add(new MailboxAddress("", email)); // Receiver email (use Mailtrap email for testing)
        message.Subject = "Your OTP Code";

        message.Body = new TextPart("plain")
        {
            Text = $"Your OTP code is: {otp}"
        };

        try
        {
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Email sending failed", ex);
        }
    }
}