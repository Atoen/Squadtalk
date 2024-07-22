using System.Web;
using Microsoft.AspNetCore.Http.Extensions;

namespace Squadtalk.Services;

public class LocalizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.ToString().Contains("_framework"))
        {
            await next(context);
            return;
        }

        var language = GetUserLanguage(context);
        if (language is not null)
        {
            context.Items[ServerLocalizationService.ContextLanguageItem] = language;
        }

        await next(context);
    }

    private string? GetUserLanguage(HttpContext context)
    {
        var languageQuery = context.Request.Query[ServerLocalizationService.LanguageQueryParam].ToString();
        if (!string.IsNullOrWhiteSpace(languageQuery))
        {
            context.Response.Cookies.Append(ServerLocalizationService.LanguageCookieName, languageQuery);

            var uri = context.Request.GetEncodedUrl();
            var uriBuilder = new UriBuilder(uri);

            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query.Remove(ServerLocalizationService.LanguageQueryParam);
            uriBuilder.Query = query.ToString();

            context.Response.Redirect(uriBuilder.ToString());

            return languageQuery;
        }

        var languageCookie = context.Request.Cookies[ServerLocalizationService.LanguageCookieName];
        if (!string.IsNullOrWhiteSpace(languageCookie))
        {
            return languageCookie;
        }

        var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();
        return !string.IsNullOrWhiteSpace(acceptLanguage)
            ? acceptLanguage.Split(',').FirstOrDefault()
            : null;
    }
}

public static class LocalizationMiddlewareExtensions
{
    public static IApplicationBuilder UseLocalization(this IApplicationBuilder app)
    {
        return app.UseMiddleware<LocalizationMiddleware>();
    }
}
