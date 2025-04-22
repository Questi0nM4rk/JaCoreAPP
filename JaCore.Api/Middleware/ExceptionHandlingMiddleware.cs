using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; // Needed for ProblemDetails
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace JaCore.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next; // The next middleware in the pipeline
    private readonly ILogger<ExceptionHandlingMiddleware> _logger; // For logging errors
    private readonly IHostEnvironment _env; // To check if running in Development/Test

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Call the next middleware in the pipeline
            await _next(context);
        }
        catch (Exception ex) // Catch ANY unhandled exception
        {
            // Log the error with details
            _logger.LogError(ex, "Unhandled exception for request {Path}", context.Request.Path);

            // Prepare a standard ProblemDetails response
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var problemDetails = new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = "An internal server error occurred.",
                // Provide more detail only in non-production environments
                Detail = _env.IsDevelopment() || _env.IsEnvironment("Test")
                    ? $"{ex.Message} \n{ex.StackTrace}"
                    : "Please contact support.",
                Instance = context.Request.Path // Identify the request path
            };

            // Write the JSON response
            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}

// Extension method for easy registration in Program.cs
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
