using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oportuniza.Domain.Models
{
    public class UserLogin
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Required]
        public string IdentityProvider { get; set; }

        [Required]
        public string ProviderId { get; set; }
    }
}
