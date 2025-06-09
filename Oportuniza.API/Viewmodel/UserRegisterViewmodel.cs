using Oportuniza.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Oportuniza.API.Viewmodel
{
    public class UserRegisterViewmodel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserType UserType { get; set; }
    }
}
