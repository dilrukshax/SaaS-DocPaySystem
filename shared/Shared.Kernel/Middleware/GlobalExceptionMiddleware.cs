using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace Shared.Kernel.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false,
            Data = null,
            Errors = new List<string>()
        };

        switch (exception)
        {
            case UnauthorizedAccessException:
                response.Errors.Add("Unauthorized access");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;
            
            case ArgumentException argEx:
                response.Errors.Add(argEx.Message);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            
            case InvalidOperationException invalidOpEx:
                response.Errors.Add(invalidOpEx.Message);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            
            case KeyNotFoundException:
                response.Errors.Add("Resource not found");
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;
            
            case FluentValidation.ValidationException validationEx:
                response.Errors.AddRange(validationEx.Errors.Select(e => e.ErrorMessage));
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            
            default:
                response.Errors.Add("An internal server error occurred");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public static class ApiResponseExtensions
{
    public static ApiResponse<T> ToSuccessResponse<T>(this T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<object> ToErrorResponse(this IEnumerable<string> errors)
    {
        return new ApiResponse<object>
        {
            Success = false,
            Data = null,
            Errors = errors.ToList()
        };
    }

    public static ApiResponse<object> ToErrorResponse(this string error)
    {
        return new ApiResponse<object>
        {
            Success = false,
            Data = null,
            Errors = new List<string> { error }
        };
    }
}
