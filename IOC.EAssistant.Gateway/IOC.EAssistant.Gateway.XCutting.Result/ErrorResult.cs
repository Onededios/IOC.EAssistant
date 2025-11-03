using System.Text.Json.Serialization;

namespace IOC.EAssistant.Gateway.XCutting.Results;
public class ErrorResult
{
#if !DEBUG
    [JsonIgnore]
#endif
    public string? ExceptionMessage { get; set; }
    public string ErrorMessage { get; set; }
    public ErrorResult(string message, Exception? exception)
    {
        ErrorMessage = message;
        ExceptionMessage = exception?.Message;
    }
}
