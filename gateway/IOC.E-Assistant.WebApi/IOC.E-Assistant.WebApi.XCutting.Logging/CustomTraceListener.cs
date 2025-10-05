using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace CustomManager.Hairdresser.API.Logging;
public class CustomTraceListener : TraceListener
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly StringBuilder _stringBuilder = new();
    public CustomTraceListener(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger(nameof(CustomTraceListener));
    }

    public override void Write(string? message, string? category) => _stringBuilder.Append(message + "-" + category);
    public override void Write(string? message) => _stringBuilder.Append(message);
    public override void WriteLine(string? message)
    {
        _stringBuilder.AppendLine(message);
        _logger.LogInformation(_stringBuilder.ToString());
        _stringBuilder.Clear();
    }
}