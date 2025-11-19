using IOC.EAssistant.Gateway.XCutting.Results;
using System.Net;
using System.Text.Json;

namespace IOC.EAssistant.Gateway.Api.Middleware;

/// <summary>
/// Middleware for handling unhandled exceptions globally across the application.
/// </summary>
/// <remarks>
/// <para>
/// This middleware catches all unhandled exceptions that propagate from controllers and services,
/// providing centralized exception handling with:
/// <list type="bullet">
/// <item><description>Consistent error response format</description></item>
/// <item><description>Appropriate HTTP status codes</description></item>
/// <item><description>Structured logging with full exception details</description></item>
/// <item><description>Security-conscious error messages (no sensitive data exposure)</description></item>
/// </list>
/// </para>
/// <para>
/// The middleware categorizes exceptions and returns appropriate HTTP status codes:
/// <list type="bullet">
/// <item><description>400 Bad Request - For validation and argument exceptions</description></item>
/// <item><description>404 Not Found - For key/item not found exceptions</description></item>
/// <item><description>408 Request Timeout - For timeout exceptions</description></item>
/// <item><description>500 Internal Server Error - For all other exceptions</description></item>
/// </list>
/// </para>
/// </remarks>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandlerMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance for recording exceptions.</param>
    /// <param name="environment">The hosting environment for determining error detail level.</param>
    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Invokes the middleware to handle HTTP requests and catch exceptions.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception occurred. Request: {Method} {Path}. User: {User}",
                context.Request.Method,
                context.Request.Path,
                context.User?.Identity?.Name ?? "Anonymous"
            );

            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles exceptions by creating appropriate error responses.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="exception">The exception that was thrown.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errorResult) = GetErrorResponse(exception);
        context.Response.StatusCode = statusCode;

        var operationResult = new OperationResult<object>
        {
            Status = statusCode,
            Instance = context.Request.Path
        };

        operationResult.AddError(errorResult, exception);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(operationResult, options);
        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// Determines the appropriate HTTP status code and error message based on the exception type.
    /// </summary>
    /// <param name="exception">The exception to analyze.</param>
    /// <returns>A tuple containing the HTTP status code and error result.</returns>
    private (int statusCode, ErrorResult errorResult) GetErrorResponse(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException or ArgumentException => ((int)HttpStatusCode.BadRequest, new ErrorResult("Invalid request parameters", "Bad Request")),

            InvalidOperationException => ((int)HttpStatusCode.BadRequest, new ErrorResult("The operation could not be completed", "Invalid Operation")),

            KeyNotFoundException => ((int)HttpStatusCode.NotFound, new ErrorResult("The requested resource was not found", "Not Found")),

            TimeoutException => ((int)HttpStatusCode.RequestTimeout, new ErrorResult("The request timed out", "Timeout")),

            HttpRequestException httpEx => ((int)(httpEx.StatusCode ?? HttpStatusCode.BadGateway), new ErrorResult("Error communicating with external service", "External Service Error")),

            UnauthorizedAccessException => ((int)HttpStatusCode.Forbidden, new ErrorResult("You do not have permission to perform this action", "Forbidden")),

            _ => ((int)HttpStatusCode.InternalServerError, new ErrorResult(_environment.IsDevelopment() ? $"An error occurred: {exception.Message}" : "An internal error occurred. Please try again later.", "Internal Server Error"))
        };
    }
}
