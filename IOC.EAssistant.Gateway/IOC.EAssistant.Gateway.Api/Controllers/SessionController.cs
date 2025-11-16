using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Api.Controllers;

/// <summary>
/// Controller for managing session entities in the EAssistant Gateway API.
/// </summary>
/// <remarks>
/// This controller provides RESTful endpoints for session management operations including
/// retrieval and deletion. Sessions represent user interaction sessions that contain
/// multiple conversations. Inherits standard CRUD operations from <see cref="BaseController{TEntity}"/>.
/// </remarks>
/// <param name="_logger">Logger instance for tracking session-related operations.</param>
/// <param name="_service">Service layer implementation for session business logic.</param>
public class SessionController(
        ILogger<SessionController> _logger,
        IServiceSession _service
    ) : BaseController<Session>(_logger, _service)
{
}