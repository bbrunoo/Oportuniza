namespace Oportuniza.Domain.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendVerificationEmailAsync(string to, string subject, string message, string code);
    }
}
