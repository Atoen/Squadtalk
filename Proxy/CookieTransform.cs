using Yarp.ReverseProxy.Transforms;

namespace Proxy;

public class CookieTransform : RequestTransform
{
    private const string PreferencesCookieName = "ap";
    private const string PrerenerUserDataCookeName = "ud";

    private static readonly PathString AngularInternal = "/ng/@ng";

    public override ValueTask ApplyAsync(RequestTransformContext context)
    {
        var request = context.HttpContext.Request;
        if (request.Path.StartsWithSegments(AngularInternal) ||
            !request.Headers.Accept.ToString().StartsWith("text/html"))
        {
            return ValueTask.CompletedTask;
        }

        var preferencesCookie = request.Cookies[PreferencesCookieName];
        var userData = request.Cookies[PrerenerUserDataCookeName];

        var preferencesQuery = string.IsNullOrWhiteSpace(preferencesCookie) ? QueryString.Empty : new QueryString($"?p={preferencesCookie}");
        var userQuery = string.IsNullOrWhiteSpace(userData) ? QueryString.Empty : new QueryString($"?u={userData}");
        var dataQuery = preferencesQuery.Add(userQuery);

        var fullQuery = context.Query.QueryString.Add(dataQuery);
        var uri = new Uri($"{context.DestinationPrefix}{context.Path}{fullQuery}");
        context.ProxyRequest.RequestUri = uri;

        Console.WriteLine($"\n\n\n{request.Path}");
        Console.WriteLine(request.Headers.Accept);
        Console.WriteLine(context.ProxyRequest.RequestUri);

        return ValueTask.CompletedTask;
    }
}
