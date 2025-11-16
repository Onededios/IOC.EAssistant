using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Validators;

/// <summary>
/// Provides validation logic for chat-related requests and responses.
/// </summary>
/// <remarks>
/// <para>
/// This validator ensures data integrity and correctness for chat operations by validating:
/// <list type="bullet">
/// <item><description>Client request structure and required fields</description></item>
/// <item><description>Message content and formatting</description></item>
/// <item><description>AI model response completeness</description></item>
/// </list>
/// </para>
/// <para>
/// All validation methods return collections of <see cref="ErrorResult"/> objects, allowing
/// multiple validation errors to be collected and returned together. Empty collections indicate
/// successful validation with no errors found.
/// </para>
/// <para>
/// The validator integrates with the logging infrastructure to record validation failures,
/// supporting debugging and monitoring of data quality issues.
/// </para>
/// </remarks>
/// <param name="_logger">The logger instance for recording validation warnings and errors.</param>
public class ValidatorChat(
    ILogger<ValidatorChat> _logger
)
{
    /// <summary>
    /// Validates a chat request to ensure it contains all required information and properly formatted data.
    /// </summary>
    /// <param name="request">The <see cref="ChatRequestDto"/> to validate.</param>
    /// <returns>
    /// A collection of <see cref="ErrorResult"/> objects describing any validation failures.
    /// An empty collection indicates the request is valid.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the following validations:
    /// <list type="number">
    /// <item>
    /// <description>
    /// <strong>Context Validation:</strong> Verifies that either a ConversationId or SessionId is provided.
    /// At least one identifier is required to establish or continue a conversation context.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <strong>Messages Collection Validation:</strong> Ensures the Messages collection is not null or empty.
    /// At least one message must be provided for the chat operation to process.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <strong>Message Content Validation:</strong> Checks that all messages contain non-empty question content.
    /// Empty or whitespace-only questions are considered invalid and cannot be processed by the AI model.
    /// </description>
  /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// Each validation failure is logged as a warning and added to the error collection with
    /// a descriptive message and "Invalid Request" title for consistent error reporting.
    /// </para>
    /// <para>
    /// This validation is performed before any external service calls or database operations,
    /// providing fast feedback and preventing unnecessary resource consumption for invalid requests.
    /// </para>
    /// </remarks>
    public IEnumerable<ErrorResult> ValidateRequest(ChatRequestDto request)
    {
        var errors = new List<ErrorResult>();

        if (!request.ConversationId.HasValue && !request.SessionId.HasValue)
    {
    errors.Add(new ErrorResult("Either ConversationId or SessionId must be provided", "Invalid Request"));
    _logger.LogWarning("Chat request missing both ConversationId and SessionId");
    }

        if (request.Messages == null || !request.Messages.Any())
        {
         errors.Add(new ErrorResult("Messages cannot be null or empty", "Invalid Request"));
          _logger.LogWarning("Chat request contains null or empty messages");
        }
        else if (request.Messages.Any(m => string.IsNullOrWhiteSpace(m.Question)))
        {
       errors.Add(new ErrorResult("All messages must have non-empty content", "Invalid Request"));
         _logger.LogWarning("Chat request contains messages with empty content");
        }

        return errors;
  }

    /// <summary>
  /// Validates the AI model's response to ensure it contains usable data.
    /// </summary>
    /// <param name="modelResponse">The <see cref="ChatResponse"/> received from the AI model to validate.</param>
/// <returns>
    /// A collection of <see cref="ErrorResult"/> objects describing any validation failures.
    /// An empty collection indicates the response is valid and contains usable data.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs the following validations:
    /// <list type="number">
    /// <item>
    /// <description>
    /// <strong>Null Response Check:</strong> Verifies the response object itself is not null.
    /// A null response indicates a critical failure in the AI model communication and prevents
    /// further processing. If null is detected, validation returns immediately with an error.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <strong>Choices Validation:</strong> Ensures the response contains at least one choice.
    /// The Choices collection contains the AI-generated responses, and an empty collection
    /// indicates the model failed to generate any content, making the response unusable.
 /// </description>
    /// </item>
 /// </list>
    /// </para>
    /// <para>
    /// Each validation failure is logged as a warning with descriptive information to aid
    /// in troubleshooting model communication issues. Errors are categorized with an
    /// "Invalid Response" title to distinguish them from request validation errors.
    /// </para>
    /// <para>
    /// This validation is performed immediately after receiving the model response and before
    /// attempting to extract content or persist data, preventing null reference exceptions
    /// and data corruption.
    /// </para>
    /// </remarks>
    public IEnumerable<ErrorResult> ValidateModelResponse(ChatResponse? modelResponse)
  {
        var errors = new List<ErrorResult>();
        if (modelResponse == null)
        {
            errors.Add(new ErrorResult("Model response is null", "Invalid Response"));
  _logger.LogWarning("Received null model response");
            return errors;
        }
        if (modelResponse.Choices == null || !modelResponse.Choices.Any())
        {
   errors.Add(new ErrorResult("Model response contains no choices", "Invalid Response"));
       _logger.LogWarning("Model response contains no choices");
        }
        return errors;
  }
}
