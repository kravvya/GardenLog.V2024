using GardenLog.SharedInfrastructure;
using SendGrid;
using SendGrid.Helpers.Mail;
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
        private readonly ISendGridClient _sendGridClient;
        private readonly ILogger<EmailClient> _logger;


        public EmailClient(ISendGridClient sendGridClient, ILogger<EmailClient> logger)
        {
            _sendGridClient = sendGridClient;
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

                var message = new SendGridMessage()
                {
                    From = new EmailAddress("contact@slavgl.com", "GardenLog Contact"),
                    Subject = request.Subject
                };

                message.AddContent(MimeType.Html, sb.ToString());
                message.AddTo(new EmailAddress("stevchik@yahoo.com", "GardenLog Admin"));

                var response = await _sendGridClient.SendEmailAsync(message);

                if (response != null)
                {
                    return response.StatusCode == System.Net.HttpStatusCode.Accepted;
                }

            }
            catch (Exception ex)
            {
                _logger.LogCritical("Exception sending email: {ex}", ex);
            }
            return false;
        }

        public async Task<bool> SendEmailToUser(SendEmailCommand request)
        {

            try
            {
                var message = new SendGridMessage()
                {
                    From = new EmailAddress("admin@slavgl.com", "GardenLog"),
                    Subject = request.Subject
                };

                message.AddContent(MimeType.Html, request.Message);
                message.AddTo(new EmailAddress(request.EmailAddress, request.Name));

                var response = await _sendGridClient.SendEmailAsync(message);

                if (response != null)
                {
                    return response.StatusCode == System.Net.HttpStatusCode.Accepted;
                }

            }
            catch (Exception ex)
            {
                _logger.LogCritical("Exception sending email: {ex}", ex);
            }
            return false;
        }

        //public async Task<bool> SendEmailAsyncOld(SendEmailCommand request)
        //{
        //    try
        //    {
        //        MailMessage newMail = new MailMessage();
        //        // use the Gmail SMTP Host
        //        using (var client = new SmtpClient("smtp.mail.yahoo.com"))
        //        {

        //            // Follow the RFS 5321 Email Standard
        //            newMail.From = new MailAddress("gardenlog@yahoo.com","GardenLog Contact");

        //            newMail.To.Add("stevchik@yahoo.com");

        //            newMail.Subject = request.Subject;

        //            newMail.IsBodyHtml = true;

        //            StringBuilder sb = new();
        //            sb.AppendLine($" from: {request.Name} <br/>");
        //            sb.AppendLine($" at: {request.EmailAddress} <br/>");
        //            sb.AppendLine($" sent message: {request.Message}");

        //            newMail.Body = sb.ToString();

        //            // enable SSL for encryption across channels
        //            client.EnableSsl = true;
        //            // Port 465 for SSL communication
        //            client.Port = 587; // try port 587 if you experience issues)
        //            // Provide authentication information with Gmail SMTP server to authenticate your sender account
        //            client.Credentials = new System.Net.NetworkCredential("gardenlog@yahoo.com", _email_password);

        //             await client.SendMailAsync(newMail);

        //            return true;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogCritical("Exception sending email", ex);
        //        return false;
        //    }
        //}

        //public bool SendEmail(SendEmailCommand request)
        //{
        //    try
        //    {

        //        var message = new MimeMessage();
        //        message.From.Add(new MailboxAddress ("GardenLog Contact", "gardenlog@yahoo.com"));
        //        message.To.Add(new MailboxAddress("Garden Log Admin", "stevchik@yahoo.com"));
        //        message.Subject = request.Subject;

        //        StringBuilder sb = new();
        //        sb.AppendLine($" from: {request.Name} <br/>");
        //        sb.AppendLine($" at: {request.EmailAddress} <br/>");
        //        sb.AppendLine($" sent message: {request.Message}");

        //        message.Body = new TextPart("plain") { Text = sb.ToString() };

        //        using (var client = new SmtpClient())
        //        {
        //            client.Connect("smtp.mail.yahoo.com", 465, SecureSocketOptions.Auto);

        //            // Note: only needed if the SMTP server requires authentication
        //            client.Authenticate("gardenlog@yahoo.com", _email_password);

        //            client.Send(message);
        //            client.Disconnect(true);
        //        }
        //        return true;

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogCritical("Exception sending email", ex);
        //        return false;
        //    }
        //}
    }
}
