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

            CreateMap<User, UserDTO>();
            CreateMap<User, UserByIdDTO>();
            CreateMap<User, UserInfoDTO>();
            CreateMap<UserProfile, AllUsersInfoDTO>();
        }
    }
}
