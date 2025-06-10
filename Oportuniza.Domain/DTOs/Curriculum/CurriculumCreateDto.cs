using System.ComponentModel.DataAnnotations;

namespace Oportuniza.Domain.DTOs.Curriculum
{
    public class CurriculumCreateDto
    {
        [Required(ErrorMessage = "O ID do usuário é obrigatório.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [Phone(ErrorMessage = "Formato de telefone inválido.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "O objetivo é obrigatório.")]
        [StringLength(500, ErrorMessage = "O objetivo deve ter no máximo 500 caracteres.")]
        public string Objective { get; set; }

        [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "A cidade é obrigatória.")]
        public Guid CityId { get; set; }

        public List<EducationCreateDto> Educations { get; set; } = new();
        public List<ExperienceCreateDto> Experiences { get; set; } = new();
        public List<CertificationCreateDto> Certifications { get; set; } = new();
    }
}
