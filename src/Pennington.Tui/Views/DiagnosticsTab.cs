namespace Pennington.Tui.Views;

using Pennington.Diagnostics;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;
using static Pennington.Tui.Views.TuiMarkup;

/// <summary>
/// Diagnostics tab: per-URL diagnostics captured as the user navigates the site.
/// Empty until someone hits a page — there is no background crawl. The tab header
/// reflects the aggregate state across all visited pages (error/warning counts or
/// "all clean").
/// </summary>
internal static class DiagnosticsTab
{
    internal static Visual BuildHeader(TuiState state, PageDiagnosticsCollector collector) => new TextBlock(() =>
    {
        _ = state.RenderTick.Value;
        var snapshot = collector.Snapshot();
        if (snapshot.Count == 0) return "Diagnostics";
        int err = 0, warn = 0;
        foreach (var entry in snapshot)
        {
            foreach (var d in entry.Diagnostics)
            {
                if (d.Severity == DiagnosticSeverity.Error) err++;
                else if (d.Severity == DiagnosticSeverity.Warning) warn++;
            }
        }
        if (err + warn == 0) return $"Diagnostics ({snapshot.Count} pages, clean)";
        return $"Diagnostics ({err} err, {warn} warn, {snapshot.Count} pages)";
    }) { Wrap = false };

    internal static Visual BuildContent(LogControl diagnostics) =>
        new Group()
            .TopLeftText("By page")
            .Padding(1)
            .Content(diagnostics)
            .Stretch();

    /// <summary>
    /// Rebuilds the diagnostics log whenever the collector snapshot changes. The
    /// signature combines the page count and the newest capture timestamp so we only
    /// redraw after a real update.
    /// </summary>
    internal static Action CreatePump(PageDiagnosticsCollector collector, LogControl diagnostics)
    {
        long lastSignature = -1;

        return () =>
        {
            var snapshot = collector.Snapshot();
            var signature = ComputeSignature(snapshot);
            if (signature == lastSignature) return;
            lastSignature = signature;

            diagnostics.Clear();
            if (snapshot.Count == 0)
            {
                diagnostics.AppendMarkupLine("[dim](no pages visited yet — open one in your browser)[/]");
                return;
            }

            foreach (var entry in snapshot)
            {
                var severity = MaxSeverity(entry);
                var (tag, color) = severity switch
                {
                    DiagnosticSeverity.Error => ("ERR", "red"),
                    DiagnosticSeverity.Warning => ("WRN", "yellow"),
                    _ => ("OK ", "green"),
                };
                diagnostics.AppendMarkupLine($"[{color}]{tag}[/] [bold]{Escape(entry.Path)}[/]  [dim]{entry.CapturedAt:HH:mm:ss}[/]");
                if (entry.Diagnostics.IsDefaultOrEmpty)
                {
                    diagnostics.AppendMarkupLine("     [dim](clean)[/]");
                }
                else
                {
                    foreach (var d in entry.Diagnostics)
                    {
                        var dColor = d.Severity switch
                        {
                            DiagnosticSeverity.Error => "red",
                            DiagnosticSeverity.Warning => "yellow",
                            _ => "cyan",
                        };
                        var source = string.IsNullOrEmpty(d.Source) ? "" : $" [dim]({Escape(d.Source)})[/]";
                        diagnostics.AppendMarkupLine($"     [{dColor}]-[/] {Escape(d.Message)}{source}");
                    }
                }
                diagnostics.AppendMarkupLine("");
            }
        };
    }

    private static DiagnosticSeverity? MaxSeverity(PageDiagnosticsEntry entry)
    {
        if (entry.Diagnostics.IsDefaultOrEmpty) return null;
        DiagnosticSeverity? max = null;
        foreach (var d in entry.Diagnostics)
        {
            if (max is null || d.Severity > max) max = d.Severity;
        }
        return max;
    }

    private static long ComputeSignature(IReadOnlyList<PageDiagnosticsEntry> snapshot)
    {
        if (snapshot.Count == 0) return 0;
        long acc = snapshot.Count;
        foreach (var entry in snapshot)
        {
            acc = HashCode.Combine(acc, entry.Path, entry.CapturedAt.UtcTicks, entry.Diagnostics.IsDefaultOrEmpty ? 0 : entry.Diagnostics.Length);
        }
        return acc;
    }
}
