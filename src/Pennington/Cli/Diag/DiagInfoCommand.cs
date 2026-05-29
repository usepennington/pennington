namespace Pennington.Cli.Diag;

using System.CommandLine;
using Content;
using Generation;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Navigation;

/// <summary><c>diag info</c> — a high-level overview of the site: the shape of the app at a glance.</summary>
internal sealed class DiagInfoCommand : IDiagCommand
{
    /// <inheritdoc/>
    public string Name => "info";

    /// <inheritdoc/>
    public string Description => "Show a high-level overview: title, content roots, pages, sections, locales, and features.";

    /// <inheritdoc/>
    public Command Build(IServiceProvider services, TextWriter output)
    {
        var command = new Command(Name, Description);
        command.SetAction(async (_, _) =>
        {
            var options = services.GetRequiredService<PenningtonOptions>();
            var outputOptions = services.GetRequiredService<OutputOptions>();
            var toc = await services.GetServices<IContentService>().CollectTocEntriesAsync();
            var tree = await services.GetRequiredService<NavigationBuilder>().BuildTreeAsync(toc);
            var localization = options.Localization;

            var features = new List<string> { "search" };
            if (options.MapSitemap)
            {
                features.Add("sitemap");
            }

            if (options.LlmsTxt is not null)
            {
                features.Add("llms.txt");
            }

            var sourceCount = options.MarkdownSources.Count;

            output.WriteLine($"Pennington {PenningtonVersion.Value}");
            output.WriteLine();
            output.WriteLine($"Site:        {(string.IsNullOrWhiteSpace(options.SiteTitle) ? "(untitled)" : options.SiteTitle)}");
            if (!string.IsNullOrWhiteSpace(options.SiteDescription))
            {
                output.WriteLine($"Description: {options.SiteDescription}");
            }

            output.WriteLine($"Base URL:    {options.CanonicalBaseUrl ?? "(not set)"}");
            output.WriteLine($"Content:     {options.ContentRootPath}  ({sourceCount} markdown source{(sourceCount == 1 ? "" : "s")})");
            output.WriteLine($"Output:      {outputOptions.OutputDirectory}  (base {outputOptions.BaseUrl})");
            output.WriteLine($"Pages:       {toc.Count}");
            output.WriteLine($"Sections:    {(tree.Count == 0 ? "(none)" : string.Join(", ", tree.Select(t => t.Title)))}");
            output.WriteLine($"Locales:     {FormatLocales(localization)}");
            output.WriteLine($"Features:    {string.Join(", ", features)}");
            return 0;
        });
        return command;
    }

    private static string FormatLocales(LocalizationOptions localization)
    {
        if (!localization.IsMultiLocale)
        {
            return $"{localization.DefaultLocale} (single-locale)";
        }

        return string.Join(", ", localization.Locales.Keys.Select(code =>
            string.Equals(code, localization.DefaultLocale, StringComparison.OrdinalIgnoreCase)
                ? $"{code} (default)"
                : code));
    }
}
