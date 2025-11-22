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
                    src.AuthorCompany != null ? src.AuthorCompany.Id :
                    src.AuthorUser != null ? src.AuthorUser.Id :
                    src.CreatedByUser != null ? src.CreatedByUser.Id : Guid.Empty))

                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src =>
                    src.AuthorCompany != null ? src.AuthorCompany.Name :
                    src.AuthorUser != null ? src.AuthorUser.Name :
                    src.CreatedByUser != null ? src.CreatedByUser.Name : "Unknown"))

                .ForMember(dest => dest.AuthorImageUrl, opt => opt.MapFrom(src =>
                    src.AuthorCompany != null ? src.AuthorCompany.ImageUrl :
                    src.AuthorUser != null ? src.AuthorUser.ImageUrl :
                    src.CreatedByUser != null ? src.CreatedByUser.ImageUrl : null))

                .ForMember(dest => dest.AuthorType, opt => opt.MapFrom(src =>
                    src.AuthorCompany != null ? "Company" :
                    src.AuthorUser != null ? "User" : "Unknown"))

                .ForMember(dest => dest.CompanyOwnerId, opt => opt.MapFrom(src => src.AuthorCompanyId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))

                .ForMember(dest => dest.PostAuthorName,
                    opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.Name : null));

            CreateMap<PublicationCreateDto, Publication>();

            CreateMap<PublicationUpdateDto, Publication>()
                .ForMember(dest => dest.Resumee, opt => opt.MapFrom(src => src.Resumee));


        }
    }
}
