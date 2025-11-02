using IOC.EAssistant.Gateway.XCutting.Logging;
using Serilog;
using System.Diagnostics;

namespace IOC.EAssistant.Gateway.Api.Extension;
public static class LoggingExtension
{
    public static IHostBuilder ConfigureLogging(this IHostBuilder builder)
    {
        builder.UseSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(hostingContext.Configuration)
                .WriteTo.Console(new CustomFormatter());
        });
        return builder;
    }

    public static IHost AndLoggingExtension(this IHost host)
    {
        var factory = host.Services.GetRequiredService<ILoggerFactory>();
        Trace.Listeners.Add(new CustomTraceListener(factory));
        return host;
    }
}