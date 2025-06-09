using AutoMapper;
using Oportuniza.Domain.DTOs.AreasOfInterest;
using Oportuniza.Domain.Models;

namespace Oportuniza.API.Mapper
{
    public class CompanyAreaOfInterestProfile : Profile
    {
        public CompanyAreaOfInterestProfile()
        {
            CreateMap<CompanyAreaCreateDto, CompanyAreaOfInterest>();

            CreateMap<CompanyAreaOfInterest, CompanyAreaDto>()
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.AreaOfInterest.InterestArea))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name));
        }
    }
}
