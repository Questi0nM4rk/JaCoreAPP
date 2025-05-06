using AutoMapper;
using JaCore.Api.DTOs.Users;
using JaCore.Api.Entities.Identity;

namespace JaCore.Api.Mappings.User;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // Corrected DTO name
        CreateMap<ApplicationUser, UserDto>()
            // Roles mapping is complex, often handled in the service after mapping.
            .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Ignore roles in direct mapping

        // Corrected DTO name
        CreateMap<UpdateUserDto, ApplicationUser>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Never map ID on update
            .ForMember(dest => dest.UserName, opt => opt.Ignore()) // Usually don't change username
            .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore()) // Usually handle email change separately due to confirmation
            .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Handle password changes separately
            .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
            .ForMember(dest => dest.EmailConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
            .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
            .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshTokens, opt => opt.Ignore())
            // Explicitly map updatable fields present in UpdateUserDto
            // Assuming UpdateUserDto has FirstName, LastName
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));
            // .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber)); // PhoneNumber not in UpdateUserDto
            // Note: IsActive is handled in UserService based on admin privileges

         // Add other User related mappings here
    }
} 