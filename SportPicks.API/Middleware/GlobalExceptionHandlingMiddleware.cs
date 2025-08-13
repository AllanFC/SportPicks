using SportPicks.API.Models;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SportPicks.API.Middleware;

/// <summary>
/// Global exception handling middleware following .NET 9 best practices
/// </summary>
public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during request processing for {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Message = GetUserFriendlyMessage(exception),
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case ArgumentNullException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Required data is missing.";
                break;

            case ArgumentException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Invalid request data provided.";
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = "Authentication required or token has expired.";
                break;

            case InvalidOperationException when exception.Message.Contains("role", StringComparison.OrdinalIgnoreCase):
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Message = "Insufficient permissions to perform this operation.";
                break;

            case TimeoutException:
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                response.Message = "The operation timed out. Please try again.";
                break;

            case HttpRequestException:
                context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
                response.Message = "External service is temporarily unavailable.";
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An internal server error occurred.";
                break;
        }

        // Include detailed error information in development
        if (_environment.IsDevelopment())
        {
            response.Error = exception.ToString();
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => "Required request data is missing.",
            ArgumentException => "Invalid request parameters provided.",
            UnauthorizedAccessException => "You are not authorized to perform this action.",
            TimeoutException => "The operation took too long to complete.",
            HttpRequestException => "External service is currently unavailable.",
            InvalidOperationException => "The requested operation cannot be performed at this time.",
            _ => "An unexpected error occurred."
        };
    }
}