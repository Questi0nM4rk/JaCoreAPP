using JaCore.Api.Data;
using JaCore.Api.Entities.Identity;
using JaCore.Api.Extensions;
using JaCore.Api.Middleware;
using JaCore.Api.Services.Abstractions;
using JaCore.Api.Services.Auth;
using JaCore.Api.Services.Repositories;
using JaCore.Api.Services.Repositories.Device;
using JaCore.Api.Services.Users;
using JaCore.Api.Services.Device;
using JaCore.Common;
using JaCore.Api.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Runtime.CompilerServices;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;

[assembly: InternalsVisibleTo("JaCore.Api.Tests")]
[assembly: InternalsVisibleTo("JaCore.Api.IntegrationTests")]

try
{
    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;
    var environment = builder.Environment;

    builder.Host.UseSerilog((context, loggerConfig) =>
        loggerConfig.ReadFrom.Configuration(context.Configuration)
    );

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllers();

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = ApiConstants.Versions.V1_0;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    });
    builder.Services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found.");
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

    builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredUniqueChars = 1;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    var jwtSecret = configuration[ApiConstants.JwtConfigKeys.Secret]
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
            ValidIssuer = configuration[ApiConstants.JwtConfigKeys.Issuer],
            ValidAudience = configuration[ApiConstants.JwtConfigKeys.Audience],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(ApiConstants.Policies.AdminOnly, policy => policy.RequireRole(RoleConstants.Roles.Admin, RoleConstants.Roles.Debug));
        options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    });

    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

    builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
    builder.Services.AddScoped<IDeviceCardRepository, DeviceCardRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
    builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
    builder.Services.AddScoped<IEventRepository, EventRepository>();
    builder.Services.AddScoped<IDeviceOperationRepository, DeviceOperationRepository>();

    builder.Services.AddScoped<IDeviceService, DeviceService>();
    builder.Services.AddScoped<IDeviceCardService, DeviceCardService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<ISupplierService, SupplierService>();
    builder.Services.AddScoped<IServiceService, ServiceService>();
    builder.Services.AddScoped<IEventService, EventService>();
    builder.Services.AddScoped<IDeviceOperationService, DeviceOperationService>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("BearerAuth", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "BearerAuth"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("Database");

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    if (!environment.IsDevelopment()) app.UseHsts();
    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseExceptionHandling(); // Custom middleware for exception handling

    app.UseAuthentication();
    app.UseAuthorization();

    if (environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
             var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
             foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse()) {
                 options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"API {description.GroupName.ToUpperInvariant()}");
             }
             options.RoutePrefix = string.Empty;
        });
    }

    app.MapControllers();
    app.MapHealthChecks("/healthz");

    Log.Information("Starting JaCore API application...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally 
{
    // Ensure logs are flushed on shutdown
    Log.CloseAndFlush(); 
}

// --- Partial Program Class for Accessibility ---
// Make the partial class public so WebApplicationFactory<Program> can access it from the test project
namespace JaCore.Api { public partial class Program { } }
