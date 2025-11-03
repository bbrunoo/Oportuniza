using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace Oportuniza.API.Services
{
    public interface IVerificationCodeService
    {
        string GenerateCode(string email);
        bool ValidateCode(string email, string code);
    }

    public class VerificationCodeService : IVerificationCodeService
    {
        private static readonly ConcurrentDictionary<string, (string Code, DateTime Expiry)> _codes
            = new ConcurrentDictionary<string, (string, DateTime)>();

        private const int ExpirationSeconds = 60;
        private const int CodeLength = 8;

        private static readonly char[] _digits = "0123456789".ToCharArray();

        public string GenerateCode(string email)
        {
            var code = GenerateNumericCode(CodeLength);
            _codes[email] = (code, DateTime.UtcNow.AddSeconds(ExpirationSeconds));
            return code;
        }

        public bool ValidateCode(string email, string code)
        {
            if (_codes.TryGetValue(email, out var entry))
            {
                if (entry.Code == code && entry.Expiry > DateTime.UtcNow)
                {
                    _codes.TryRemove(email, out _);
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
