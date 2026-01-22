// File: PutZige.Application/Mappings/UserMappingProfile.cs

#nullable enable
using AutoMapper;
using PutZige.Domain.Entities;
using PutZige.Application.DTOs.Auth;

namespace PutZige.Application.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // User -> RegisterUserResponse
        CreateMap<User, RegisterUserResponse>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));

        // Additional mappings can be added here
    }
}
