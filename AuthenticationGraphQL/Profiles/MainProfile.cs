using AuthenticationGraphQL.Dto;
using AuthenticationGraphQL.Models;
using AutoMapper;

namespace AuthenticationGraphQL.Profiles
{
    public class MainProfile : Profile
    {
        public MainProfile()
        {
            CreateMap<UserModel, UserDto>();
            CreateMap<UserDto, UserModel>();
        }
    }
}
