using Microsoft.AspNetCore.Mvc;
using Oportuniza.API.Services;

namespace Oportuniza.API.Controllers
{
    [Route("api/sms")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        private readonly SmsService _smsService;
        private readonly OtpCacheService _otpCache;

        public SmsController(SmsService smsService, OtpCacheService otpCache)
        {
            _smsService = smsService;
            _otpCache = otpCache;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendSmsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                return BadRequest("Número inválido");

            var otpCode = new Random().Next(100000, 999999).ToString();

            // Salva o código
            _otpCache.SaveOtp(request.PhoneNumber, otpCode);

            await _smsService.SendOtpAsync(request.PhoneNumber, otpCode);

            return Ok("Código enviado");
        }

        [HttpPost("validate-otp")]
        public IActionResult ValidateOtp([FromBody] ValidateOtpRequest request)
        {
            var isValid = _otpCache.ValidateOtp(request.PhoneNumber, request.OtpCode);
            if (isValid)
                return Ok("Código válido");
            else
                return BadRequest("Código incorreto ou expirado");
        }
    }
    public class SendSmsRequest
    {
        public string PhoneNumber { get; set; }
    }

    public class ValidateOtpRequest
    {
        public string PhoneNumber { get; set; }
        public string OtpCode { get; set; }
    }
}
