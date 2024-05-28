using DealDynamo.Models;

namespace DealDynamo.Services
{
    public interface IEmailService
    {
        bool SendEmail(EmailData emailData);
    }
}
