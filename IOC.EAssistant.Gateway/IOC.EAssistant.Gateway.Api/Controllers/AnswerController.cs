using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Databases.EAssistant;
namespace IOC.EAssistant.Gateway.Api.Controllers; public class AnswerController(
        IServiceAnswer _service,
        ILogger<AnswerController> _logger
    ) : BaseController<Answer>(_logger, _service)
{
}
