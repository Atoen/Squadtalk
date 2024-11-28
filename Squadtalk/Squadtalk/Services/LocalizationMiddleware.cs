namespace Squadtalk.Services;

public class LocalizationMiddleware(RequestDelegate next, ILogger<LocalizationMiddleware> logger)
{
    public const string ContextLanguageItem = "ApplicationLanguage";
    public const string CookieLanguageName = ContextLanguageItem;

    public const string ContextThemeItem = "ApplicationTheme";
    public const string CookieThemeName = ContextThemeItem;

    public Task InvokeAsync(HttpContext context)
    {
        var accept = context.Request.Headers.Accept;

        // filtering out requests not made by the user directly
        if (accept is not [ { } first, .. ] || !first.Contains("text/html"))
        {
            return next(context);
        }

        StoreUserPreferences(context, logger);

        return next(context);
    }

    private static void StoreUserPreferences(HttpContext context, ILogger<LocalizationMiddleware> logger)
    {
        var themeCookie = context.Request.Cookies[CookieThemeName];
        if (!string.IsNullOrWhiteSpace(themeCookie))
        {
            logger.LogInformation("Stored theme: {Theme}", themeCookie);
            context.Items[ContextThemeItem] = themeCookie;
        }

        var languageCookie = context.Request.Cookies[CookieLanguageName];
        if (!string.IsNullOrWhiteSpace(languageCookie))
        {
            logger.LogInformation("Stored languge: {Language}", languageCookie);
            context.Items[ContextLanguageItem] = languageCookie;
        }
        else
        {
            var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();
            if (!string.IsNullOrWhiteSpace(acceptLanguage) && acceptLanguage.Split(',').FirstOrDefault() is { } first)
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
