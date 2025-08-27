using AutoMapper;
using Oportuniza.Domain.DTOs.User;
using Oportuniza.Domain.Models;

namespace Oportuniza.API.Mapper
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDTO, User>();
            CreateMap<UserByIdDTO, User>();
            CreateMap<UserInfoDTO, User>();
            CreateMap<AllUsersInfoDTO, User>();
            CreateMap<CompleteProfileDTO, User>();

            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.IsProfileCompleted, opt => opt.MapFrom(src => src.IsProfileCompleted));

            CreateMap<User, UserByIdDTO>();
            CreateMap<User, UserInfoDTO>();

            CreateMap<User, AllUsersInfoDTO>();
        }
    }
}
