using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using JaCore.Api.Entities.Identity;
using JaCore.Common;
using System;
using AutoMapper;
using JaCore.Api.DTOs.Auth;
using Microsoft.Extensions.Logging;

namespace JaCore.Api.Helpers;

public static class TestDataSeeder
{
    public static async Task SeedEssentialUsersAsync(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        if (environment.EnvironmentName != "Test") return;

        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("TestDataSeeder");

        await SeedUserWithRoleAsync(userManager, roleManager, mapper, logger, RoleConstants.Roles.Admin, "admin@jacore.app", "AdminPassword123!", new RegisterDto { Email = "admin@jacore.app", FirstName = "Admin", LastName = "User" });
        await SeedUserWithRoleAsync(userManager, roleManager, mapper, logger, RoleConstants.Roles.User, "user@jacore.app", "UserPassword123!", new RegisterDto { Email = "user@jacore.app", FirstName = "Standard", LastName = "User" });
        await SeedUserWithRoleAsync(userManager, roleManager, mapper, logger, RoleConstants.Roles.Management, "management@jacore.app", "ManagementPassword123!", new RegisterDto { Email = "management@jacore.app", FirstName = "Management", LastName = "User" });
        await SeedUserWithRoleAsync(userManager, roleManager, mapper, logger, RoleConstants.Roles.Debug, "debug@jacore.app", "DebugPassword123!", new RegisterDto { Email = "debug@jacore.app", FirstName = "Debug", LastName = "User" });
    }

    private static async Task SeedUserWithRoleAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IMapper mapper,
        ILogger logger,
        string roleName,
        string email,
        string password,
        RegisterDto userDetailsDto)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var errorMsg = $"Required role '{roleName}' does not exist. Ensure roles are seeded before users.";
            logger.LogError(errorMsg);
            throw new InvalidOperationException(errorMsg);
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = mapper.Map<ApplicationUser>(userDetailsDto);
            user.EmailConfirmed = true;
            user.IsActive = true;

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Failed to create test user {Email}: {Errors}", email, errors);
                throw new InvalidOperationException($"Failed to create test user {email}: {errors}");
            }
            logger.LogInformation("Created test user {Email} with ID {UserId}", email, user.Id);

            var roleResult = await userManager.AddToRoleAsync(user, roleName);
            if (!roleResult.Succeeded)
            {
                var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                logger.LogError("Failed to assign role '{RoleName}' to new user {Email}: {Errors}", roleName, email, roleErrors);
                throw new InvalidOperationException($"Failed to assign role '{roleName}' to new user {email}: {roleErrors}");
            }
            logger.LogInformation("Assigned role '{RoleName}' to new user {Email}", roleName, email);
        }
        else
        {
            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                var roleResult = await userManager.AddToRoleAsync(user, roleName);
                if (!roleResult.Succeeded)
                {
                    var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    logger.LogError("Failed to assign role '{RoleName}' to existing user {Email}: {Errors}", roleName, email, roleErrors);
                    throw new InvalidOperationException($"Failed to assign role '{roleName}' to existing user {email}: {roleErrors}");
                }
                logger.LogInformation("Assigned role '{RoleName}' to existing user {Email}", roleName, email);
            }
        }
    }
}
