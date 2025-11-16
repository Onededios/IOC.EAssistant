using IOC.EAssistant.Gateway.XCutting.Result;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace IOC.EAssistant.Gateway.XCutting.Results;

/// <summary>
/// Represents the result of an operation, including status information, errors, and exceptions.
/// </summary>
/// <remarks>
/// This class provides a standardized way to return operation results with error handling capabilities.
/// It maintains collections of <see cref="ErrorResult"/> and <see cref="ExceptionResult"/> instances
/// to track any issues that occurred during the operation. Exception details are only included in 
/// serialized output when running in DEBUG mode.
/// </remarks>
public class OperationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResult"/> class.
    /// </summary>
    /// <remarks>
    /// Initializes empty collections for errors and exceptions.
    /// </remarks>
    public OperationResult()
    {
        Errors = new List<ErrorResult>();
        Exceptions = new List<ExceptionResult>();
    }

    /// <summary>
    /// Gets or sets the HTTP status code for the operation result.
    /// </summary>
    /// <value>
    /// An integer representing the HTTP status code (e.g., 200, 400, 500).
    /// This property is ignored during JSON serialization.
    /// </value>
    [JsonIgnore]
    public int Status { get; set; }

    /// <summary>
    /// Gets or sets the instance identifier for the operation.
    /// </summary>
    /// <value>
    /// A string identifying the specific instance or context where the operation was executed.
    /// This property is ignored during JSON serialization.
    /// </value>
    [JsonIgnore]
    public string Instance { get; set; } = string.Empty;

    /// <summary>
    /// Gets the collection of errors that occurred during the operation.
    /// </summary>
    /// <value>
    /// A list of <see cref="ErrorResult"/> instances representing validation or business logic errors.
    /// </value>
    public List<ErrorResult> Errors { get; }

    /// <summary>
    /// Gets the collection of exceptions that occurred during the operation.
    /// </summary>
    /// <value>
    /// A list of <see cref="ExceptionResult"/> instances representing unhandled exceptions.
    /// This property is only included in JSON serialization when running in DEBUG mode.
    /// </value>
#if !DEBUG
    [JsonIgnore]
#endif
    public List<ExceptionResult> Exceptions { get; }

    /// <summary>
    /// Gets a value indicating whether the operation has any errors.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the <see cref="Errors"/> collection contains one or more items; otherwise, <see langword="false"/>.
    /// This property is ignored during JSON serialization.
    /// </value>
    [JsonIgnore]
    public bool HasErrors => Errors.Any();

    /// <summary>
    /// Gets a value indicating whether the operation has any exceptions.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the <see cref="Exceptions"/> collection contains one or more items; otherwise, <see langword="false"/>.
    /// This property is ignored during JSON serialization.
    /// </value>
    [JsonIgnore]
    public bool HasExceptions => Exceptions.Any();
}

/// <summary>
/// Represents the result of an operation that returns a value of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result value returned by the operation.</typeparam>
/// <remarks>
/// This class extends <see cref="OperationResult"/> to include a strongly-typed result value.
/// It provides fluent methods for adding results, errors, and converting to ASP.NET Core <see cref="ActionResult{T}"/> instances.
/// </remarks>
public class OperationResult<TResult> : OperationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResult{TResult}"/> class.
    /// </summary>
    public OperationResult() : base() { }

    /// <summary>
    /// Gets or sets the result value of the operation.
    /// </summary>
    /// <value>
    /// The result value of type <typeparamref name="TResult"/>, or <see langword="null"/> if no result is available.
    /// This property is serialized first in JSON output due to the <see cref="JsonPropertyOrderAttribute"/> with order -1.
    /// </value>
    [JsonPropertyOrder(-1)]
    public TResult? Result { get; set; }

    /// <summary>
    /// Adds a result value to the operation result.
    /// </summary>
    /// <param name="result">The result value to add.</param>
    /// <returns>The current <see cref="OperationResult"/> instance for method chaining.</returns>
    public OperationResult AddResult(TResult result)
    {
        Result = result;
        return this;
    }

    /// <summary>
    /// Adds a result value along with an error and optional exception to the operation result.
    /// </summary>
    /// <param name="result">The result value to add.</param>
    /// <param name="error">The <see cref="ErrorResult"/> describing the error condition.</param>
    /// <param name="errorCode">The error code (currently not used in the implementation).</param>
    /// <param name="exception">An optional <see cref="Exception"/> that caused the error.</param>
    /// <returns>The current <see cref="OperationResult"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method is useful when an operation produces a partial result but also encounters an error.
    /// If an exception is provided, it will be added to the <see cref="OperationResult.Exceptions"/> collection.
    /// </remarks>
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

    /// <summary>
    /// Adds an error and optional exception to the operation result.
    /// </summary>
    /// <param name="error">The <see cref="ErrorResult"/> describing the error condition.</param>
    /// <param name="exception">An optional <see cref="Exception"/> that caused the error.</param>
    /// <returns>The current <see cref="OperationResult"/> instance for method chaining.</returns>
    /// <remarks>
    /// If an exception is provided, it will be added to the <see cref="OperationResult.Exceptions"/> collection.
    /// </remarks>
    public OperationResult AddError(ErrorResult error, Exception? exception = null)
    {
        Errors.Add(error);
        if (exception != null) Exceptions.Add(new ExceptionResult(exception));
        return this;
    }

    /// <summary>
    /// Adds multiple errors to the operation result.
    /// </summary>
    /// <param name="validationErrors">A collection of <see cref="ErrorResult"/> instances to add.</param>
    /// <returns>The current <see cref="OperationResult"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method is particularly useful for adding validation errors from validators or business rules.
    /// </remarks>
    public OperationResult AddErrors(IEnumerable<ErrorResult> validationErrors)
    {
        Errors.AddRange(validationErrors);
        return this;
    }

    /// <summary>
    /// Converts the operation result to an ASP.NET Core <see cref="ActionResult{T}"/> with appropriate HTTP status code.
    /// </summary>
    /// <typeparam name="T">The type parameter for the action result.</typeparam>
    /// <param name="controller">The <see cref="ControllerBase"/> instance used to create the action result.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> with:
    /// <list type="bullet">
    /// <item><description>HTTP 500 (Internal Server Error) if <see cref="OperationResult.HasExceptions"/> is true</description></item>
    /// <item><description>HTTP 400 (Bad Request) if <see cref="OperationResult.HasErrors"/> is true</description></item>
    /// <item><description>HTTP 200 (OK) with the operation result if no errors or exceptions exist</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// This method provides automatic HTTP status code determination based on the operation's error state,
    /// simplifying controller action implementations.
    /// </remarks>
    public ActionResult<T> ToActionResult<T>(ControllerBase controller)
    {
        if (HasExceptions) return GetServerError<T>(controller);
        else if (HasErrors) return GetBadRequest<T>(controller);
        Status = 200;
        return controller.Ok(this);
    }

    /// <summary>
    /// Creates a Bad Request (HTTP 400) action result.
    /// </summary>
    /// <typeparam name="T">The type parameter for the action result.</typeparam>
    /// <param name="controller">The <see cref="ControllerBase"/> instance used to create the action result.</param>
    /// <returns>An <see cref="ActionResult{T}"/> with HTTP 400 status code containing this operation result.</returns>
    public ActionResult<T> GetBadRequest<T>(ControllerBase controller)
    {
        Status = 400;
        return controller.BadRequest(this);
    }

    /// <summary>
    /// Creates an Internal Server Error (HTTP 500) action result.
    /// </summary>
    /// <typeparam name="T">The type parameter for the action result.</typeparam>
    /// <param name="controller">The <see cref="ControllerBase"/> instance used to create the action result.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> with HTTP 500 status code containing a generic error message.
    /// The detailed exception information is not included in the response for security reasons.
    /// </returns>
    /// <remarks>
    /// This method returns a generic "Internal server error" message instead of exposing the actual 
    /// operation result to prevent leaking sensitive exception details to clients.
    /// </remarks>
    public ActionResult<T> GetServerError<T>(ControllerBase controller)
    {
        Status = 500;
        return controller.StatusCode(500, new ErrorResult("Internal server error", "An exception occurred. Unhandled error."));
    }
}