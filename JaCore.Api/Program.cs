using JaCore.Api.Data;
using JaCore.Api.Interfaces.Repositories.Device;
using JaCore.Api.Models.User;
using JaCore.Api.Repositories.Device;
using JaCore.Api.Services.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using JaCore.Api.Configuration;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using System.IO.Compression;
using JaCore.Api.Middleware;
using JaCore.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

[assembly: InternalsVisibleTo("JaCore.Api.Tests")]

namespace JaCore.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add Serilog logging
                builder.AddLoggingConfiguration();

                // Enable response compression
                builder.Services.AddResponseCompression(options =>
                {
                    options.Providers.Add<GzipCompressionProvider>();
                    options.EnableForHttps = true;
                });
                
                builder.Services.Configure<GzipCompressionProviderOptions>(options =>
                {
                    options.Level = CompressionLevel.Fastest;
                });

                // Configure API controllers
                builder.Services.AddControllers(options =>
                {
                    // Add consistent API versioning
                    options.UseRoutePrefix(ApiConstants.ApiRoutePrefix);
                });
                
                // API versioning
                builder.Services.AddApiVersioning(options =>
                {
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ReportApiVersions = true;
                });

                // Configure essential services
                builder.Services.AddDatabaseConfiguration(builder.Configuration, builder.Environment);
                builder.Services.AddRepositories();
                builder.Services.AddApplicationServices();
                builder.Services.AddAuthenticationConfiguration(builder.Configuration, builder.Environment);

                // Configure authorization policies
                builder.Services.AddAuthorization(options =>
                {
                    // Read-only policy - allows viewing resources
                    options.AddPolicy(ApiConstants.Policies.ReadOnly, policy =>
                        policy.RequireRole(ApiConstants.Roles.User, ApiConstants.Roles.Management, 
                                         ApiConstants.Roles.Admin, ApiConstants.Roles.Debug));

                    // Read-write policy - allows modifications to resources
                    options.AddPolicy(ApiConstants.Policies.ReadWrite, policy =>
                        policy.RequireRole(ApiConstants.Roles.Management, ApiConstants.Roles.Admin, 
                                         ApiConstants.Roles.Debug));

                    // Full access policy - allows all operations including delete
                    options.AddPolicy(ApiConstants.Policies.FullAccess, policy =>
                        policy.RequireRole(ApiConstants.Roles.Admin, ApiConstants.Roles.Debug));

                    // Admin-only policy - restricts to admin users
                    options.AddPolicy(ApiConstants.Policies.AdminOnly, policy =>
                        policy.RequireRole(ApiConstants.Roles.Admin));

                    // System operations policy - for monitoring and debugging
                    options.AddPolicy(ApiConstants.Policies.SystemOperations, policy =>
                        policy.RequireRole(ApiConstants.Roles.Debug, ApiConstants.Roles.Admin));

                    // Set fallback policy to require authentication by default
                    options.FallbackPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                });

                // Add health checks for system monitoring
                builder.Services.AddHealthChecksConfiguration(builder.Configuration, builder.Environment);

                // Configure Swagger documentation
                builder.Services.AddSwaggerConfiguration();

                // Configure CORS
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
                });

                var app = builder.Build();

                // Use response compression
                app.UseResponseCompression();
                
                // Use global exception handling middleware
                app.UseExceptionHandling();

                // Configure Swagger UI for development and test environments
                if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(options =>
                    {
                        options.SwaggerEndpoint("/swagger/v1/swagger.json", "JaCore API v1");
                        options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
                    });
                }

                app.UseHttpsRedirection();

                // Apply CORS policy
                app.UseCors("AllowAll");
                
                // Configure proper middleware order
                app.UseRouting();
                
                // Add authentication and authorization middleware
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();
                   // No need for RequireAuthorization here since we have FallbackPolicy

                // Configure health checks endpoints
                app.UseHealthChecksConfiguration();

                // Seed data in development environment
                if (app.Environment.IsDevelopment())
                {
                    // Uncomment when needed for data seeding
                    // DatabaseConfig.SeedDatabase(app.Services, app.Environment);
                    // await AuthConfig.SeedUsers(app.Services, app.Environment);
                }

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
    
    // Extension methods for cleaner middleware configuration
    public static class ApplicationBuilderExtensions
    {
        // Extension method for applying route prefix to all controllers
        public static MvcOptions UseRoutePrefix(this MvcOptions opts, string prefix)
        {
            var routeAttribute = new RouteAttribute(prefix);
            
            opts.Conventions.Add(new RoutePrefixConvention(routeAttribute));
            
            return opts;
        }
    }
    
    // Convention for applying route prefix
    public class RoutePrefixConvention(RouteAttribute routeAttribute) : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            foreach (var selector in controller.Selectors)
            {
                if (selector.AttributeRouteModel != null)
                {
                    selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                        new AttributeRouteModel(routeAttribute),
                        selector.AttributeRouteModel
                    );
                }
            }
        }
    }
}
