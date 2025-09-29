using System.Text.Json.Serialization;

namespace IOC.E_Assistant.WebApi.XCutting.Results;
public class ErrorResult
{
    [JsonIgnore]
    public Exception Exception { get; set; }
    public string Message { get; set; }
    public ErrorResult(string message, Exception exception = null)
    {
        Message = message;
        Exception = exception;
    }
}