using Serilog.Events;
using Serilog.Formatting;

namespace IOC.E_Assistant.WebApi.Logging;
public class Formatter : ITextFormatter
{
    private readonly string STANDARD = "\x1b[34m"; // Color blue
    private readonly string ERROR = "\x1b[31m"; // Color red
    private readonly string WARNING = "\x1b[33m"; // Color yellow
    private readonly string INFORMATION = "\x1b[36m"; // Color cyan
    private readonly string CRITICAL = "\x1b[41m"; // Background Red

    public void Format(LogEvent logEvent, TextWriter output)
    {
        switch (logEvent.Level)
        {
            case LogEventLevel.Debug:
                PrintMessage(output, logEvent, STANDARD);
                break;
            case LogEventLevel.Information:
                PrintMessage(output, logEvent, INFORMATION);
                break;
            case LogEventLevel.Warning:
                PrintMessage(output, logEvent, WARNING);
                break;
            case LogEventLevel.Error:
                PrintMessage(output, logEvent, ERROR);
                break;
            case LogEventLevel.Fatal:
                PrintMessage(output, logEvent, CRITICAL);
                break;
        }
    }

    private void PrintMessage(TextWriter output, LogEvent logEvent, string color)
    {
        var message = logEvent.MessageTemplate.Render(logEvent.Properties);
        var context = string.Empty;

        if (logEvent.Properties.TryGetValue("SourceContext", out var ctx)) context = ctx.ToString().Replace("\"", "");

        output.WriteLine($"{STANDARD}[{logEvent.Timestamp}] {context}");
        output.WriteLine($"{color}{message}");
        output.WriteLine($"══════════════════════════════════════════");
    }
}
