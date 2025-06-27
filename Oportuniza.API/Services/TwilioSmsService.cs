using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Oportuniza.API.Services
{
    public class SmsService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromNumber;

        public SmsService(IConfiguration configuration)
        {
            _accountSid = configuration["Twilio:AccountSid"];
            _authToken = configuration["Twilio:AuthToken"];
            _fromNumber = configuration["Twilio:FromNumber"];

            TwilioClient.Init(_accountSid, _authToken);
        }
        public async Task SendOtpAsync(string toPhoneNumber, string otpCode)
        {
            var message = await MessageResource.CreateAsync(
                body: $"Seu código de verificação é: {otpCode}",
                from: new PhoneNumber(_fromNumber),
                to: new PhoneNumber(toPhoneNumber)
            );

            Console.WriteLine($"SMS enviado. SID: {message.Sid}");
        }
    }
}
