// Program.cs (Top-Level Statements, Production Standards, No Seeding)
using JaCore.Api.Data;
using JaCore.Api.Models.User;
using JaCore.Api.Services.User;
using JaCore.Api.Services.Authentication;
using JaCore.Api.Middleware; // For UseExceptionHandling
using JaCore.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using JaCore.Common;

// Assembly Attributes for testing visibility
[assembly: InternalsVisibleTo("JaCore.ApiO.Tests")]
[assembly: InternalsVisibleTo("JaCore.ApiO.IntegrationTests")]

// --- Top-Level Statements Start Here ---

// Bootstrap logger (essential for capturing early startup errors)
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
Log.Information("Starting JaCore API host builder...");

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;
    var environment = builder.Environment;

    // --- Service Configuration ---

    // Logging (Using your extension)
    builder.AddLoggingConfiguration();

    // Standard Services
    builder.Services.AddHttpContextAccessor(); // Often useful for accessing HttpContext in services
    builder.Services.AddResponseCompression(options =>
    {
        options.Providers.Add<GzipCompressionProvider>();
        options.EnableForHttps = true; // Enable compression for HTTPS
    });
    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Optimal; // Use Optimal for better compression in prod, Fastest for dev if needed
    });

    // Controllers & API Versioning
    builder.Services.AddControllers();
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true; // Good practice to report supported versions
        options.ApiVersionReader = new UrlSegmentApiVersionReader(); // Standard: /api/v1/...
    });
    builder.Services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV"; // Formats group names like v1, v2 in Swagger
        options.SubstituteApiVersionInUrl = true; // Replaces {version:apiVersion} in route templates
    });

    // Database
    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString)); // Replace UseSqlServer if using PostgreSQL/etc.

    // Identity
    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        // Production Standard Password Policy
        options.Password.RequiredLength = 10; // Increase minimum length
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredUniqueChars = 1; // Requires at least 1 unique char

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5; // Lock after 5 failed attempts
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.RequireUniqueEmail = true;
        // options.SignIn.RequireConfirmedAccount = true; // Strongly recommended for production
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // Needed for password reset, email confirmation, etc.

    // Authentication (JWT)
    var jwtSecret = configuration[ApiConstants.JwtConfigKeys.Secret]
        ?? throw new InvalidOperationException($"JWT Secret '{ApiConstants.JwtConfigKeys.Secret}' is not configured.");
    if (jwtSecret.Length < 32) // Enforce minimum length at startup
    {
        throw new InvalidOperationException("JWT Secret must be at least 32 characters long.");
    }
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Validate the server that generates the token
            ValidateAudience = true, // Validate the recipient of the token is authorized to receive
            ValidateLifetime = true, // Check if the token is not expired and that the signing key is valid
            ValidateIssuerSigningKey = true, // Validate the signature of the token
            ValidIssuer = configuration[ApiConstants.JwtConfigKeys.Issuer], // Should match issuer in token generation
            ValidAudience = configuration[ApiConstants.JwtConfigKeys.Audience], // Should match audience in token generation
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero // Remove clock skew, rely on accurate server time
        };
    });

    // Authorization (Policies)
    builder.Services.AddAuthorization(options =>
    {
        // Keep your well-defined policies
        options.AddPolicy(ApiConstants.Policies.ReadOnly, policy => policy.RequireRole(ApiConstants.Roles.User, ApiConstants.Roles.Management, ApiConstants.Roles.Admin, ApiConstants.Roles.Debug));
        options.AddPolicy(ApiConstants.Policies.ReadWrite, policy => policy.RequireRole(ApiConstants.Roles.Management, ApiConstants.Roles.Admin, ApiConstants.Roles.Debug));
        options.AddPolicy(ApiConstants.Policies.FullAccess, policy => policy.RequireRole(ApiConstants.Roles.Admin, ApiConstants.Roles.Debug));
        options.AddPolicy(ApiConstants.Policies.AdminOnly, policy => policy.RequireRole(ApiConstants.Roles.Admin));
        options.AddPolicy(ApiConstants.Policies.SystemOperations, policy => policy.RequireRole(ApiConstants.Roles.Debug, ApiConstants.Roles.Admin));

        // Fallback policy enforces authentication by default
        options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    });

    // Application Services & Repositories
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddRepositories(); // Your repository registration extension

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>(); // Basic DB connectivity check
    // builder.Services.AddHealthChecksConfiguration(configuration, environment); // Or your more complex setup

    // Swagger / OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        // Add document info per version
        // Use BuildServiceProvider ONLY if IApiVersionDescriptionProvider isn't available otherwise
        // It's generally better to configure options using dependencies available later if possible
        var apiVersionDescriptionProvider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = $"JaCore API {description.ApiVersion}",
                Version = description.ApiVersion.ToString()
            });
        }

        // Define JWT Bearer security scheme
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        // Apply security requirement globally
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ProductionPolicy", policy => // Define a more restrictive policy for production
        {
            policy.WithOrigins("https://your-frontend-domain.com") // Specify allowed origins
                  .AllowAnyMethod() // Or restrict methods (GET, POST, etc.)
                  .AllowAnyHeader(); // Or restrict headers
        });
        options.AddPolicy("DevelopmentPolicy", policy => // Keep a permissive policy for local dev
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
    });


    // --- Build App ---
    var app = builder.Build();


    // --- Middleware Pipeline (Order Matters!) ---

    // 1. Exception Handling (Early)
    app.UseExceptionHandling(); // Your custom handler

    // 2. Security Headers (Optional but Recommended)
    // app.UseHsts(); // Enforces HTTPS (only if UseHttpsRedirection is also used) - see below
    // app.UseXXssProtection(options => options.EnabledWithBlockMode());
    // app.UseXContentTypeOptions();
    // app.UseReferrerPolicy(opts => opts.NoReferrer());
    // app.Use(async (context, next) => {
    //     context.Response.Headers.Add("X-Frame-Options", "DENY");
    //     await next();
    // });

    // 3. HTTPS Redirection (Before HSTS)
    app.UseHttpsRedirection();

    // Enable HSTS only in production after confirming HTTPS works correctly
    if (!environment.IsDevelopment())
    {
        app.UseHsts();
    }

    // 4. Static Files (if serving any directly, e.g., wwwroot)
    // app.UseStaticFiles();

    // 5. Routing
    app.UseRouting(); // Marks the position where routing decisions are made

    // 6. CORS (After Routing, Before Auth/Endpoints)
    // Use different policies for different environments
    app.UseCors(environment.IsDevelopment() ? "DevelopmentPolicy" : "ProductionPolicy");

    // 7. Authentication (Identifies the user)
    app.UseAuthentication();

    // 8. Authorization (Checks user permissions)
    app.UseAuthorization();

    // 9. Response Compression (Can often go here, after Auth/Authz but before endpoints)
    app.UseResponseCompression();

    // 10. Endpoints (Controllers, Minimal APIs, Health Checks)
    app.MapControllers();
    app.UseHealthChecksConfiguration(); // Your endpoint mapping extension or MapHealthChecks("/healthz")


    // --- NO SEEDING HERE ---
    // Seeding should be handled via separate mechanisms for production readiness


    // --- Run Application ---
    Log.Information("Starting application...");
    await app.RunAsync();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    Environment.ExitCode = 1; // Indicate failure
}
finally
{
    Log.CloseAndFlush(); // Ensure logs are written
}

// --- Partial Program Class for Logger ---
// Keep if ILogger<Program> is used by extensions or other parts of the setup
namespace JaCore.Api
{
    public partial class Program { }
}
