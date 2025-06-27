using AutoMapper;
using Oportuniza.Domain.DTOs.AreasOfInterest;
using Oportuniza.Domain.Models;

namespace Oportuniza.API.Mapper
{
    public class UserAreaOfInterestProfile : Profile
    {
        public UserAreaOfInterestProfile()
        {
            CreateMap<UserAreaCreateDto, UserAreaOfInterest>();

            CreateMap<UserAreaOfInterest, UserAreaDto>()
                .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.AreaOfInterest.InterestArea))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.Name));
        }
    }
}
