using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Api.Controllers;

/// <summary>
/// Controller for managing conversation entities in the EAssistant Gateway API.
/// </summary>
/// <remarks>
/// This controller provides RESTful endpoints for conversation management operations including
/// retrieval and deletion. Conversations represent dialogue exchanges within a session and
/// contain multiple questions. Inherits standard CRUD operations from <see cref="BaseController{TEntity}"/>.
/// </remarks>
/// <param name="_logger">Logger instance for tracking conversation-related operations.</param>
/// <param name="_serviceConversation">Service layer implementation for conversation business logic.</param>
public class ConversationController(
        ILogger<ConversationController> _logger,
        IServiceConversation _serviceConversation
    ) : BaseController<Conversation>(_logger, _serviceConversation)
{
}
