//using MailKit.Net.Smtp;
//using MimeKit;
//using System.Net.Mail;
//using SmtpClient = MailKit.Net.Smtp.SmtpClient;

//namespace CineStream.Services
//{
//  public class EmailService
//  {
//    public async Task SendEmail(string toEmail, string link)
//    {
//      var email = new MimeMessage();

//      email.From.Add(new MailboxAddress("CineStream", "your-email@gmail.com"));
//      email.To.Add(MailboxAddress.Parse(toEmail));
//      email.Subject = "Confirm your email";

//      email.Body = new TextPart("html")
//      {
//        Text = $"Click here: <a href='{link}'>Confirm Email</a>"
//      };

//      using var smtp = new SmtpClient();
//      await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

//      await smtp.AuthenticateAsync("chatterjishubham21@gmail.com", "bhfk lmhs ussj qlou");

//      await smtp.SendAsync(email);
//      await smtp.DisconnectAsync(true);
//    }
//  }
//}
using MailKit.Net.Smtp;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace CineStream.Services
{
  public class EmailService
  {
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public async Task SendEmail(string toEmail, string link)
    {
      var email = new MimeMessage();

      email.From.Add(new MailboxAddress("CineStream", "shubhamchatterji47@gmail.com"));
      email.To.Add(MailboxAddress.Parse(toEmail));
      email.Subject = "Confirm your CineStream email";

      email.Body = new TextPart("html")
      {
        Text = $@"
          <div style='font-family:Arial,sans-serif;max-width:500px;margin:auto;padding:20px'>
            <h2 style='color:#e50914'>🎬 Welcome to CineStream!</h2>
            <p>Thanks for registering. Please confirm your email to start watching.</p>
            <a href='{link}' style='background:#e50914;color:white;padding:12px 24px;text-decoration:none;border-radius:4px;display:inline-block;margin:16px 0'>
              Confirm Email
            </a>
            <p style='color:#999;font-size:12px'>If you didn't register, ignore this email.</p>
          </div>"
      };
      var username = _configuration["Brevo:Username"]
                    ?? Environment.GetEnvironmentVariable("Brevo__Username");
      var password = _configuration["Brevo:Password"]
                     ?? Environment.GetEnvironmentVariable("Brevo__Password");

      using var smtp = new SmtpClient();
      //await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
      //await smtp.AuthenticateAsync("chatterjishubham21@gmail.com", "bhfk lmhs ussj qlou");
      await smtp.ConnectAsync("smtp-relay.brevo.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
      await smtp.AuthenticateAsync(username, password);
      await smtp.SendAsync(email);
      await smtp.DisconnectAsync(true);
    }
  }
}
