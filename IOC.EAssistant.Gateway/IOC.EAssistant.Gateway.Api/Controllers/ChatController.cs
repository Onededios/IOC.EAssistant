using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.AspNetCore.Mvc;

namespace IOC.EAssistant.Gateway.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ChatController(
    ILogger<ChatController> _logger,
    IServiceChat _serviceChat
) : ControllerBase
{

    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<OperationResult<ChatResponseDto>> Chat([FromBody] ChatRequestDto request)
    {
        var res = await _serviceChat.ChatAsync(request);
        return res;
    }
}
