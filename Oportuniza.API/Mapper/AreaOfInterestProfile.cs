using AutoMapper;
using Oportuniza.Domain.DTOs.AreasOfInterest;
using Oportuniza.Domain.Models;

namespace Oportuniza.API.Mapper
{
    public class AreaOfInterestProfile : Profile
    {
        public AreaOfInterestProfile()
        {
            CreateMap<AreasCreateDto, AreaOfInterest>();
            CreateMap<AreaOfInterest, AreasDto>();
        }
    }
}
