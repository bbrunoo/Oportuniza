using AutoMapper;
using Oportuniza.Domain.DTOs.Company;
using Oportuniza.Domain.Models;

namespace Oportuniza.API.Mapper
{
    public class CompanyProfile : Profile
    {
        public CompanyProfile() {
            CreateMap<CompanyCreateDto, Company>();

            CreateMap<Company, CompanyDTO>()
                .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src => src.Manager.Name))
                .ForMember(dest => dest.Employees, opt => opt.MapFrom(src => src.Employees));

            CreateMap<CompanyEmployee, CompanyEmployeeDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name));
        }
    }
}
