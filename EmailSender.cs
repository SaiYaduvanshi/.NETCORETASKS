using System.Net.Mail;
using System.Net;

namespace UserProfileApp
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mailMessage = new MailMessage("your-email@example.com", email, subject, message);
            mailMessage.IsBodyHtml = true;

            using (var smtpClient = new SmtpClient("smtp.example.com"))
            {
                smtpClient.Credentials = new NetworkCredential("your-email@example.com", "your-password");
                smtpClient.Port = 587; // or your SMTP server port
                smtpClient.EnableSsl = true;

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}
