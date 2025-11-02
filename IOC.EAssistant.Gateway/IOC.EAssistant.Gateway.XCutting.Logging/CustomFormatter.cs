using Serilog.Events;
using Serilog.Formatting;

namespace IOC.EAssistant.Gateway.XCutting.Logging;
public class CustomFormatter : ITextFormatter
{
    private readonly string standard = "\x1b[34m"; // Color blue
    private readonly string error = "\x1b[31m"; // Color red
    private readonly string warning = "\x1b[33m"; // Color yellow
    private readonly string information = "\x1b[36m"; // Color cyan
    private readonly string critical = "\x1b[41m"; // Background Red

    public void Format(LogEvent logEvent, TextWriter output)
    {
        switch (logEvent.Level)
        {
            case LogEventLevel.Debug:
                PrintMessage(output, logEvent, standard);
                break;
            case LogEventLevel.Information:
                PrintMessage(output, logEvent, information);
                break;
            case LogEventLevel.Warning:
                PrintMessage(output, logEvent, warning);
                break;
            case LogEventLevel.Error:
                PrintMessage(output, logEvent, error);
                break;
            case LogEventLevel.Fatal:
                PrintMessage(output, logEvent, critical);
                break;
        }
    }

    private void PrintMessage(TextWriter output, LogEvent logEvent, string color)
    {
        var message = logEvent.MessageTemplate.Render(logEvent.Properties);
        var context = string.Empty;

        if (logEvent.Properties.TryGetValue("SourceContext", out var ctx)) context = ctx.ToString().Replace("\"", "");

        output.WriteLine($"{standard}[{logEvent.Timestamp}] {context}");
        output.WriteLine($"{color}{message}");
        output.WriteLine($"══════════════════════════════════════════");
    }
}
