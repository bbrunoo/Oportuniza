namespace Oportuniza.Domain.Models
{
    public class CNPJCache
    {
        public string Cnpj { get; set; } = string.Empty;
        public string Situacao { get; set; } = string.Empty;
        public DateTime AtualizadoEm { get; set; }
    }
}
