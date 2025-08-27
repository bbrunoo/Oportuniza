using Azure;
using Azure.Communication.Email;

namespace Oportuniza.API.Services
{
    public class AzureEmailService
    {
        public interface IEmailService
        {
            Task<bool> SendVerificationEmailAsync(string toEmail, string subject, string message, string verificationCode);
        }
        public class EmailService : IEmailService
        {
            private readonly EmailClient _emailClient;
            private readonly string _fromEmail;

            public EmailService(IConfiguration configuration)
            {
                var connectionString = configuration["AzureCommunicationService:ConnectionString"];
                _fromEmail = configuration["AzureCommunicationService:SenderAdress"];

                _emailClient = new EmailClient(connectionString);
            }

            public async Task<bool> SendVerificationEmailAsync(string toEmail, string subject, string message, string verificationCode)
            {
                try
                {
                    var body = $@"
                        <html>
                            <body>
                                <h2>{subject}</h2>
                                <p>{message}</p>
                                <p><strong>Código de verificação: {verificationCode}</strong></p>
                            </body>
                        </html>";

                    var emailMessage = new EmailMessage(
                        senderAddress: _fromEmail,
                        content: new EmailContent(subject)
                        {
                            Html = body,
                            PlainText = $"{subject}\n{message}\nCódigo: {verificationCode}"
                        },
                        recipients: new EmailRecipients(new[] { new EmailAddress(toEmail) })
                    );

                    EmailSendOperation operation = await _emailClient.SendAsync(
                        wait: WaitUntil.Completed,
                        message: emailMessage
                    );

                    return operation.HasCompleted && operation.Value.Status == EmailSendStatus.Succeeded;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                    return false;
                }
            }
        }
    }
}
