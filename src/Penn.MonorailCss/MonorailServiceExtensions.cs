using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Pennington.Infrastructure;

namespace Pennington.MonorailCss;

/// <summary>
/// Extension methods for registering and configuring MonorailCSS services.
/// </summary>
public static partial class MonorailServiceExtensions
{
    /// <summary>
    /// Registers MonorailCSS services including the CSS class collector and stylesheet generator.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="optionFactory">Optional factory for configuring MonorailCSS options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMonorailCss(this IServiceCollection services,
        Func<IServiceProvider, MonorailCssOptions>? optionFactory = null)
    {
        if (optionFactory == null)
        {
            services.AddSingleton(new MonorailCssOptions());
        }
        else
        {
            services.AddTransient(optionFactory);
        }

        services.AddSingleton<CssClassCollector>();
        services.AddTransient<MonorailCssService>();
        services.AddSingleton<IResponseProcessor, CssClassCollectorProcessor>();

        return services;
    }

    /// <summary>
    /// Maps the MonorailCSS stylesheet endpoint and scans configured content files for CSS classes.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="path">The URL path for the stylesheet endpoint. Defaults to "/styles.css".</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseMonorailCss(this WebApplication app, string path = "/styles.css")
    {
        // Ensure the MonorailCssService is available
        if (app.Services.GetService<MonorailCssService>() is null)
        {
            throw new InvalidOperationException(
                "MonorailCssService is not registered. Please call AddMonorailCss() in ConfigureServices.");
        }

        // Ensure the CssClassCollector is available
        if (app.Services.GetService<CssClassCollector>() is null)
        {
            throw new InvalidOperationException(
                "CssClassCollector is not registered. Please call AddMonorailCss() in ConfigureServices.");
        }

        // Scan configured content files for CSS classes at startup.
        // This catches classes that only exist in client-side JS or other non-HTML files
        // (the Tailwind "content" problem).
        var options = app.Services.GetRequiredService<MonorailCssOptions>();
        if (options.ContentPaths.Length > 0)
        {
            var collector = app.Services.GetRequiredService<CssClassCollector>();
            var fileProvider = app.Environment.WebRootFileProvider;
            ScanContentFiles(collector, fileProvider, options.ContentPaths);
        }

        // Custom CSS. The Blazor Static service will discover the mapped URL automatically
        // and include it with the static generation.
        // Note: CSS class collection happens via CssClassCollectorProcessor registered as
        // IResponseProcessor in AddMonorailCss, run by the unified ResponseProcessingMiddleware.
        app.MapGet(path, (MonorailCssService cssService) => Results.Content(cssService.GetStyleSheet(), "text/css"));

        return app;
    }

    /// <summary>
    /// Scans files for potential CSS class names and registers them with the collector.
    /// Uses a broad extraction approach — false positives are harmless since MonorailCSS
    /// ignores tokens it doesn't recognize as utility classes.
    /// </summary>
    internal static void ScanContentFiles(CssClassCollector collector, IFileProvider fileProvider, string[] contentPaths)
    {
        foreach (var contentPath in contentPaths)
        {
            var fileInfo = fileProvider.GetFileInfo(contentPath);
            if (!fileInfo.Exists)
                continue;

            using var stream = fileInfo.CreateReadStream();
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            var classes = ExtractPotentialClasses(content);
            if (classes.Count == 0)
                continue;

            collector.BeginProcessing();
            try
            {
                collector.AddClasses(contentPath, classes);
            }
            finally
            {
                collector.EndProcessing();
            }
        }
    }

    /// <summary>
    /// Extracts potential CSS class names from file content using two strategies:
    /// 1. HTML class attribute extraction (class="..." patterns)
    /// 2. Broad token extraction — splits on delimiters and keeps tokens that look
    ///    like utility classes (contain hyphens, colons, slashes, or dots).
    /// </summary>
    internal static List<string> ExtractPotentialClasses(string content)
    {
        var classes = new HashSet<string>(StringComparer.Ordinal);

        // Strategy 1: Extract from class="..." attributes (works for HTML, Razor, and JS template literals)
        foreach (Match match in CssClassAttributeRegex().Matches(content))
        {
            foreach (var cls in match.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                classes.Add(cls);
            }
        }

        // Strategy 2: Split on delimiter characters and treat every token as a potential class.
        // This catches classes in JS string constants like: const PROSE = 'prose dark:prose-invert ...'
        // False positives (e.g. JS keywords like "function") are harmless — MonorailCSS
        // ignores tokens it doesn't recognize as utility classes.
        foreach (var token in TokenSplitRegex().Split(content))
        {
            if (token.Length > 0)
            {
                classes.Add(token);
            }
        }

        return classes.ToList();
    }

    [GeneratedRegex("""class\s*=\s*["']([^"']+)["']""", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex CssClassAttributeRegex();

    [GeneratedRegex("""[\s"'`<>{}()=;,!?#@$^&*|~\[\]\\]+""")]
    private static partial Regex TokenSplitRegex();
}
