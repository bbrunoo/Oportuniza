using Microsoft.Extensions.Configuration;
using Oportuniza.Domain.Interfaces;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Threading.Tasks;

namespace Oportuniza.API.Services
{
    public class MailGunEmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly string _domain;
        private readonly string _sender;

        public MailGunEmailService(IConfiguration configuration)
        {
            _apiKey = configuration["Mailgun:ApiKey"] ?? throw new ArgumentNullException("Mailgun:ApiKey não configurada");
            _domain = configuration["Mailgun:Domain"] ?? throw new ArgumentNullException("Mailgun:Domain não configurado");
            _sender = configuration["Mailgun:Sender"] ?? $"Oportuniza <postmaster@{_domain}>";
        }

        public async Task<bool> SendVerificationEmailAsync(string to, string subject, string message, string code)
        {
            try
            {
                var clientOptions = new RestClientOptions("https://api.mailgun.net")
                {
                    Authenticator = new HttpBasicAuthenticator("api", _apiKey)
                };

                var client = new RestClient(clientOptions);
                var request = new RestRequest($"/v3/{_domain}/messages", Method.Post)
                {
                    AlwaysMultipartFormData = true
                };

                string body = $@"
                    <html>
                        <body style='font-family:Arial, sans-serif;'>
                            <h2 style='color:#1a73e8;'>Verificação de Conta - Oportuniza</h2>
                            <p>{message}</p>
                            <div style='margin-top:20px;padding:10px 20px;
                                background-color:#1a73e8;color:white;
                                display:inline-block;border-radius:5px;
                                font-size:20px;letter-spacing:3px;'>
                                {code}
                            </div>
                            <p style='margin-top:20px;color:#555;'>Esse código expira em 60 segundos.</p>
                        </body>
                    </html>";

                request.AddParameter("from", _sender);
                request.AddParameter("to", to);
                request.AddParameter("subject", subject);
                request.AddParameter("html", body);

                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    Console.WriteLine($"[Mailgun] Erro ao enviar e-mail: {response.StatusCode} - {response.Content}");
                    return false;
                }

                Console.WriteLine($"[Mailgun] E-mail de verificação enviado com sucesso para {to}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Mailgun] Erro: {ex.Message}");
                return false;
            }
        }
    }
}
