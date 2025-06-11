using AutoMapper;
using Oportuniza.Domain.DTOs.Publication;
using Oportuniza.Domain.Models;

namespace Oportuniza.API.Mapper
{
    public class PublicationProfile : Profile
    {
        public PublicationProfile()
        {
            CreateMap<PublicationDto, Publication>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(x => x.AuthorName));
            CreateMap<PublicationCreateDto, Publication>();
            
            CreateMap<Publication, PublicationDto>();
            CreateMap<Publication, PublicationCreateDto>();
        }
    }
}
