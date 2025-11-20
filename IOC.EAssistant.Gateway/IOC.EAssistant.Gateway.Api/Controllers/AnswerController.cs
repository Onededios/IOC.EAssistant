using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Api.Controllers;

/// <summary>
/// Controller for managing answer entities in the EAssistant Gateway API.
/// </summary>
/// <remarks>
/// This controller provides RESTful endpoints for answer management operations including
/// retrieval and deletion. Answers represent AI-generated responses to user questions and
/// include metadata, sources, and token count information. Inherits standard CRUD operations
/// from <see cref="BaseController{TEntity}"/>.
/// </remarks>
/// <param name="_service">Service layer implementation for answer business logic.</param>
/// <param name="_logger">Logger instance for tracking answer-related operations.</param>
public class AnswerController(
        IServiceAnswer _service,
        ILogger<AnswerController> _logger
    ) : BaseController<Answer>(_logger, _service)
{
}
