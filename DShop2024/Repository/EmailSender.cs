
using System.Net.Mail;
using System.Net;

namespace DShop2024.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("haythamkenway17254@gmail.com", "hojcxzcgqwedrwwu")
            };

            return client.SendMailAsync(
                new MailMessage(from: "haythamkenway17254@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
