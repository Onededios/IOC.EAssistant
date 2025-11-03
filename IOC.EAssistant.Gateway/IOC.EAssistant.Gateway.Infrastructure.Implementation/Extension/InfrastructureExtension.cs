using Dapper;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
using IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases.EAssistant;
using IOC.EAssistant.Gateway.Infrastructure.Implementation.Helpers;
using IOC.EAssistant.Gateway.Infrastructure.Implementation.Proxies;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Extension;
public static class InfrastructureExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connstr = configuration["EASSISTANT_CONNSTR"];
        services.AddScopedDatabase<Answer, IDatabaseEAssistantAnswer, DatabaseEAssistantAnswer>(connstr);
        services.AddScopedDatabase<Question, IDatabaseEAssistantQuestion, DatabaseEAssistantQuestion>(connstr);
        services.AddScopedDatabase<Conversation, IDatabaseEAssistantConversation, DatabaseEAssistantConversation>(connstr);
        services.AddScopedDatabase<Session, IDatabaseEAssistantSession, DatabaseEAssistantSession>(connstr);

        services.AddScoped<IProxyEAssistant, ProxyEAssistant>(c =>
        {
            var uri = configuration["EASSISTANT_URI"];
            return new ProxyEAssistant(uri);
        });

        SqlMapper.AddTypeHandler(new JsonObjectTypeHandler());

        return services;
    }

    private static IServiceCollection AddScopedDatabase<TEntity, TInterface, TImplementation>(
            this IServiceCollection services,
            string connstr
        )
        where TImplementation : class, TInterface, IDatabaseEAssistantBase<TEntity>
        where TInterface : class
    {
        services.AddScoped(sp => (TImplementation)Activator.CreateInstance(typeof(TImplementation), connstr)!);
        services.AddScoped<TInterface>(sp => sp.GetRequiredService<TImplementation>());
        services.AddScoped<IDatabaseEAssistantBase<TEntity>>(sp => sp.GetRequiredService<TImplementation>());

        return services;
    }
}
