namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Extension;

/// <summary>
/// Provides extension methods for <see cref="Uri"/> to simplify URI manipulation operations.
/// </summary>
public static class UriExtension
{
    /// <summary>
    /// Appends one or more path segments to the URI, ensuring proper path separator formatting.
    /// </summary>
    /// <param name="uri">The base <see cref="Uri"/> to which the paths will be appended.</param>
    /// <param name="paths">One or more path segments to append to the base URI.</param>
    /// <returns>A new <see cref="Uri"/> instance with the specified paths appended.</returns>
    /// <remarks>
    /// This method automatically handles path separators by trimming trailing slashes from the current URI 
    /// and leading slashes from each path segment, then joining them with a single forward slash.
    /// Multiple path segments can be provided and will be appended in the order specified.
    /// </remarks>
    /// <example>
    /// <code>
    /// var baseUri = new Uri("https://example.com/api");
    /// var fullUri = baseUri.Append("users", "123");
    /// // Result: https://example.com/api/users/123
    /// </code>
    /// </example>
    public static Uri Append(this Uri uri, params string[] paths) => new Uri(paths.Aggregate(uri.AbsoluteUri, (current, path) => string.Format("{0}/{1}", current.TrimEnd('/'), path.TrimStart('/'))));
}