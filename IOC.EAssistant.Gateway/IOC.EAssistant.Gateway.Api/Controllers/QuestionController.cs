using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;

namespace IOC.EAssistant.Gateway.Api.Controllers;
public class QuestionController(
        IServiceQuestion _service,
        ILogger<QuestionController> _logger
    ) : BaseController<Question>(_logger, _service)
{
}
