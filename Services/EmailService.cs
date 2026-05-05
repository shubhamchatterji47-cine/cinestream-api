using MailKit.Net.Smtp;
using MimeKit;
using System.Net.Mail;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace CineStream.Services
{
  public class EmailService
  {
    public async Task SendEmail(string toEmail, string link)
    {
      var email = new MimeMessage();

      email.From.Add(new MailboxAddress("CineStream", "your-email@gmail.com"));
      email.To.Add(MailboxAddress.Parse(toEmail));
      email.Subject = "Confirm your email";

      email.Body = new TextPart("html")
      {
        Text = $"Click here: <a href='{link}'>Confirm Email</a>"
      };

      using var smtp = new SmtpClient();
      await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

      await smtp.AuthenticateAsync("chatterjishubham21@gmail.com", "bhfk lmhs ussj qlou");

      await smtp.SendAsync(email);
      await smtp.DisconnectAsync(true);
    }
  }
}
