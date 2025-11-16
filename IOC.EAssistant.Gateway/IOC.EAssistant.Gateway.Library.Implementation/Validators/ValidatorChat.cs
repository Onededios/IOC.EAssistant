using IOC.EAssistant.Gateway.Infrastructure.Contracts.Proxies.EAssistant;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.Extensions.Logging;

namespace IOC.EAssistant.Gateway.Library.Implementation.Validators;
public class ValidatorChat(
    ILogger<ValidatorChat> _logger
)
{
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
