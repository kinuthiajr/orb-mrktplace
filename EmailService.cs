using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace Orb.API
{
    public class EmailService : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using (var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525))
            {
                client.Credentials = new NetworkCredential("3ebcacf4db1471", "d33b9bada39176");
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("from@example.com"),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email);

                try
                {
                    await client.SendMailAsync(mailMessage);
                    Console.WriteLine("Email sent successfully.");
                }
                catch (SmtpException smtpEx)
                {
                    Console.WriteLine($"SMTP Exception: {smtpEx.Message}");
                    // Handle specific SMTP exceptions here if needed
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Exception: {ex.Message}");
                    // Handle other exceptions here if needed
                }
            }
        }
    }
}
