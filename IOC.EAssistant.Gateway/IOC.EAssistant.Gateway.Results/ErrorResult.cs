using System.Text.Json.Serialization;

namespace IOC.EAssistant.Gateway.Results;
public class ErrorResult
{
    [JsonIgnore]
    public Exception? Exception { get; set; }
    public string Message { get; set; }
    public ErrorResult(string message, Exception? exception)
    {
        Message = message;
        Exception = exception;
    }
}
