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
                .ForMember(dest => dest.Author, opt => opt.MapFrom(x => x.AuthorName))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(x => x.ImageUrl))
                .ForPath(dest => dest.Author.ImageUrl, opt => opt.MapFrom(x => x.AuthorImageUrl))
                .ForPath(dest => dest.Author.UserType, opt => opt.MapFrom(x => x.AuthorType));

            CreateMap<PublicationCreateDto, Publication>();
            
            CreateMap<Publication, PublicationDto>();
            CreateMap<Publication, PublicationCreateDto>();
        }
    }
}
