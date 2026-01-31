#nullable enable
using AutoMapper;
using PutZige.Application.DTOs.Auth;
using PutZige.Domain.Entities;

namespace PutZige.Application.Mappings
{
    public class AuthMappingProfile : Profile
    {
        public AuthMappingProfile()
        {
            CreateMap<User, LoginResponse>()
                .ForMember(dst => dst.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dst => dst.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dst => dst.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dst => dst.AccessToken, opt => opt.Ignore())
                .ForMember(dst => dst.RefreshToken, opt => opt.Ignore())
                .ForMember(dst => dst.ExpiresIn, opt => opt.Ignore());
        }
    }
}
