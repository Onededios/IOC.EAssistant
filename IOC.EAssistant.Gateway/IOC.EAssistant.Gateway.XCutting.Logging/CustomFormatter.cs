using Serilog.Events;
using Serilog.Formatting;

namespace IOC.EAssistant.Gateway.XCutting.Logging;

/// <summary>
/// Custom text formatter for Serilog that applies ANSI color codes to log messages based on their severity level.
/// </summary>
/// <remarks>
/// This formatter implements <see cref="ITextFormatter"/> to provide colored console output for different log levels.
/// It uses ANSI escape sequences to colorize log messages, making them easier to read and distinguish in terminal output.
/// Each log level (Debug, Information, Warning, Error, Fatal) is associated with a specific color scheme.
/// </remarks>
public class CustomFormatter : ITextFormatter
{
    private readonly string standard = "\x1b[34m"; // Color blue
    private readonly string error = "\x1b[31m"; // Color red
    private readonly string warning = "\x1b[33m"; // Color yellow
    private readonly string information = "\x1b[36m"; // Color cyan
    private readonly string critical = "\x1b[41m"; // Background Red

    /// <summary>
    /// Formats a log event and writes it to the output with appropriate color coding based on the log level.
    /// </summary>
    /// <param name="logEvent">The <see cref="LogEvent"/> to format, containing the log level, timestamp, message, and properties.</param>
    /// <param name="output">The <see cref="TextWriter"/> to write the formatted log event to.</param>
    /// <remarks>
    /// This method routes the log event to the appropriate color scheme based on its severity level:
    /// <list type="bullet">
    /// <item><description><see cref="LogEventLevel.Debug"/> - Blue</description></item>
    /// <item><description><see cref="LogEventLevel.Information"/> - Cyan</description></item>
    /// <item><description><see cref="LogEventLevel.Warning"/> - Yellow</description></item>
    /// <item><description><see cref="LogEventLevel.Error"/> - Red</description></item>
    /// <item><description><see cref="LogEventLevel.Fatal"/> - Red background (critical)</description></item>
    /// </list>
    /// The output includes the timestamp, source context, and message, separated by a visual divider.
    /// </remarks>
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
