using AutoMapper;
using Oportuniza.Domain.DTOs.Candidates;
using Oportuniza.Domain.DTOs.Extra;
using Oportuniza.Domain.DTOs.Publication;
using Oportuniza.Domain.Models;

namespace Oportuniza.API.Mapper
{
    public class CandidateProfile : Profile
    {
        public CandidateProfile()
        {
            CreateMap<CreateCandidatesDTO, CandidateApplication>();
            CreateMap<PutCandidatesDTO, CandidateApplication>();

            CreateMap<CandidateApplication, CandidatesDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
                .ForMember(dest => dest.UserIdKeycloak, opt => opt.MapFrom(src => src.UserIdKeycloak))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.User.ImageUrl))
                .ForMember(dest => dest.PublicationId, opt => opt.MapFrom(src => src.Publication.Id))
                .ForMember(dest => dest.PublicationTitle, opt => opt.MapFrom(src => src.Publication.Title))
                .ForMember(dest => dest.Resumee, opt => opt.MapFrom(src => src.Publication.Resumee))
                .ForMember(dest => dest.ApplicationDate, opt => opt.MapFrom(src => src.ApplicationDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            CreateMap<Publication, PublicationWithCandidatesDto>()
                .ForMember(dest => dest.Resumee, opt => opt.MapFrom(src => src.Resumee))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorUser != null
                                                                            ? src.AuthorUser.Id
                                                                            : src.AuthorCompany != null
                                                                                ? src.AuthorCompany.Id
                                                                                : src.CreatedByUser.Id))
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.AuthorUser != null
                                                                            ? src.AuthorUser.Name
                                                                            : src.AuthorCompany != null
                                                                                ? src.AuthorCompany.Name
                                                                                : src.CreatedByUser.Name))
                .ForMember(dest => dest.AuthorImage, opt => opt.MapFrom(src => src.AuthorUser != null
                                                                            ? src.AuthorUser.ImageUrl
                                                                            : src.AuthorCompany != null
                                                                                ? src.AuthorCompany.ImageUrl
                                                                                : src.CreatedByUser.ImageUrl))
                .ForMember(dest => dest.Candidates, opt => opt.MapFrom(src => src.CandidateApplication));

            CreateMap<CandidateApplication, CandidateDto>()
                .ForMember(dest => dest.CandidateId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.UserImage, opt => opt.MapFrom(src => src.User.ImageUrl))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CandidateApplication, UserApplicationDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Publication, opt => opt.MapFrom(src => src.Publication));

            CreateMap<Publication, PublicationDto>()
                .ForMember(dest => dest.Resumee, opt => opt.MapFrom(src => src.Resumee))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl));
        }
    }
}
