using IOC.EAssistant.Gateway.Library.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

namespace IOC.EAssistant.Gateway.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class ControllerSession(IServiceConversation _service) : ControllerBase
{
}
