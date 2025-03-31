using System.ComponentModel.DataAnnotations;

namespace Oportuniza.API.Viewmodel
{
    public class UserRegisterViewmodel
    {
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        public bool isACompany { get; set; }
    }
}
