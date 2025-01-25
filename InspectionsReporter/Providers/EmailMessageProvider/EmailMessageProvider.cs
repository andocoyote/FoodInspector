using Azure;
using Azure.Communication.Email;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InspectionsReporter.Providers.EmailMessageProvider
{
    public class EmailMessageProvider : IEmailMessageProvider
    {
        private readonly string _sender = string.Empty;
        private readonly string _toEmail = string.Empty;
        private readonly string _subject = "Latest Food Inspector Results";

        private readonly ILogger _logger;

        public EmailMessageProvider(
            IOptions<EmailMessageOptions> emailMessageOptions,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<EmailMessageProvider>();

            _toEmail = emailMessageOptions.Value.ReceiverAccount;
            _sender = emailMessageOptions.Value.SenderAccount;
        }

        public async Task SendEmailAsync(string messageBody)
        {
            try
            {
                // Create the EmailClient
                _logger.LogInformation($"[SendEmailAsync] Creating the EmailClient.");
                //var emailClient = new EmailClient(connectionString);
                var emailClient = new EmailClient(new Uri("https://comm-food-inspector.unitedstates.communication.azure.com/"), new DefaultAzureCredential());

                //Create the email message
                _logger.LogInformation($"[SendEmailAsync] Creating the email message.");
                var emailMessage = new EmailMessage(
                    senderAddress: _sender,
                    content: new EmailContent(_subject)
                    {
                        PlainText = messageBody
                    },
                    recipients: new EmailRecipients(new List<EmailAddress> { new EmailAddress(_toEmail) }));

                // Send the email
                _logger.LogInformation($"[SendEmailAsync] Sending the email.");
                EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                    WaitUntil.Completed,
                    emailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SendEmailAsync] An exception was caught. Exception: {ex}");
            }
        }
    }
}
