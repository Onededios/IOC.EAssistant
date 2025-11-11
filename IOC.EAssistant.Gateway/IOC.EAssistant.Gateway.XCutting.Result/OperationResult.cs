using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace IOC.EAssistant.Gateway.XCutting.Results;
public class OperationResult
{
    public OperationResult()
    {
        Errors = new List<ErrorResult>();
        Exceptions = new List<Exception>();
    }


    [JsonIgnore]
    public int Status { get; set; }
    [JsonIgnore]
    public string Instance { get; set; } = string.Empty;
    public List<ErrorResult> Errors { get; }
#if !DEBUG
    [JsonIgnore]
#endif
    public List<Exception> Exceptions { get; }
    [JsonIgnore]
    public bool HasErrors => Errors.Any();
    [JsonIgnore]
    public bool HasExceptions => Exceptions.Any();
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
        ErrorResult error,
        int errorCode,
        Exception? exception
    )
    {
        Result = result;
        Errors.Add(error);
        if (exception != null) Exceptions.Add(exception);
        return this;
    }

    public OperationResult AddError(ErrorResult error, Exception? exception)
    {
        Errors.Add(error);
        if (exception != null) Exceptions.Add(exception);
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
        return controller.Ok(Result);
    }

    public ActionResult<T> GetBadRequest<T>(ControllerBase controller)
    {
        Status = 400;
        return controller.BadRequest(Errors);
    }

    public ActionResult<T> GetServerError<T>(ControllerBase controller)
    {
        Status = 500;
        return controller.StatusCode(500, new ErrorResult("Internal server error", "An exception occurred. Unhandled error."));
    }
}