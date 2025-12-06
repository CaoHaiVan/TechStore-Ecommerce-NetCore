using System.Net.Mail;
using System.Net;

namespace Doanchuyennganh.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("testmailwebsite123@gmail.com", "ikhasbqqsdwlomcx")
            };

            var mailMessage = new MailMessage("testmailwebsite123@gmail.com", email, subject, message);
            mailMessage.IsBodyHtml = true; // Cho phép HTML

            return client.SendMailAsync(mailMessage);
        }
    }
}
