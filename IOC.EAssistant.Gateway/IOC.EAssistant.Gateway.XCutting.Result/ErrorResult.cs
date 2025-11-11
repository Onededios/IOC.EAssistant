using System.Text.Json.Serialization;

namespace IOC.EAssistant.Gateway.XCutting.Results;
public class ErrorResult
{
    public string Title { get; set; }
    public string Message { get; set; }

    public ErrorResult(string message, string title = "unknown")
    {
        Title = title;
        Message = message;
    }
}
