namespace IOC.EAssistant.Gateway.XCutting.Results;

/// <summary>
/// Represents an error result containing descriptive information about an error condition.
/// </summary>
/// <remarks>
/// This class is used to encapsulate error information with a title and message,
/// providing a consistent structure for error reporting across the application.
/// </remarks>
public class ErrorResult
{
    /// <summary>
    /// Gets or sets the title of the error.
    /// </summary>
    /// <value>
    /// A string representing the error title or category. Defaults to "unknown" if not specified.
    /// </value>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the detailed error message.
    /// </summary>
    /// <value>
    /// A string containing a detailed description of the error.
    /// </value>
    public string Message { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorResult"/> class with the specified message and optional title.
    /// </summary>
    /// <param name="message">The detailed error message describing what went wrong.</param>
    /// <param name="title">The title or category of the error. Defaults to "unknown" if not provided.</param>
    public ErrorResult(string message, string title = "unknown")
    {
        Title = title;
        Message = message;
    }
}
