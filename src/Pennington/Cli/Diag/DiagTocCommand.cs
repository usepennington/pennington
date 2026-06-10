namespace Pennington.Cli.Diag;

using System.CommandLine;
using Content;
using Infrastructure;
using Localization;
using Microsoft.Extensions.DependencyInjection;
using Navigation;

/// <summary><c>diag toc</c> — an ASCII tree of the table of contents, grouped by top-level section/area.</summary>
internal sealed class DiagTocCommand : IDiagCommand
{
    /// <inheritdoc/>
    public string Name => "toc";

    /// <inheritdoc/>
    public string Description => "Print the table of contents as a tree, grouped by top-level section/area.";

    /// <inheritdoc/>
    public Command Build(IServiceProvider services, TextWriter output)
    {
        var localeOption = new Option<string>("--locale")
        {
            Description = "Locale to render (default: the site default locale).",
        };
        var areaOption = new Option<string>("--area")
        {
            Description = "Limit to one top-level section/area by slug (its first URL segment).",
        };
        var depthOption = new Option<int?>("--depth")
        {
            Description = "Maximum tree depth to print (default: unlimited).",
        };

        var command = new Command(Name, Description);
        command.Options.Add(localeOption);
        command.Options.Add(areaOption);
        command.Options.Add(depthOption);
        command.SetAction(async (parseResult, _) =>
        {
            var areaSlug = parseResult.GetValue(areaOption);
            var maxDepth = parseResult.GetValue(depthOption) ?? int.MaxValue;

            var options = services.GetRequiredService<PenningtonOptions>();
            var localization = options.Localization;
            var toc = await services.GetServices<IContentService>().CollectTocEntriesAsync();
            var effectiveLocale = parseResult.GetValue(localeOption)
                ?? (localization.IsMultiLocale ? localization.DefaultLocale : null);

            var tree = await services.GetRequiredService<NavigationBuilder>()
                .BuildTreeAsync(toc, currentPath: null, locale: effectiveLocale);

            // Recover per-page flags the nav tree drops by keying TOC items on canonical path.
            var byPath = new Dictionary<string, ContentTocItem>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in toc)
            {
                byPath[item.Route.CanonicalPath.Value] = item;
            }

            var localeForSlug = effectiveLocale ?? localization.DefaultLocale;
            IReadOnlyList<NavigationTreeItem> sections = tree;
            if (!string.IsNullOrEmpty(areaSlug))
            {
                var match = tree.FirstOrDefault(n =>
                    string.Equals(SlugOf(n, localization, localeForSlug), areaSlug, StringComparison.OrdinalIgnoreCase));
                if (match is null)
                {
                    var available = string.Join(", ", tree.Select(n => SlugOf(n, localization, localeForSlug)));
                    output.WriteLine($"No area '{areaSlug}' found. Available: {available}");
                    return 2;
                }

                sections = [match];
            }

            var title = string.IsNullOrWhiteSpace(options.SiteTitle) ? "(untitled)" : options.SiteTitle;
            output.WriteLine($"{title}  ({toc.Count} pages, {sections.Count} section{(sections.Count == 1 ? "" : "s")})");
            output.WriteLine();

            foreach (var section in sections)
            {
                output.WriteLine($"{section.Title}  ({RouteLabel(section)})");
                if (section.Children.Count > 0 && maxDepth > 1)
                {
                    AsciiTreeWriter.Write(
                        output,
                        section.Children,
                        node => Label(node, byPath),
                        node => node.Children,
                        maxDepth - 1);
                }

                output.WriteLine();
            }

            return 0;
        });
        return command;
    }

    private static string SlugOf(NavigationTreeItem node, LocalizationOptions localization, string locale)
    {
        var stripped = localization.StripLocalePrefix(node.Route.CanonicalPath.Value, locale).Trim('/');
        if (!string.IsNullOrEmpty(stripped))
        {
            return stripped.Split('/')[0];
        }

        return node.Title.Replace(' ', '-').ToLowerInvariant();
    }

    private static string RouteLabel(NavigationTreeItem node)
    {
        var path = node.Route.CanonicalPath.Value;
        return string.IsNullOrEmpty(path) ? "section" : path;
    }

    private static string Label(NavigationTreeItem node, IReadOnlyDictionary<string, ContentTocItem> byPath)
    {
        var path = node.Route.CanonicalPath.Value;
        var flags = "";
        if (byPath.TryGetValue(path, out var item))
        {
            var parts = new List<string>();
            if (item.SearchOnly)
            {
                parts.Add("search-only");
            }

            if (item.ExcludeFromSearch)
            {
                parts.Add("no-search");
            }

            if (item.ExcludeFromLlms)
            {
                parts.Add("no-llms");
            }

            if (parts.Count > 0)
            {
                flags = "  [" + string.Join("] [", parts) + "]";
            }
        }

        var route = string.IsNullOrEmpty(path) ? "" : $"  {path}";
        return $"{node.Title}{route}  #{node.Order}{flags}";
    }
}
