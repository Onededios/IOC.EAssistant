using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace IOC.E_Assistant.Infraestructure.Contracts;
public static class InfrastructureExtension
{
    public static IServiceCollection AddInfraestructureServices(this IServiceCollection services, IConfiguration config)
    {
        return services;
    }
}
