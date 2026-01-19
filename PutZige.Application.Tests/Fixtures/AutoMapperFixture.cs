// PutZige.Application.Tests/Fixtures/AutoMapperFixture.cs
#nullable enable
using AutoMapper;
using PutZige.Application.DTOs.Auth;
using PutZige.Domain.Entities;

namespace PutZige.Application.Tests.Fixtures
{
    public class AutoMapperFixture
    {
        public IMapper Mapper { get; }

        public AutoMapperFixture()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, RegisterUserResponse>()
                    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                    .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                    .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                    .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => src.IsEmailVerified));
            });

            Mapper = config.CreateMapper();
        }
    }
}
