using Shared.Data.Personalization;

namespace Squadtalk.Services;

public class LocalizationMiddleware(RequestDelegate next)
{
    public const string ContextLanguageItem = "ApplicationLanguage";
    private const string CookieLanguageName = ContextLanguageItem;

    public const string ContextThemeItem = "ApplicationTheme";
    private const string CookieThemeName = ContextThemeItem;

    public const string ContextUseDarkThemeItem = "UseDarkTheme";
    private const string CookieUseDarkThemeName = ContextUseDarkThemeItem;

    public Task InvokeAsync(HttpContext context)
    {
        var acceptHeader = context.Request.Headers.Accept;

        // filtering out requests not made directly by the user
        if (acceptHeader is not [ { } first, .. ] || !first.StartsWith("text/html", StringComparison.Ordinal))
        {
            return next(context);
        }

        StoreUserPreferences(context);

        return next(context);
    }

    private static void StoreUserPreferences(HttpContext context)
    {
        var themeCookie = context.Request.Cookies[CookieThemeName];
        if (!string.IsNullOrWhiteSpace(themeCookie))
        {
            context.Items[ContextThemeItem] = themeCookie;
        }

        if (themeCookie == ApplicationTheme.AutoValue)
        {
            var darkThemeCookie = context.Request.Cookies[CookieUseDarkThemeName];
            if (!string.IsNullOrWhiteSpace(darkThemeCookie))
            {
                context.Items[ContextUseDarkThemeItem] = darkThemeCookie;
            }
        }

        var languageCookie = context.Request.Cookies[CookieLanguageName];
        if (!string.IsNullOrWhiteSpace(languageCookie))
        {
            context.Items[ContextLanguageItem] = languageCookie;
        }
        else
        {
            var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();
            if (!string.IsNullOrWhiteSpace(acceptLanguage) && acceptLanguage.Split(',', 2).FirstOrDefault() is { } first)
            {
                context.Items[ContextLanguageItem] = first;
            }
        }
    }
}

public static class LocalizationMiddlewareExtensions
{
    public static IApplicationBuilder UseLocalization(this IApplicationBuilder app)
    {
        return app.UseMiddleware<LocalizationMiddleware>();
    }
}
