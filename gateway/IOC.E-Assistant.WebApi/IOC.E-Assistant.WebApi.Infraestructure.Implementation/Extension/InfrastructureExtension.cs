using System;

namespace IOC.E_Assistant.Infraestructure.Implementation.Extension;
public static class InfrastructureExtension
{
    public static IServiceCollection AddInfraestructureServices(this IServiceCollection services, IConfiguration config)
    {
        return services;
    }
}
