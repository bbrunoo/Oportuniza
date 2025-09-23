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
                    src.AuthorCompanyId.HasValue ? src.AuthorCompany.Id : src.AuthorUser.Id)) 

                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src =>
                    src.AuthorCompanyId.HasValue ? src.AuthorCompany.Name : src.AuthorUser.Name))

                .ForMember(dest => dest.AuthorImageUrl, opt => opt.MapFrom(src =>
                    src.AuthorCompanyId.HasValue ? src.AuthorCompany.ImageUrl : src.AuthorUser.ImageUrl))

                .ForMember(dest => dest.AuthorType, opt => opt.MapFrom(src =>
                    src.AuthorCompanyId.HasValue ? "Company" : "User"))

                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            CreateMap<PublicationCreateDto, Publication>();

            CreateMap<PublicationUpdateDto, Publication>()
                .ForMember(dest => dest.Resumee, opt => opt.MapFrom(src => src.Resumee));
        }
    }
}
