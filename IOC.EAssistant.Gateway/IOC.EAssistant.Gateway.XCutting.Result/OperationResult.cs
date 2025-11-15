using IOC.EAssistant.Gateway.XCutting.Result;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace IOC.EAssistant.Gateway.XCutting.Results;
public class OperationResult
{
    public OperationResult()
    {
        Errors = new List<ErrorResult>();
        Exceptions = new List<ExceptionResult>();
    }


    [JsonIgnore]
    public int Status { get; set; }
    [JsonIgnore]
    public string Instance { get; set; } = string.Empty;
    public List<ErrorResult> Errors { get; }
#if !DEBUG
    [JsonIgnore]
#endif
    public List<ExceptionResult> Exceptions { get; }
    [JsonIgnore]
    public bool HasErrors => Errors.Any();
    [JsonIgnore]
    public bool HasExceptions => Exceptions.Any();
}

public class OperationResult<TResult> : OperationResult
{
    public OperationResult() : base() { }
    [JsonPropertyOrder(-1)]
    public TResult? Result { get; set; }

    public OperationResult AddResult(TResult result)
    {
        Result = result;
        return this;
    }

    public OperationResult AddResultWithError(
        TResult result,
        ErrorResult error,
        int errorCode,
        Exception? exception
    )
    {
        Result = result;
        Errors.Add(error);
        if (exception != null) Exceptions.Add(new ExceptionResult(exception));
        return this;
    }

    public OperationResult AddError(ErrorResult error, Exception? exception = null)
    {
        Errors.Add(error);
        if (exception != null) Exceptions.Add(new ExceptionResult(exception));
        return this;
    }

    public OperationResult AddErrors(IEnumerable<ErrorResult> validationErrors)
    {
        Errors.AddRange(validationErrors);
        return this;
    }

    public ActionResult<T> ToActionResult<T>(ControllerBase controller)
    {
        if (HasExceptions) return GetServerError<T>(controller);
        else if (HasErrors) return GetBadRequest<T>(controller);
        Status = 200;
        return controller.Ok(this);
    }

    public ActionResult<T> GetBadRequest<T>(ControllerBase controller)
    {
        Status = 400;
        return controller.BadRequest(this);
    }

    public ActionResult<T> GetServerError<T>(ControllerBase controller)
    {
        Status = 500;
        return controller.StatusCode(500, new ErrorResult("Internal server error", "An exception occurred. Unhandled error."));
    }
}