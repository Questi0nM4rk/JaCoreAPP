using System.Net;
using System.Text.Json;

namespace JaCore.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError; // 500 if unexpected
        var result = string.Empty;

        switch (exception)
        {
            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                break;
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                break;
            case ArgumentException:
            case InvalidOperationException:
                code = HttpStatusCode.BadRequest;
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        if (_environment.IsDevelopment())
        {
            // Return detailed error information in development
            result = JsonSerializer.Serialize(new
            {
                error = new
                {
                    message = exception.Message,
                    detail = exception.StackTrace,
                    exceptionType = exception.GetType().Name
                },
                statusCode = (int)code
            });
        }
        else
        {
            // Return minimal error information in production
            result = JsonSerializer.Serialize(new
            {
                error = new
                {
                    message = "An error occurred processing your request.",
                    status = (int)code
                }
            });
        }

        return context.Response.WriteAsync(result);
    }
}

// Extension method for middleware registration
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
} 