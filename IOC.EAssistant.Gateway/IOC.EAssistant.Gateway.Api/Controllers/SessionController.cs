using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Api.Controllers;
public class SessionController(
        ILogger<SessionController> _logger,
        IServiceSession _service
    ) : BaseController<Session>(_logger, _service)
{
}
