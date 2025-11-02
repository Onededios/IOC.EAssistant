using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace IOC.EAssistant.Gateway.Results;
public class OperationResult
{
    public int Status { get; set; }
    public string Instance { get; set; } = string.Empty;
    public OperationResult() => ErrorList = new List<ErrorResult>();
    [JsonIgnore]
    public List<ErrorResult> ErrorList { get; }
    public List<string> Errors => [.. ErrorList.Select(e => e.Message)];
    [JsonIgnore]
    public bool HasErrors => ErrorList.Any();
    [JsonIgnore]
    public bool HasExceptions => ErrorList.Exists(x => x.Exception != null);
}

public class OperationResult<TResult> : OperationResult
{
    public OperationResult() : base() { }
    public TResult? Result { get; set; }

    public OperationResult AddResult(TResult result)
    {
        Result = result;
        return this;
    }

    public OperationResult AddResultWithError(
        TResult result,
        string errorMessage,
        int errorCode,
        Exception exception
    )
    {
        Result = result;
        ErrorList.Add(new ErrorResult(errorMessage, exception));
        return this;
    }

    public OperationResult AddError(string errorMessage, Exception exception)
    {
        ErrorList.Add(new ErrorResult(errorMessage, exception));
        return this;
    }

    public OperationResult AddErrors(IEnumerable<ErrorResult> validationErrors)
    {
        ErrorList.AddRange(validationErrors);
        return this;
    }

    public ActionResult ToActionResult(ControllerBase controller)
    {
        Instance = controller.HttpContext.Request.Path;

        if (HasExceptions) return GetServerError(controller);
        else if (HasErrors) return GetBadRequest(controller);
        Status = 200;
        return controller.Ok(this);
    }

    public ActionResult GetBadRequest(ControllerBase controller)
    {
        Status = 400;
        return controller.BadRequest(this);
    }

    public ActionResult GetServerError(ControllerBase controller)
    {
        Status = 500;
        return controller.StatusCode(500, this);
    }
}