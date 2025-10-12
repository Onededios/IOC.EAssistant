using Microsoft.Extensions.DependencyInjection;

namespace IOC.E_Assistant.Library.Implementation.Extension;
public static class LibraryExtension
{
    public static IServiceCollection AddLibraryServices(this IServiceCollection services)
    {
        return services;
    }
}