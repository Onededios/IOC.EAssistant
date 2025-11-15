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
    /// <summary>
    /// Processes a chat request and returns a response based on the provided input.
    /// </summary>
    /// <remarks>This method handles HTTP POST requests and delegates the processing of the chat request to
    /// the underlying service. The response is returned as an HTTP 200 status code if the operation is
    /// successful.</remarks>
    /// <param name="request">The chat request containing the input data required to generate a response.</param>
    /// <returns>An <see cref="ActionResult{T}"/> containing a <see cref="ChatResponseDto"/> with the result of the chat
    /// operation.</returns>
    [HttpPost]
    [ProducesResponseType(200)]
    public async Task<OperationResult<ChatResponseDto>> Chat([FromBody] ChatRequestDto request)
    {
        var res = await _serviceChat.ChatAsync(request);
        return res;
    }
}
