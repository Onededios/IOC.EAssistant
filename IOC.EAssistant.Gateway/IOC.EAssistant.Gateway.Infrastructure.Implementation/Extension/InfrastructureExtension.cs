using IOC.EAssistant.Gateway.Infrastructure.Contracts.Databases;
using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies;
using IOC.EAssistant.Gateway.Infrastructure.Implementation.Databases;
using IOC.EAssistant.Gateway.Infrastructure.Implementation.Proxies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Extension;
public static class InfrastructureExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDatabaseEAssistant, DatabaseEAssistant>(c =>
        {
            var connstr = configuration["EASSISTANT_CONNSTR"];
            return new DatabaseEAssistant(connstr);
        });
        services.AddScoped<IProxyEAssistant, ProxyEAssistant>(c =>
        {
            var uri = configuration["EASSISTANT_URI"];
            return new ProxyEAssistant(uri);
        });
        return services;
    }
}
