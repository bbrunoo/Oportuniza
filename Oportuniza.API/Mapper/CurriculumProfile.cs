using AutoMapper;
using Oportuniza.Domain.DTOs.Curriculum;
using Oportuniza.Domain.Models;

namespace Oportuniza.API.Mapper
{
    public class CurriculumProfile : Profile
    {
        public CurriculumProfile()
        {
            // Dto -> Model
            CreateMap<CurriculumCreateDto, Curriculum>()
                .ForMember(dest => dest.Educations, opt => opt.MapFrom(src => src.Educations))
                .ForMember(dest => dest.Experiences, opt => opt.MapFrom(src => src.Experiences))
                .ForMember(dest => dest.Certifications, opt => opt.MapFrom(src => src.Certifications));

            CreateMap<EducationCreateDto, Education>();
            CreateMap<ExperienceCreateDto, Experience>();
            CreateMap<CertificationCreateDto, Certification>();

            CreateMap<CurriculumDto, Curriculum>()
                .ForMember(dest => dest.Educations, opt => opt.MapFrom(src => src.Educations))
                .ForMember(dest => dest.Experiences, opt => opt.MapFrom(src => src.Experiences))
                .ForMember(dest => dest.Certifications, opt => opt.MapFrom(src => src.Certifications));

            // Model -> Dto
            CreateMap<Curriculum, CurriculumResponseDto>()
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.Name))
                .ForMember(dest => dest.Uf, opt => opt.MapFrom(src => src.City.Uf));
                

            CreateMap<Curriculum, CurriculumDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.Educations, opt => opt.MapFrom(src => src.Educations))
                .ForMember(dest => dest.Experiences, opt => opt.MapFrom(src => src.Experiences))
                .ForMember(dest => dest.Certifications, opt => opt.MapFrom(src => src.Certifications));

            CreateMap<Education, EducationDto>();
            CreateMap<Experience, ExperienceDto>();
            CreateMap<Certification, CertificationDto>();
        }
    }
}
