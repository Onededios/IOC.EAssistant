namespace IOC.EAssistant.Gateway.Infrastructure.Implementation.Extension;
public static class UriExtension
{
    public static Uri Append(this Uri uri, params string[] paths) => new Uri(paths.Aggregate(uri.AbsoluteUri, (current, path) => string.Format("{0}/{1}", current.TrimEnd('/'), path.TrimStart('/'))));
}