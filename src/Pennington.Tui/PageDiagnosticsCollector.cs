namespace Pennington.Tui;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Pennington.Diagnostics;

/// <summary>
/// Per-URL snapshot of the most recent <see cref="DiagnosticContext.Diagnostics"/>
/// captured by <see cref="Infrastructure.TuiDiagnosticsCaptureMiddleware"/> as the
/// user navigates the site. Replaces the crawl-based <c>BuildReport</c> model —
/// diagnostics fill in on demand as pages are rendered, not ahead of time.
/// </summary>
public sealed class PageDiagnosticsCollector
{
    private readonly ConcurrentDictionary<string, PageDiagnosticsEntry> _byPath = new(StringComparer.Ordinal);

    /// <summary>Upsert the latest diagnostics snapshot for <paramref name="path"/>.</summary>
    public void Record(string path, DateTimeOffset at, ImmutableArray<Diagnostic> diagnostics)
    {
        _byPath[path] = new PageDiagnosticsEntry(path, at, diagnostics);
    }

    /// <summary>
    /// Snapshot of every page that's been visited, ordered so problems float to the top:
    /// errors first, then warnings, then info-only, then clean pages. Within a severity
    /// bucket, most recently captured first.
    /// </summary>
    public IReadOnlyList<PageDiagnosticsEntry> Snapshot()
    {
        var items = _byPath.Values.ToArray();
        Array.Sort(items, static (a, b) =>
        {
            var aSev = MaxSeverity(a);
            var bSev = MaxSeverity(b);
            if (aSev != bSev)
            {
                return bSev.CompareTo(aSev);
            }

            return b.CapturedAt.CompareTo(a.CapturedAt);
        });
        return items;
    }

    private static int MaxSeverity(PageDiagnosticsEntry e)
    {
        if (e.Diagnostics.IsDefaultOrEmpty)
        {
            return -1;
        }

        var max = -1;
        foreach (var d in e.Diagnostics)
        {
            var s = (int)d.Severity;
            if (s > max)
            {
                max = s;
            }
        }
        return max;
    }
}

/// <summary>One collected per-page diagnostics snapshot.</summary>
/// <param name="Path">Request path the snapshot belongs to.</param>
/// <param name="CapturedAt">When the snapshot was taken (end of that request).</param>
/// <param name="Diagnostics">Diagnostics the content pipeline recorded during that request.</param>
public readonly record struct PageDiagnosticsEntry(
    string Path,
    DateTimeOffset CapturedAt,
    ImmutableArray<Diagnostic> Diagnostics);