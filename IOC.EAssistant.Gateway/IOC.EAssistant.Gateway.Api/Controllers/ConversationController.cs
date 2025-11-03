using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Api.Controllers;
public class ConversationController(
        ILogger<ConversationController> _logger,
        IServiceConversation _serviceConversation
    ) : BaseController<Conversation>(_logger, _serviceConversation)
{
}
