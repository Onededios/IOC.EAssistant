using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Api.Controllers;

/// <summary>
/// Controller for managing question entities in the EAssistant Gateway API.
/// </summary>
/// <remarks>
/// This controller provides RESTful endpoints for question management operations including
/// retrieval and deletion. Questions represent user queries within a conversation and are
/// associated with answers. Inherits standard CRUD operations from <see cref="BaseController{TEntity}"/>.
/// </remarks>
/// <param name="_service">Service layer implementation for question business logic.</param>
/// <param name="_logger">Logger instance for tracking question-related operations.</param>
public class QuestionController(
        IServiceQuestion _service,
        ILogger<QuestionController> _logger
    ) : BaseController<Question>(_logger, _service)
{
}