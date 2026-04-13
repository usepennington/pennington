namespace Pennington.Localization;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure;

/// <summary>
/// Middleware that detects the locale from the URL path prefix, populates
/// <see cref="LocaleContext"/>, and rewrites the request path to strip the
/// locale segment. This allows Razor components to use a single <c>@page</c>
/// directive without duplicating routes per locale.
/// </summary>
public sealed class LocaleDetectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly LocalizationOptions _localization;

    public LocaleDetectionMiddleware(RequestDelegate next, LocalizationOptions localization)
    {
        _next = next;
        _localization = localization;
    }

    public Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";
        var locale = _localization.GetLocaleFromUrl(path);
        var isDefault = string.Equals(locale, _localization.DefaultLocale, StringComparison.OrdinalIgnoreCase);
        var contentPath = _localization.StripLocalePrefix(path, locale);

        var info = _localization.Locales.TryGetValue(locale, out var li)
            ? li
            : new LocaleInfo(locale);

        // Populate the scoped LocaleContext
        var localeContext = context.RequestServices.GetRequiredService<LocaleContext>();
        localeContext.Locale = locale;
        localeContext.Info = info;
        localeContext.ContentPath = contentPath;
        localeContext.IsDefaultLocale = isDefault;

        // Store for any non-DI access patterns
        context.Items["Pennington.Locale"] = locale;
        context.Items["Pennington.LocaleContext"] = localeContext;

        // Rewrite the request path to strip the locale prefix so Blazor routing
        // matches routes without locale-specific @page directives.
        // Preserve the original path in PathBase so URL generation still works.
        if (!isDefault)
        {
            var stripped = _localization.StripLocalePrefix(path, locale);
            var prefix = path[..^stripped.Length]; // e.g., "/gen-z"
            if (prefix.Length > 0 && !prefix.Equals("/", StringComparison.Ordinal))
            {
                context.Request.PathBase = context.Request.PathBase.Add(prefix);
                context.Request.Path = new PathString(stripped);
            }
        }

        return _next(context);
    }
}