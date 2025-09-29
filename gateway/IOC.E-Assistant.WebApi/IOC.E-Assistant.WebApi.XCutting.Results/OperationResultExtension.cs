using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace IOC.E_Assistant.WebApi.XCutting.Results;
public static class OperationResultExtension
{
    public static OperationResult<TResult> AddResult<TResult>(this OperationResult<TResult> operationResult, TResult result)
    {
        if (operationResult == null) throw new ArgumentNullException(nameof(operationResult), "builderContext cannot be null");
        operationResult.Result = result;
        return operationResult;
    }

    public static OperationResult<TResult> AddResultWithError<TResult>(
        this OperationResult<TResult> operationResult,
        TResult result,
        string errorMessage,
        int errorCode,
        Exception exception
        )
    {
        if (operationResult == null) throw new ArgumentNullException(nameof(operationResult), "builderContext cannot be null");
        operationResult.Result = result;
        operationResult.ErrorList.Add(new ErrorResult(errorMessage, exception));
        return operationResult;
    }

    public static OperationResult<TResult> AddError<TResult>(
        this OperationResult<TResult> operationResult,
        string errorMessage
        )
    {
        if (operationResult == null) throw new ArgumentNullException(nameof(operationResult), "builderContext cannot be null");
        operationResult.ErrorList.Add(new ErrorResult(errorMessage));
        return operationResult;
    }

    public static OperationResult<TResult> AddError<TResult>(
        this OperationResult<TResult> operationResult,
        string errorMessage,
        Exception exception
        )
    {
        if (operationResult == null) throw new ArgumentNullException(nameof(operationResult), "builderContext cannot be null");
        operationResult.ErrorList.Add(new ErrorResult(errorMessage, exception));
        return operationResult;
    }

    public static OperationResult<TResult> AddErrors<TResult>(
        this OperationResult<TResult> operationResult,
        IEnumerable<ErrorResult> validationErrors
        )
    {
        if (operationResult == null) throw new ArgumentNullException(nameof(operationResult), "builderContext cannot be null");
        operationResult.ErrorList.AddRange(validationErrors);
        return operationResult;
    }

    public static ActionResult ToActionResult<TResult>(
        this OperationResult<TResult> operationResult,
        ControllerBase controller)
    {
        operationResult.Instance = controller.HttpContext.Request.Path;

        if (operationResult == null) return controller.NoContent();
        else if (operationResult.HasExceptions) return GetServerError(operationResult, controller);
        else if (operationResult.HasErrors) return GetBadRequest(operationResult, controller);
        operationResult.Status = 200;
        return controller.Ok(operationResult);
    }

    private static ActionResult GetBadRequest<TResult>(this OperationResult<TResult> operationResult, ControllerBase controller)
    {
        operationResult.Status = 400;
        return controller.BadRequest(operationResult);
    }

    private static ActionResult GetServerError<TResult>(this OperationResult<TResult> operationResult, ControllerBase controller)
    {
        operationResult.Status = 500;
        return controller.StatusCode(500, operationResult);
    }
}