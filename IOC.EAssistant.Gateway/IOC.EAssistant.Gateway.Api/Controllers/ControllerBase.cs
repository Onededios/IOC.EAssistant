using Microsoft.AspNetCore.Mvc;

namespace IOC.EAssistant.Gateway.Api.Controllers;
public class ControllerBase<T>(ILogger _logger, IServiceBase<T> _service) : ControllerBase
{

}
