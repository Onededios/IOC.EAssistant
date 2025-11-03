using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
using IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases.EAssistant;
using IOC.EAssistant.Gateway.Infrastructure.Implementation.Proxies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Extension;
public static class InfrastructureExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connstr = configuration["EASSISTANT_CONNSTR"];

        services.AddScoped<IDatabaseEAssistantQuestion, DatabaseEAssistantQuestion>(c => new DatabaseEAssistantQuestion(connstr));
        services.AddScoped<IDatabaseEAssistantAnswer, DatabaseEAssistantAnswer>(c => new DatabaseEAssistantAnswer(connstr));
        services.AddScoped<IDatabaseEAssistantConversation, DatabaseEAssistantConversation>(c => new DatabaseEAssistantConversation(connstr));
        services.AddScoped<IDatabaseEAssistantSession, DatabaseEAssistantSession>(c => new DatabaseEAssistantSession(connstr));
        services.AddScoped<IProxyEAssistant, ProxyEAssistant>(c =>
        {
            var uri = configuration["EASSISTANT_URI"];
            return new ProxyEAssistant(uri);
        });
        return services;
    }
}
