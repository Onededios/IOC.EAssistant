using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Implementation.Services;
using IOC.EAssistant.Gateway.Library.Implementation.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace IOC.EAssistant.Gateway.Library.Implementation.Extension;
public static class LibraryExtension
{
    public static IServiceCollection AddLibraryServices(this IServiceCollection services)
    {
        services.AddScoped<IServiceSession, ServiceSession>();
        services.AddScoped<IServiceConversation, ServiceConversation>();
        services.AddScoped<IServiceAnswer, ServiceAnswer>();
        services.AddScoped<IServiceQuestion, ServiceQuestion>();
        services.AddScoped<IServiceChat, ServiceChat>();
        services.AddScoped<IServiceHealthCheck, ServiceHealthCheck>();
        services.AddScoped<ValidatorChat>();
        return services;
    }
}
