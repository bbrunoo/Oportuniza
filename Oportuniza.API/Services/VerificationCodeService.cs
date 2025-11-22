using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace Oportuniza.API.Services
{
    public interface IVerificationCodeService
    {
        string GenerateCode(string email, string context);
        bool ValidateCode(string email, string code, string context);
    }

    public class VerificationCodeService : IVerificationCodeService
    {
        private static readonly ConcurrentDictionary<string, (string Code, DateTime Expiry)> _codes
            = new ConcurrentDictionary<string, (string, DateTime)>();

        private const int ExpirationSeconds = 60;
        private static readonly char[] _digits = "0123456789".ToCharArray();

        public string GenerateCode(string email, string context)
        {
            int length = context == "email" ? 8 : 6;
            var key = $"{context}:{email}";
            var code = GenerateNumericCode(length);
            _codes[key] = (code, DateTime.UtcNow.AddSeconds(ExpirationSeconds));
            return code;
        }

        public bool ValidateCode(string email, string code, string context)
        {
            var key = $"{context}:{email}";
            if (_codes.TryGetValue(key, out var entry))
            {
                if (entry.Code == code && entry.Expiry > DateTime.UtcNow)
                {
                    _codes.TryRemove(key, out _);
                    return true;
                }
            }
            return false;
        }

        private string GenerateNumericCode(int length)
        {
            var sb = new StringBuilder(length);
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[length];
                rng.GetBytes(data);

                for (int i = 0; i < length; i++)
                {
                    sb.Append(_digits[data[i] % _digits.Length]);
                }
            }
            return sb.ToString();
        }
    }
}
