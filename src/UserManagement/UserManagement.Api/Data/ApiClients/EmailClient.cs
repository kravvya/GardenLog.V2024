using Azure;
using Azure.Communication.Email;
using GardenLog.SharedInfrastructure;
using System.Text;

namespace UserManagement.Api.Data.ApiClients
{
    public interface IEmailClient
    {
        Task<bool> SendEmail(SendEmailCommand request);
        Task<bool> SendEmailToUser(SendEmailCommand request);
    }

    public class EmailClient : IEmailClient
    {
        private readonly Azure.Communication.Email.EmailClient _emailClient;
        private readonly ILogger<EmailClient> _logger;

        public EmailClient(Azure.Communication.Email.EmailClient emailClient, ILogger<EmailClient> logger)
        {
            _emailClient = emailClient;
            _logger = logger;
        }

        public async Task<bool> SendEmail(SendEmailCommand request)
        {
            try
            {
                StringBuilder sb = new();
                sb.AppendLine($" from: {request.Name} <br/>");
                sb.AppendLine($" at: {request.EmailAddress} <br/>");
                sb.AppendLine($" sent message: {request.Message}");

                var emailContent = new EmailContent(request.Subject)
                {
                    Html = sb.ToString()
                };

                var toRecipients = new List<EmailAddress>
                {
                    new EmailAddress("stevchik@yahoo.com", "GardenLog Admin")
                };

                var emailRecipients = new EmailRecipients(toRecipients);

                var emailMessage = new EmailMessage(
                    senderAddress: "GardenLog@slavgl.com",
                    emailRecipients,
                    emailContent);

                EmailSendOperation emailSendOperation = await _emailClient.SendAsync(
                    WaitUntil.Completed,
                    emailMessage);

                return emailSendOperation.HasCompleted && emailSendOperation.HasValue 
                    && (emailSendOperation.Value.Status == EmailSendStatus.Succeeded);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception sending email: {Message}", ex.Message);
            }
            return false;
        }

        public async Task<bool> SendEmailToUser(SendEmailCommand request)
        {
            try
            {
                var emailContent = new EmailContent(request.Subject)
                {
                    Html = request.Message
                };

                var toRecipients = new List<EmailAddress>
                {
                    new EmailAddress(request.EmailAddress, request.Name)
                };

                var emailRecipients = new EmailRecipients(toRecipients);

                var emailMessage = new EmailMessage(
                    senderAddress: "GardenLog@slavgl.com",
                    emailRecipients,
                    emailContent);

                EmailSendOperation emailSendOperation = await _emailClient.SendAsync(
                    WaitUntil.Completed,
                    emailMessage);

                return emailSendOperation.HasCompleted && emailSendOperation.HasValue 
                    && (emailSendOperation.Value.Status == EmailSendStatus.Succeeded);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception sending email: {Message}", ex.Message);
            }
            return false;
        }
    }
}
