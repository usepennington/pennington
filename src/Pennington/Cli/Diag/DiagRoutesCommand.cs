namespace Pennington.Cli.Diag;

using System.CommandLine;
using Content;
using Microsoft.Extensions.DependencyInjection;
using Pipeline;

/// <summary><c>diag routes</c> — a flat list of every URL the site emits, with its kind and source file.</summary>
internal sealed class DiagRoutesCommand : IDiagCommand
{
    /// <inheritdoc/>
    public string Name => "routes";

    /// <inheritdoc/>
    public string Description => "List every emitted route: URL, kind (markdown/razor/redirect/...), and source file.";

    /// <inheritdoc/>
    public Command Build(IServiceProvider services, TextWriter output)
    {
        var kindOption = new Option<string>("--kind")
        {
            Description = "Filter by kind: markdown, razor, redirect, programmatic, endpoint, or llms-only.",
        };
        var localeOption = new Option<string>("--locale")
        {
            Description = "Filter by locale code.",
        };

        var command = new Command(Name, Description);
        command.Options.Add(kindOption);
        command.Options.Add(localeOption);
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var kindFilter = parseResult.GetValue(kindOption);
            var localeFilter = parseResult.GetValue(localeOption);

            var rows = new List<Row>();
            await foreach (var item in services.GetServices<IContentService>().DiscoverAllAsync(cancellationToken))
            {
                var kind = KindOf(item.Source);
                if (!string.IsNullOrEmpty(kindFilter) && !string.Equals(kind, kindFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(localeFilter) && !string.Equals(item.Route.Locale, localeFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var redirect = (item.Source.Value as RedirectSource)?.TargetUrl.Value;
                rows.Add(new Row(item.Route.CanonicalPath.Value, kind, item.Route.SourceFile?.Value, redirect));
            }

            rows.Sort((a, b) => string.CompareOrdinal(a.Url, b.Url));

            if (rows.Count == 0)
            {
                output.WriteLine("No routes found.");
                return 0;
            }

            var urlWidth = Math.Max(3, rows.Max(r => r.Url.Length));
            var kindWidth = Math.Max(4, rows.Max(r => r.Kind.Length));
            output.WriteLine($"{"URL".PadRight(urlWidth)}  {"KIND".PadRight(kindWidth)}  SOURCE");
            foreach (var row in rows)
            {
                var source = row.Redirect is not null
                    ? $"-> {row.Redirect}"
                    : string.IsNullOrEmpty(row.SourceFile) ? "(generated)" : row.SourceFile;
                output.WriteLine($"{row.Url.PadRight(urlWidth)}  {row.Kind.PadRight(kindWidth)}  {source}");
            }

            output.WriteLine();
            var byKind = rows.GroupBy(r => r.Kind).OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(g => $"{g.Count()} {g.Key}");
            output.WriteLine($"{rows.Count} route{(rows.Count == 1 ? "" : "s")} ({string.Join(", ", byKind)})");
            return 0;
        });
        return command;
    }

    private static string KindOf(ContentSource source) => source.Value switch
    {
        MarkdownFileSource => "markdown",
        RazorPageSource => "razor",
        RedirectSource => "redirect",
        EndpointSource => "endpoint",
        LlmsOnlySource => "llms-only",
        _ => "unknown",
    };

    private sealed record Row(string Url, string Kind, string? SourceFile, string? Redirect);
}
