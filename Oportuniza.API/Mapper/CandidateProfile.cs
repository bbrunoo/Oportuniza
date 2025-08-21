using AutoMapper;
using Oportuniza.Domain.DTOs.Candidates;
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
                .ForMember(dest => dest.PublicationTitle,
                           opt => opt.MapFrom(src => src.Publication.Title))
                .ForMember(dest => dest.UserName,
                           opt => opt.MapFrom(src => src.User.Name));
        }
    }
}
