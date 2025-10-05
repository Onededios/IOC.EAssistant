using CustomManager.Hairdresser.API.Logging;
using Serilog;
using System.Diagnostics;

namespace IOC.E_Assistant.WebApi.Extensions;
public static class LoggingExtension
{
    public static IHostBuilder ConfigureLogging(this IHostBuilder builder)
    {
        builder.ConfigureLogging(delegate (HostBuilderContext hostingContext, ILoggingBuilder logging)
        {
            if (hostingContext.Configuration == null) throw new InvalidOperationException("Log needs an IConfiguration object to run.");

            Log.Logger = new LoggerConfiguration()
                .ReadFrom
                .Configuration(hostingContext.Configuration)
                .CreateLogger();
        });
        builder.UseSerilog();
        return builder;
    }

    public static IHost AndLoggingExtension(this IHost host)
    {
        var factory = host.Services.GetRequiredService<ILoggerFactory>();
        Trace.Listeners.Add(new CustomTraceListener(factory));
        return host;
    }
}
