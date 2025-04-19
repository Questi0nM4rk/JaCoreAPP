using JaCore.Api.Models.User; // Correct namespace for ApplicationUser

namespace JaCore.Api.IntegrationTests;

public static class TestUsers
{
    public static ApplicationUser GetTestUser(string role)
    {
        return role switch
        {
            "Admin" => new ApplicationUser
            {
                Email = ApiWebApplicationFactory.AdminEmail,
                UserName = ApiWebApplicationFactory.AdminEmail, // Often UserName is the same as Email
                EmailConfirmed = true // Assume emails are confirmed for tests
            },
            "Management" => new ApplicationUser
            {
                Email = ApiWebApplicationFactory.ManagementEmail,
                UserName = ApiWebApplicationFactory.ManagementEmail,
                EmailConfirmed = true
            },
            "Debug" => new ApplicationUser
            {
                Email = ApiWebApplicationFactory.DebugEmail,
                UserName = ApiWebApplicationFactory.DebugEmail,
                EmailConfirmed = true
            },
            "User" or _ => new ApplicationUser // Default to User role
            {
                Email = ApiWebApplicationFactory.UserEmail,
                UserName = ApiWebApplicationFactory.UserEmail,
                EmailConfirmed = true
            },
        };
    }
} 