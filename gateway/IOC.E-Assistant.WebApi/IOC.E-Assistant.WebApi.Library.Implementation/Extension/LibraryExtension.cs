using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOC.E_Assistant.Library.Implementation.Extension;
public static class LibraryExtension
{
    public static IServiceCollection AddLibraryServices(this IServiceCollection services)
    {
        return services;
    }
}