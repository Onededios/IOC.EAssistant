using IOC.EAssistant.Gateway.Library.Contracts.Services;
using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant.Chat;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.AspNetCore.Mvc;

namespace IOC.EAssistant.Gateway.Api.Controllers;

[Route("[controller]")]
[Produces("application/json")]
[ProducesResponseType(typeof(List<ErrorResult>), 400)]
public class ChatController(
    ILogger<ChatController> _logger,
    IServiceChat _service
) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<ActionResult<ChatResponseDto>> Chat([FromBody] ChatRequestDto request)
    {
        var res = await _service.ChatAsync(request);
        return res.ToActionResult<ChatResponseDto>(this);
    }
}
