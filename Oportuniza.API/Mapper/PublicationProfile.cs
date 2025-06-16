using AutoMapper;
using Oportuniza.Domain.DTOs.Publication;
using Oportuniza.Domain.Models;

namespace Oportuniza.API.Mapper
{
    public class PublicationProfile : Profile
    {
        public PublicationProfile()
        {
            CreateMap<Publication, PublicationDto>()
             .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src =>
                 src.AuthorCompanyId.HasValue && src.AuthorCompany != null
                     ? src.AuthorCompany.Id
                     : src.AuthorUser.Id))

             .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src =>
                 src.AuthorCompanyId.HasValue && src.AuthorCompany != null
                     ? src.AuthorCompany.Name
                     : src.AuthorUser.Name))

             .ForMember(dest => dest.AuthorImageUrl, opt => opt.MapFrom(src =>
                 src.AuthorCompanyId.HasValue && src.AuthorCompany != null
                     ? src.AuthorCompany.ImageUrl
                     : src.AuthorUser.ImageUrl))

             .ForMember(dest => dest.AuthorType, opt => opt.MapFrom(src =>
                 src.AuthorCompanyId.HasValue ? "Company" : "User"));


            CreateMap<PublicationCreateDto, Publication>();
        }
    }
}
