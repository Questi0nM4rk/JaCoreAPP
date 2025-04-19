using JaCore.Api.Data;
using JaCore.Api.Models.Auth;
using JaCore.Api.Models.User;
using JaCore.Api.Services.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace JaCore.Api.Configuration;

public static class AuthConfig
{
    public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Register unified JWT service
        services.AddScoped<IJwtService, JwtService>();
        
        // Configure JwtSettings
        services.Configure<JwtSettings>(configuration.GetSection("JWT"));
        
        // Configure Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
        
        // Configure Authentication and JWT
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        });
        /* // Temporarily comment out AddJwtBearer
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            
            // Check if running in test environment
            if (environment.EnvironmentName == "Test")
            {
                // For test environment, read configuration like other environments
                // but perhaps relax some validation if needed (e.g., lifetime)
                var testSecret = configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret is not configured for Test environment");
                if (string.IsNullOrEmpty(testSecret) || testSecret.Length < 32)
                {
                    throw new InvalidOperationException("JWT:Secret for Test environment must be configured and at least 32 characters long.");
                }
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, // Validate using config
                    ValidateAudience = true, // Validate using config
                    ValidateLifetime = false, // Keep lifetime validation relaxed for testing flexibility
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["JWT:Issuer"], // Read from config
                    ValidAudience = configuration["JWT:Audience"], // Read from config
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(testSecret))
                };
            }
            else
            {
                // For non-test environments, use strict validation from configuration
                var secret = configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret is not configured");
                if (string.IsNullOrEmpty(secret) || secret.Length < 32)
                {
                    // Optional: Add length check for non-test environments too for robustness
                    throw new InvalidOperationException("JWT:Secret must be configured and at least 32 characters long.");
                }
                
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["JWT:Issuer"], // Changed from ValidIssuer
                    ValidAudience = configuration["JWT:Audience"], // Changed from ValidAudience
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                };
            }
        });
        */

        return services;
    }

    public static async Task SeedUsers(IServiceProvider serviceProvider, IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment()) return;
        
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            
        // Create roles if they don't exist
        string[] roles = { "Admin", "Debug", "Management", "User" };
            
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName, $"Role for {roleName} users"));
            }
        }
            
        // Create a default admin user if it doesn't exist
        var adminEmail = "admin@jacore.app";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                IsActive = true
            };
                
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
                
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
            
        // Create a default debug user if it doesn't exist
        var debugEmail = "debug@jacore.app";
        var debugUser = await userManager.FindByEmailAsync(debugEmail);
            
        if (debugUser == null)
        {
            debugUser = new ApplicationUser
            {
                UserName = "debug",
                Email = debugEmail,
                FirstName = "Debug",
                LastName = "User",
                EmailConfirmed = true,
                IsActive = true
            };
                
            var result = await userManager.CreateAsync(debugUser, "Debug123!");
                
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(debugUser, "Debug");
            }
        }
    }
} 