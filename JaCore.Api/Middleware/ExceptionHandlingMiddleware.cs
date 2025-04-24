using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; // Needed for ProblemDetails
using Microsoft.Extensions.Hosting; // Keep for potential future use, though not strictly needed for the change
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace JaCore.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next; // The next middleware in the pipeline
    private readonly ILogger<ExceptionHandlingMiddleware> _logger; // For logging errors

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the original exception with full details (KEEP THIS)
            _logger.LogError(ex, "Unhandled exception for request {Path}", context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = "An internal server error occurred.",
                // Always return a generic detail message, regardless of environment
                Detail = "An unexpected error occurred processing your request. Please contact support.", 
                Instance = context.Request.Path
            };

            try
            {
                // Use WriteAsJsonAsync for proper serialization
                await context.Response.WriteAsJsonAsync(problemDetails); 
            }
            catch (Exception writeEx)
            {
                // Log if writing the error response itself fails (KEEP THIS)
                _logger.LogError(writeEx, "Failed to write error response for request {Path}", context.Request.Path);
                // Optionally, write a plain text fallback (KEEP THIS)
                context.Response.StatusCode = StatusCodes.Status500InternalServerError; // Ensure status code is set even for fallback
                context.Response.ContentType = "text/plain"; // Set content type for plain text
                await context.Response.WriteAsync("A fatal error occurred and the error response could not be serialized.");
            }
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
