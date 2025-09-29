using System.Text.Json.Serialization;

namespace IOC.E_Assistant.WebApi.XCutting.Results;
public class OperationResult
{
    public int Status { get; set; }
    public string Instance { get; set; }
    public OperationResult() { ErrorList = new List<ErrorResult>(); }
    [JsonIgnore]
    public List<ErrorResult> ErrorList { get; }
    public List<string> Errors => ErrorList.Select(e => e.Message).ToList();
    [JsonIgnore]
    public bool HasErrors => ErrorList.Any();
    [JsonIgnore]
    public bool HasExceptions => ErrorList.Exists(x => x.Exception != null);
}

public class OperationResult<TResult> : OperationResult
{
    public OperationResult() : base() { }
    public TResult Result { get; set; }
}