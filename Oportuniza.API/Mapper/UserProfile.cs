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
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(x => x.Phone))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(x=>x.ImageUrl));

            CreateMap<User, UserByIdDTO>();
            CreateMap<User, UserInfoDTO>();
            CreateMap<UserProfile, AllUsersInfoDTO>();
        }
    }
}
