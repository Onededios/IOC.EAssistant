using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace IOC.EAssistant.Gateway.XCutting.Logging;

/// <summary>
/// Custom trace listener that redirects trace output to the Microsoft.Extensions.Logging infrastructure.
/// </summary>
/// <remarks>
/// This class extends <see cref="TraceListener"/> to capture trace messages and write them to an
/// <see cref="ILogger"/> instance. Messages are buffered until a line terminator is encountered,
/// at which point they are logged as information-level messages and the buffer is cleared.
/// This enables integration of System.Diagnostics tracing with modern .NET logging frameworks.
/// </remarks>
public class CustomTraceListener : TraceListener
{
    private readonly ILogger _logger;
    private readonly StringBuilder _stringBuilder = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomTraceListener"/> class.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create logger instances.</param>
    /// <remarks>
    /// The constructor creates a logger with the name <c>CustomTraceListener</c> that will be used
    /// to output all captured trace messages.
    /// </remarks>
    public CustomTraceListener(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(nameof(CustomTraceListener));
    }

    /// <summary>
    /// Writes a message with an associated category to the internal buffer.
    /// </summary>
    /// <param name="message">The message to write.</param>
    /// <param name="category">The category associated with the message.</param>
    /// <remarks>
    /// The message and category are concatenated with a dash separator and appended to the internal buffer.
    /// The message is not logged until <see cref="WriteLine(string?)"/> is called.
    /// </remarks>
    public override void Write(string? message, string? category) => _stringBuilder.Append(message + "-" + category);

    /// <summary>
    /// Writes a message to the internal buffer without a line terminator.
    /// </summary>
    /// <param name="message">The message to write.</param>
    /// <remarks>
    /// The message is appended to the internal buffer but not logged until <see cref="WriteLine(string?)"/> is called.
    /// </remarks>
    public override void Write(string? message) => _stringBuilder.Append(message);

    /// <summary>
    /// Writes a message to the internal buffer, logs the complete buffered content, and clears the buffer.
    /// </summary>
    /// <param name="message">The message to write before logging.</param>
    /// <remarks>
    /// This method appends the message with a line terminator to the buffer, logs the entire buffered content
    /// as an information-level message using the configured <see cref="ILogger"/>, and then clears the buffer
    /// for subsequent messages. This ensures that trace messages are written as complete lines to the logging infrastructure.
    /// </remarks>
    public override void WriteLine(string? message)
    {
        _stringBuilder.AppendLine(message);
        _logger.LogInformation(_stringBuilder.ToString());
        _stringBuilder.Clear();
    }
}