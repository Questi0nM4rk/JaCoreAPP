using Serilog;

namespace JaCore.Api.Extensions; // Adjust namespace

public static class LoggingExtensions
{
    // Extension method assumed by Program.cs if using Serilog
    public static WebApplicationBuilder AddLoggingConfiguration(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            // Add additional configuration here if needed
         );
         return builder;
    }
}
