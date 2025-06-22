using System.Collections.Concurrent;

namespace Oportuniza.API.Services
{
    public class OtpCacheService
    {
        private static readonly ConcurrentDictionary<string, string> otpStore = new();

        public void SaveOtp(string phoneNumber, string otpCode)
        {
            otpStore[phoneNumber] = otpCode;
        }

        public bool ValidateOtp(string phoneNumber, string inputOtp)
        {
            if (otpStore.TryGetValue(phoneNumber, out var correctOtp))
            {
                if (correctOtp == inputOtp)
                {
                    otpStore.TryRemove(phoneNumber, out _);
                    return true;
                }
            }
            return false;
        }
    }
}
