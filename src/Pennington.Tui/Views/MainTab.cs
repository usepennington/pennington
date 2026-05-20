namespace Pennington.Tui.Views;

using Microsoft.Extensions.Logging;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;
using static Pennington.Tui.Views.TuiMarkup;

/// <summary>
/// Main tab: three stacked panels showing the live dev signal — HTTP requests,
/// ILogger output, and files edited this session (deduped).
/// Each panel is a <see cref="LogControl"/> so <c>[markup]</c> tags are interpreted
/// and each entry is a real line (plain <c>TextBlock</c> collapses newlines and
/// renders tags literally).
/// </summary>
internal static class MainTab
{
    internal static Visual Build(LogControl requests, LogControl logs, LogControl fileChanges) =>
        new VSplitter(
                new VSplitter(
                        Panel("Requests", requests),
                        Panel("Logs", logs))
                    .Ratio(0.5),
                Panel("File changes", fileChanges))
            .Ratio(0.75)
            .HorizontalAlignment(Align.Stretch)
            .VerticalAlignment(Align.Stretch);

    private static Group Panel(string title, Visual content) =>
        new Group()
            .TopLeftText(title)
            .Padding(1)
            .Content(content)
            .Stretch();

    /// <summary>
    /// Returns an action the terminal-thread tick invokes to pump new entries into the
    /// three LogControls. Closes over a pair of sequence counters (requests, logs) and
    /// a last-count tracker for the deduped file-change table.
    /// </summary>
    internal static Action CreatePump(
        LogControl requestsLog,
        LogControl logsLog,
        LogControl fileChangesLog,
        BoundedSequenceLog<RequestEntry> requests,
        BoundedSequenceLog<LogEntry> logs,
        FileChangeLog fileChanges)
    {
        long lastRequestSeq = 0;
        long lastLogSeq = 0;
        var lastFileChangeSignature = -1;

        return () =>
        {
            foreach (var e in requests.Snapshot())
            {
                if (e.Sequence <= lastRequestSeq)
                {
                    continue;
                }

                lastRequestSeq = e.Sequence;
                requestsLog.AppendMarkupLine(FormatRequest(e));
            }

            foreach (var e in logs.Snapshot())
            {
                if (e.Sequence <= lastLogSeq)
                {
                    continue;
                }

                lastLogSeq = e.Sequence;
                logsLog.AppendMarkupLine(FormatLog(e));
                if (e.Exception is { } ex)
                {
                    logsLog.AppendMarkupLine($"     [red]{Escape(ex.GetType().Name)}: {Escape(ex.Message)}[/]");
                }
            }

            // File changes dedup by path, so we can't stream — when the table changes
            // we clear and redraw the whole thing. A cheap signature (count + newest
            // timestamp ticks) dodges redrawing every tick when nothing is happening.
            var fcSnapshot = fileChanges.Snapshot();
            var signature = fcSnapshot.Count == 0
                ? 0
                : HashCode.Combine(fcSnapshot.Count, fcSnapshot[0].LastChanged.UtcTicks, fcSnapshot[0].Count);
            if (signature != lastFileChangeSignature)
            {
                lastFileChangeSignature = signature;
                fileChangesLog.Clear();
                foreach (var e in fcSnapshot.Take(200))
                {
                    fileChangesLog.AppendMarkupLine(FormatFileChange(e));
                }
            }
        };
    }

    private static string FormatRequest(RequestEntry e)
    {
        var color = e.Status switch
        {
            >= 500 => "red",
            >= 400 => "yellow",
            >= 300 => "cyan",
            >= 200 => "green",
            _ => "gray",
        };
        var ms = e.Duration.TotalMilliseconds;
        return $"[dim]{e.Timestamp:HH:mm:ss}[/] [bold]{e.Method,-6}[/] [{color}]{e.Status}[/]  {Escape(e.Path)}{Escape(e.QueryString)}  [dim]{ms:F0}ms[/]";
    }

    private static string FormatLog(LogEntry e)
    {
        var (tag, color) = e.Level switch
        {
            LogLevel.Critical => ("CRIT", "red"),
            LogLevel.Error => ("ERR ", "red"),
            LogLevel.Warning => ("WARN", "yellow"),
            LogLevel.Information => ("INFO", "green"),
            LogLevel.Debug => ("DBUG", "gray"),
            LogLevel.Trace => ("TRCE", "gray"),
            _ => ("    ", "white"),
        };
        return $"[dim]{e.Timestamp:HH:mm:ss}[/] [{color}]{tag}[/] [dim]{Escape(ShortCategory(e.Category))}[/]  {Escape(e.Message)}";
    }

    private static string FormatFileChange(FileChangeEntry e)
    {
        var name = ShortenPath(e.FullPath);
        var countText = e.Count == 1 ? "1x" : $"{e.Count}x";
        return $"[dim]{e.LastChanged:HH:mm:ss}[/]  [bold]{countText,4}[/]  {Escape(name)}";
    }

    // Collapses logger category to the rightmost two dotted segments so noisy namespaces
    // ("Microsoft.AspNetCore.Hosting.Diagnostics") don't hog the row.
    private static string ShortCategory(string category)
    {
        var parts = category.Split('.');
        return parts.Length <= 2 ? category : $"{parts[^2]}.{parts[^1]}";
    }

    private static string ShortenPath(string fullPath)
    {
        try
        {
            var cwd = Environment.CurrentDirectory;
            if (fullPath.StartsWith(cwd, StringComparison.OrdinalIgnoreCase))
            {
                var rel = fullPath[cwd.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return rel.Length > 0 ? rel : fullPath;
            }
        }
        catch { }
        return Path.GetFileName(fullPath);
    }

}