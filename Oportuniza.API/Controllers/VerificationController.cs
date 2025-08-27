using Microsoft.AspNetCore.Mvc;
using Oportuniza.API.Services;
using Oportuniza.Domain.Interfaces;
using Oportuniza.Domain.Models;
using static Oportuniza.API.Services.AzureEmailService;

namespace Oportuniza.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IVerificationCodeService _codeService;
        private readonly IUserRepository _userRepository;

        public VerificationController(IEmailService emailService, IVerificationCodeService codeService)
        {
            _emailService = emailService;
            _codeService = codeService;
        }

        [HttpPost("send-verification")]
        public async Task<IActionResult> SendVerification([FromBody] VerificationRequest request)
        {
            var code = _codeService.GenerateCode(request.ToEmail);

            bool success = await _emailService.SendVerificationEmailAsync(
                request.ToEmail,
                "Verificação de Conta Oportuniza",
                "Use o código abaixo para verificar sua nova conta na Oportuniza (expira em 60 segundos):",
                code
            );

            if (success)
                return Ok("E-mail de verificação enviado com sucesso.");
            else
                return StatusCode(500, "Erro ao enviar e-mail.");
        }

        [HttpPost("validate-code")]
        public IActionResult ValidateCode([FromBody] ValidateRequest request)
        {
            bool valid = _codeService.ValidateCode(request.ToEmail, request.Code);

            if (valid)
                return Ok("Código válido!");
            else
                return BadRequest("Código inválido ou expirado.");
        }
    }
}
