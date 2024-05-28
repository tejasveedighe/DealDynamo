using DealDynamo.Models;
using DealDynamo.Services;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace DealDynamo.Data
{
    public class EmailService : IEmailService
    {
        EmailConfiguration _emailSettings = null;
        public EmailService(IOptions<EmailConfiguration> options)
        {
            _emailSettings = options.Value;
        }
        public bool SendEmail(EmailData emailData)
        {
            try
            {
                // Specify the from and to email address
                MailMessage mailMessage = new MailMessage(_emailSettings.EmailId, emailData.EmailToId);
                // Specify the email body
                mailMessage.Body = emailData.EmailBody;
                // Specify the email Subject
                mailMessage.Subject = emailData.EmailSubject;

                // Specify the SMTP server name and post number
                System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587);
                // Specify your gmail address and password
                smtpClient.Credentials = new System.Net.NetworkCredential()
                {
                    UserName = _emailSettings.EmailId,
                    Password = _emailSettings.Password
                };
                // Gmail works on SSL, so set this property to true
                smtpClient.EnableSsl = true;
                // Finall send the email message using Send() method
                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Log Exception Details
                return false;
            }
        }
    }
}
