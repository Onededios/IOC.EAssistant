using Dapper;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
using IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases.EAssistant;
using IOC.EAssistant.Gateway.Infrastructure.Implementation.Helpers;
using IOC.EAssistant.Gateway.Infrastructure.Implementation.Proxies;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Extension;
public static class InfrastructureExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connstr = configuration["EASSISTANT_CONNSTR"];

        services.AddScoped(sp => new DatabaseEAssistantAnswer(connstr));
        services.AddScoped<IDatabaseEAssistantAnswer>(sp => sp.GetRequiredService<DatabaseEAssistantAnswer>());
        services.AddScoped<IDatabaseEAssistantBase<Answer>>(sp => sp.GetRequiredService<DatabaseEAssistantAnswer>());

        services.AddScoped(sp => new DatabaseEAssistantQuestion(connstr)!);
        services.AddScoped<IDatabaseEAssistantQuestion>(sp => sp.GetRequiredService<DatabaseEAssistantQuestion>());
        services.AddScoped<IDatabaseEAssistantBase<Question>>(sp => sp.GetRequiredService<DatabaseEAssistantQuestion>());

        services.AddScoped(sp => new DatabaseEAssistantConversation(connstr)!);
        services.AddScoped<IDatabaseEAssistantConversation>(sp => sp.GetRequiredService<DatabaseEAssistantConversation>());
        services.AddScoped<IDatabaseEAssistantBase<Conversation>>(sp => sp.GetRequiredService<DatabaseEAssistantConversation>());

        services.AddScoped(sp => new DatabaseEAssistantSession(connstr)!);
        services.AddScoped<IDatabaseEAssistantSession>(sp => sp.GetRequiredService<DatabaseEAssistantSession>());
        services.AddScoped<IDatabaseEAssistantBase<Session>>(sp => sp.GetRequiredService<DatabaseEAssistantSession>());

        services.AddHttpClient();

        services.AddScoped<IProxyEAssistant, ProxyEAssistant>(sp =>
        {
            var uri = configuration["EASSISTANT_URI"];
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new ProxyEAssistant(uri, httpClientFactory);
        });

        SqlMapper.AddTypeHandler(new JsonObjectTypeHandler());

        return services;
    }
}
