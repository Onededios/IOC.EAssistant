using IOC.EAssistant.Gateway.Api.Middleware;

namespace IOC.EAssistant.Gateway.Api.Extension;

/// <summary>
/// Provides extension methods for configuring exception handling middleware.
/// </summary>
public static class ExceptionHandlingExtension
{
    /// <summary>
    /// Adds global exception handling middleware to the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for method chaining.</returns>
    /// <remarks>
    /// This middleware should be registered early in the pipeline to catch exceptions
    /// from all subsequent middleware and endpoints. It provides centralized exception
    /// handling with proper logging and consistent error responses.
    /// </remarks>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        return app;
    }
}
