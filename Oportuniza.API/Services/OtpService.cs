using System.Collections.Concurrent;

namespace Oportuniza.API.Services
{
    public class OtpService
    {
        private static readonly ConcurrentDictionary<string, string> otpCache = new();

        public string GenerateOtp(string phoneNumber)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            otpCache[phoneNumber] = otp;
            return otp;
        }

        public bool ValidateOtp(string phoneNumber, string enteredOtp)
        {
            return otpCache.TryGetValue(phoneNumber, out var correctOtp) && correctOtp == enteredOtp;
        }
    }
}
