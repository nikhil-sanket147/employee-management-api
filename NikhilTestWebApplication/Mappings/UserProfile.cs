using AutoMapper;
using NikhilTestWebApplication.Models;

namespace NikhilTestWebApplication.Mappings
{
    public class UserProfile : Profile   // ✅ FIXED
    {
        public UserProfile()
        {
            // Entity → DTO
            CreateMap<User, UserDto>();

            // Create request → Entity
            CreateMap<CreateUserRequest, User>();

            // Update request → Entity (PATCH-like behavior)
            CreateMap<UpdateUserRequestDto, User>()
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}