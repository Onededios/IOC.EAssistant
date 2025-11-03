using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace IOC.EAssistant.Gateway.XCutting.Results;
public class OperationResult
{
    public int Status { get; set; }
    public string Instance { get; set; } = string.Empty;
    public OperationResult() => ErrorList = new List<ErrorResult>();
    public List<ErrorResult> ErrorList { get; }
    public bool HasErrors => ErrorList.Any();
    public bool HasExceptions => ErrorList.Exists(x => x.ExceptionMessage != null);
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

    public ActionResult<T> ToActionResult<T>(ControllerBase controller)
    {
        Instance = controller.HttpContext.Request.Path;

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
        return controller.StatusCode(500, this);
    }
}