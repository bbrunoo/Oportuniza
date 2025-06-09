using AutoMapper;
using Oportuniza.Domain.DTOs.Publication;
using Oportuniza.Domain.Models;

namespace Oportuniza.API.Mapper
{
    public class PublicationProfile : Profile
    {
        public PublicationProfile()
        {
            CreateMap<PublicationDto, Publication>();
            CreateMap<PublicationCreateDto, Publication>();
            
            CreateMap<Publication, PublicationDto>();
            CreateMap<Publication, PublicationCreateDto>();
        }
    }
}
