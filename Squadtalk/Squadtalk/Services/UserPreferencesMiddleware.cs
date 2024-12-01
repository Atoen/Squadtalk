using System.Diagnostics.CodeAnalysis;

namespace Squadtalk.Services;

public class UserPreferencesMiddleware(RequestDelegate next)
{
    public const string ContextLanguageItem = "ApplicationLanguage";
    private const string CookieLanguageName = ContextLanguageItem;

    public const string ContextThemeItem = "ApplicationTheme";
    private const string CookieThemeName = ContextThemeItem;

    public const string ContextUseDarkThemeItem = "UseDarkTheme";
    private const string CookieUseDarkThemeName = ContextUseDarkThemeItem;

    public const string ContextPreferencesItem = "ap";
    private const string PreferencesCookieName = ContextPreferencesItem;

    [SuppressMessage("ReSharper.DPA", "DPA0003: Excessive memory allocations in LOH")]
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
        var preferencesCookie = context.Request.Cookies[PreferencesCookieName];
        if (!string.IsNullOrWhiteSpace(preferencesCookie))
        {
            context.Items[ContextPreferencesItem] = preferencesCookie;
        }
        else
        {
            var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();
            if (string.IsNullOrWhiteSpace(acceptLanguage) ||
                acceptLanguage.Split(',', 2).FirstOrDefault() is not { } languageTag)
            {
                return;
            }

            context.Items[ContextPreferencesItem] = languageTag;
            context.Response.Cookies.Append(PreferencesCookieName, languageTag, new CookieOptions
            {
                SameSite = SameSiteMode.Strict,
                Expires = new DateTimeOffset(DateTime.Now + TimeSpan.FromDays(365))
            });
        }
    }
}

public static class UserPreferencesMiddlewareExtensions
{
    public static IApplicationBuilder UseUserPreferences(this IApplicationBuilder app)
    {
        return app.UseMiddleware<UserPreferencesMiddleware>();
    }
}
