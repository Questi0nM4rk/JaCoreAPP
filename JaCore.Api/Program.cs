// Program.cs (Minimal Top-Level for Controller-Based User API - Emphasizing Interfaces/MVC)
using JaCore.Api.Data;
using JaCore.Api.Entities;
using JaCore.Api.Extensions; // Assuming Logger, Repositories extensions exist
using JaCore.Api.Middleware; // For UseExceptionHandling
using JaCore.Api.Services; // Where IAuthService, IUserService, AuthService, UserService live
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog; // Assuming used via AddLoggingConfiguration
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using JaCore.Api.Services.Auth;
using JaCore.Api.Entities.Identity;
using JaCore.Common;
using JaCore.Api.Services.Abstractions;
using JaCore.Api.Services.Users;
using JaCore.Api.Helpers; // Add using for the new constants
using FluentValidation; // Add using for FluentValidation
using FluentValidation.AspNetCore; // Add using for ASP.NET Core integration
using System.Reflection; // Add using for Assembly
using JaCore.Api.Services.Repositories; // Add using for Repositories

[assembly: InternalsVisibleTo("JaCore.Api.Tests")]
[assembly: InternalsVisibleTo("JaCore.Api.IntegrationTests")]

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
Log.Information("Starting JaCore API host builder...");

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;
    var environment = builder.Environment;

    // --- Configure Services ---

    builder.AddLoggingConfiguration(); // Setup basic logging
    builder.Services.AddHttpContextAccessor();

    // **Register services needed for MVC Controllers**
    builder.Services.AddControllers();

    // *** ADD FluentValidation ***
    // Automatically registers all validators from the assembly containing Program
    builder.Services.AddFluentValidationAutoValidation(); // Enables auto validation pipeline
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    // **Configure API Versioning for Controllers**
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = ApiConstants.Versions.V1_0; // Use static ApiVersion object
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader(); // Standard: /api/v1/...
    });
    builder.Services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV"; // Format groups as v1, v2, etc.
        options.SubstituteApiVersionInUrl = true; // Replace {version:apiVersion} in route templates
    });

    // Database Context (KEEP THIS)
    var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found.");
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString)); // Add this line for PostgreSQL

    // Identity
    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        // Production Standard Password Policy
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredUniqueChars = 1;
        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
        // User settings
        options.User.RequireUniqueEmail = true;
        // options.SignIn.RequireConfirmedAccount = true; // Recommended
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // Authentication (JWT)
    var jwtSecret = configuration[ApiConstants.JwtConfigKeys.Secret] // Use new constant
            ?? throw new InvalidOperationException($"JWT Secret '{ApiConstants.JwtConfigKeys.Secret}' is not configured.");
    if (jwtSecret.Length < 32) throw new InvalidOperationException("JWT Secret must be at least 32 characters long.");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration[ApiConstants.JwtConfigKeys.Issuer], // Use new constant
            ValidAudience = configuration[ApiConstants.JwtConfigKeys.Audience], // Use new constant
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero // Be strict with expiration
        };
    });

    // Authorization (Policies used by Controller [Authorize] attributes)
    builder.Services.AddAuthorization(options =>
    {
        // Use new Policy constant and RENAMED Common RoleConstants
        options.AddPolicy(ApiConstants.Policies.AdminOnly, policy => policy.RequireRole(RoleConstants.Roles.Admin, RoleConstants.Roles.Debug));
        // Define other policies...
        options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    });

    // **Register Application Services using Interfaces (Dependency Injection)**
    builder.Services.AddScoped<IAuthService, AuthService>(); // Maps interface to implementation
    builder.Services.AddScoped<IUserService, UserService>(); // Maps interface to implementation
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>(); // Add this line

    // Swagger / OpenAPI (for testing controllers)
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        // Define the BearerAuth security scheme (keep this)
        options.AddSecurityDefinition("BearerAuth", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer", // Must be lowercase
            BearerFormat = "JWT"
        });

        // Make sure swagger UI requires a Bearer token to be specified (keep this)
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "BearerAuth" // Must match the definition name
                    }
                },
                Array.Empty<string>()
            }
        });

        // *** REMOVED version iteration logic from here ***
    });

    // *** ADD Health Checks ***
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("Database"); // Check DB connection via EF Core


    // --- Build the App --- (KEEP THIS)
    var app = builder.Build();


    // --- Configure Middleware Pipeline ---
    // Use custom exception handler OR built-in ones
    app.UseExceptionHandling();

    if (!environment.IsDevelopment()) app.UseHsts();
    app.UseHttpsRedirection();

    app.UseRouting(); // Define endpoint routes

    // CORS would go here IF needed: app.UseCors("PolicyName");

    app.UseAuthentication(); // Identify user via token
    app.UseAuthorization(); // Check user permissions

    // Swagger UI (Conditional)
    if (environment.IsDevelopment() || environment.IsEnvironment("Test"))
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
             // *** MOVED the version iteration logic here ***
             // Retrieve the provider from the built app's services
             var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
             // Add an endpoint for each discovered API version
             foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse()) {
                 // Use new Version constant for title
                 options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"API {description.GroupName.ToUpperInvariant()}");
             }
             options.RoutePrefix = string.Empty; // Serve UI at root (keep this)
        });
    }

    app.MapControllers(); // Map requests to controller actions

    // *** MAP Health Checks Endpoint ***
    app.MapHealthChecks("/healthz"); // Expose health status at /healthz


    // --- Run Application --- (KEEP THIS)
    Log.Information("Starting application...");
    await app.RunAsync();
}
catch (Exception ex) { Log.Fatal(ex, "App terminated"); Environment.ExitCode = 1; }
finally { Log.CloseAndFlush(); }

// --- Partial Program Class for Logger ---
namespace JaCore.Api { public partial class Program { } }
