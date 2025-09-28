using Oportuniza.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Oportuniza.Domain.DTOs.Company
{
    public class EmployeeStatusUpdateDto
    {
        [Required]
        public CompanyEmployeeStatus NewStatus { get; set; }
    }
}
