using IOC.EAssistant.Gateway.Library.Entities.Proxies.EAssistant;
using IOC.EAssistant.Gateway.XCutting.Results;
using Microsoft.AspNetCore.Mvc;

namespace IOC.EAssistant.Gateway.Api.Controllers;

[Route("[controller]")]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(List<ErrorResult>))]
public class ChatController(ILogger<ChatController> _logger)
{
    [HttpPost]
    public Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
    {
        var response = new ChatResponse
        {
        };
        return Task.FromResult<ActionResult<ChatResponse>>(response);
    }
}
