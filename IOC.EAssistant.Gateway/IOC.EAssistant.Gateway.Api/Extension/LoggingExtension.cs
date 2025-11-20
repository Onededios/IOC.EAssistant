using IOC.EAssistant.Gateway.XCutting.Logging;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using System.Diagnostics;

namespace IOC.EAssistant.Gateway.Api.Extension;
public static class LoggingExtension
{
    public static IHostBuilder ConfigureLogging(this IHostBuilder builder, IConfiguration configuration)
    {
        Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine($"[Serilog] {msg}"));
        Serilog.Debugging.SelfLog.Enable(Console.Error);

        builder.UseSerilog((hostingContext, loggerConfiguration) =>
            {
                var lokiUrl = configuration["LOKI_URL"];
                var lokiUsername = configuration["LOKI_USERNAME"];
                var lokiPassword = configuration["LOKI_PASSWORD"];
                var environment = hostingContext.HostingEnvironment.EnvironmentName;
                var machineName = Environment.MachineName;

                loggerConfiguration
                    .MinimumLevel.Information()
                    .Enrich.WithProperty("Application", "eassistant-gateway")
                    .Enrich.WithProperty("Environment", environment)
                    .Enrich.WithProperty("MachineName", machineName)
                    .Enrich.WithProperty("Version", "1.0.0")
                    .Enrich.FromLogContext()
                    .WriteTo.Console(new CustomFormatter());

                if (lokiUrl != null && lokiUsername != null && lokiPassword != null)
                {
                    loggerConfiguration.WriteTo.GrafanaLoki(
                        lokiUrl,
                        credentials: new LokiCredentials
                        {
                            Login = lokiUsername,
                            Password = lokiPassword
                        },
                        labels:
                        [
                            new LokiLabel { Key = "app", Value = "eassistant-gateway" },
                            new LokiLabel { Key = "environment", Value = environment.ToLower() },
                            new LokiLabel { Key = "machine", Value = machineName },
                            new LokiLabel { Key = "source", Value = "dotnet" },
                            new LokiLabel { Key = "framework", Value = "net9.0" }
                        ],
                        propertiesAsLabels: new[] { "level" },
                        batchPostingLimit: 100,
                        queueLimit: 10000,
                        period: TimeSpan.FromSeconds(2),
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
                    );
                }
            }
        );
        return builder;
    }

    public static IHost AndLoggingExtension(this IHost host)
    {
        var factory = host.Services.GetRequiredService<ILoggerFactory>();
        Trace.Listeners.Add(new CustomTraceListener(factory));
        return host;
    }
}