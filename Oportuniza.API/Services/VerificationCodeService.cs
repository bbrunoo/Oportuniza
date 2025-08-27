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
        // Armazena código + tempo de expiração
        private static readonly ConcurrentDictionary<string, (string Code, DateTime Expiry)> _codes
            = new ConcurrentDictionary<string, (string, DateTime)>();

        private const int ExpirationSeconds = 60;
        private const int CodeLength = 8;
        private static readonly char[] _chars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        public string GenerateCode(string email)
        {
            var code = GenerateRandomCode(CodeLength);

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

        private string GenerateRandomCode(int length)
        {
            var sb = new StringBuilder(length);
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4 * length];
                rng.GetBytes(data);

                for (int i = 0; i < length; i++)
                {
                    var idx = BitConverter.ToUInt32(data, i * 4) % (uint)_chars.Length;
                    sb.Append(_chars[idx]);
                }
            }
            return sb.ToString();
        }
    }
}
