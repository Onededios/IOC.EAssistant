namespace IOC.EAssistant.Gateway.XCutting.Results;

/// <summary>
/// Represents exception information extracted from an <see cref="Exception"/> for serialization and logging purposes.
/// </summary>
/// <remarks>
/// This class captures essential exception details (message and stack trace) in a serializable format,
/// making it suitable for API responses, logging, or error reporting scenarios where the original 
/// <see cref="Exception"/> object cannot or should not be directly exposed.
/// </remarks>
public class ExceptionResult
{
    /// <summary>
    /// Gets or sets the exception message.
    /// </summary>
    /// <value>
    /// A string containing the exception message that describes the error condition.
    /// </value>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the stack trace of the exception.
    /// </summary>
    /// <value>
    /// A string containing the stack trace that shows the call stack at the point where the exception occurred.
    /// Returns an empty string if the stack trace is not available.
    /// </value>
    public string StackTrace { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionResult"/> class from an exception.
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/> from which to extract the message and stack trace.</param>
    /// <remarks>
    /// This constructor extracts the <see cref="Exception.Message"/> and <see cref="Exception.StackTrace"/> 
    /// from the provided exception. If the stack trace is null, it defaults to an empty string.
    /// </remarks>
    public ExceptionResult(Exception exception)
    {
        Message = exception.Message;
        StackTrace = exception.StackTrace ?? string.Empty;
    }
}
