namespace Pennington.Tui.Views;

using XenoAtom.Terminal;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Controls;
using XenoAtom.Terminal.UI.Geometry;
using XenoAtom.Terminal.UI.Input;
using XenoAtom.Terminal.UI.Styling;

/// <summary>
/// The root TUI view — a full-screen dashboard modeled on the
/// <c>XenoAtom.Terminal.UI</c> FullscreenDemo sample: a <see cref="DockLayout"/>
/// root with <see cref="Header"/> + visible button row on top and <see cref="Footer"/>
/// at the bottom. The main content area is a <see cref="TabControl"/> with three
/// tabs: a live dashboard (requests + logs + file changes), a content catalog, and
/// on-demand per-page diagnostics.
///
/// Reactivity rule (learned from the sample): <c>State&lt;T&gt;</c> mutations must
/// happen on the terminal thread. The hosted service writes to plain fields from
/// background tasks; the <c>Terminal.Run</c> callback below polls on a tick
/// and bumps <see cref="TuiState.RenderTick"/> from here so the framework's
/// dependency tracker picks up the changes. The same callback drains the bounded
/// queues into each tab's <see cref="LogControl"/>; the controls are owned here so
/// the tick closure can append from a single place.
/// </summary>
internal static class TuiApp
{
    internal static void Run(
        TuiState state,
        BoundedSequenceLog<RequestEntry> requests,
        BoundedSequenceLog<LogEntry> logs,
        FileChangeLog fileChanges,
        PageDiagnosticsCollector pageDiagnostics,
        Action requestShutdown,
        CancellationToken cancellationToken)
    {
        using var session = Terminal.Open();

        // Default is false — Ctrl+C triggers SIGINT / host shutdown, which our
        // cancellationToken check below catches on the next tick (up to 100ms late).
        // Flipping this makes Ctrl+C arrive as a normal key event so our explicit
        // binding below runs immediately alongside Ctrl+Q.
        Terminal.TreatControlCAsInput = true;

        var header = new Header
        {
            Left = new Markup("[bold]Pennington[/] — Dev Dashboard") { Wrap = false },
            Center = new TextBlock(() =>
            {
                _ = state.RenderTick.Value;
                return state.AppUrl ?? "(binding...)";
            }),
            Right = new TextBlock(() =>
            {
                _ = state.RenderTick.Value;
                var pageCount = pageDiagnostics.Snapshot().Count;
                return pageCount == 0 ? "" : $"pages visited: {pageCount}";
            }),
        };

        // One LogControl per panel so markup renders as styling and each append is a
        // real line. All panels auto-follow the tail so newly-written rows are visible
        // without manual scrolling. Vertical padding collapses to 0 so the Group frame
        // hugs the rows tightly — the horizontal pad (1) keeps text off the border.
        var requestsLog = MakeLogControl();
        var logsLog = MakeLogControl();
        var fileChangesLog = MakeLogControl();
        var diagnosticsLog = MakeLogControl();
        var contentTree = new TreeView();
        // Default TreeView glyphs are unicode triangles; swap to ASCII so the tab
        // stays free of decorative chars (matches the rest of the dashboard).
        contentTree.Style(TreeViewStyle.Default with
        {
            ExpandedGlyph = new System.Text.Rune('-'),
            CollapsedGlyph = new System.Text.Rune('+'),
            FocusMarkerGlyph = new System.Text.Rune(' '),
            IconResolver = null,
        });

        static LogControl MakeLogControl()
        {
            var log = new LogControl { FollowTail = true };
            log.Style(LogControlStyle.Default with
            {
                Padding = new Thickness(1, 0, 1, 0),
            });
            return log;
        }

        var pumpMain = MainTab.CreatePump(requestsLog, logsLog, fileChangesLog, requests, logs, fileChanges);
        var pumpContent = ContentTab.CreatePump(state, contentTree);
        var pumpDiagnostics = DiagnosticsTab.CreatePump(pageDiagnostics, diagnosticsLog);

        var tabs = new TabControl(
            new TabPage(new TextBlock("Main") { Wrap = false }, MainTab.Build(requestsLog, logsLog, fileChangesLog)),
            new TabPage(new TextBlock("Content") { Wrap = false }, ContentTab.Build(contentTree)),
            new TabPage(DiagnosticsTab.BuildHeader(state, pageDiagnostics), DiagnosticsTab.BuildContent(diagnosticsLog)));

        var footer = new Footer
        {
            Left = new TextBlock(() =>
            {
                _ = state.RenderTick.Value;
                var locales = state.Locales.Count == 0 ? "(none)" : string.Join(",", state.Locales);
                return $"locales: {locales}";
            }),
            Center = new Markup("arrow keys switch tabs, Ctrl+C or Ctrl+Q to quit") { Wrap = false },
            Right = new TextBlock(() =>
            {
                _ = state.RenderTick.Value;
                var total = 0;
                foreach (var g in state.ContentGroups)
                {
                    total += g.Items.Count;
                }

                return $"pages: {total}";
            }),
        };

        var root = new DockLayout()
            .HorizontalAlignment(Align.Stretch)
            .VerticalAlignment(Align.Stretch)
            .Top(header)
            .Content(tabs)
            .Bottom(footer);

        // Ctrl+C routes here (via TreatControlCAsInput above); Ctrl+Q is XenoAtom's
        // built-in exit chord, which breaks out of Terminal.Run without hitting our
        // requestShutdown — so we bind it explicitly too. Both paths end in
        // IHostApplicationLifetime.StopApplication, which unwinds Kestrel.
        root.AddKeyBinding(new KeyGesture('c', TerminalModifiers.Ctrl), requestShutdown);
        root.AddKeyBinding(new KeyGesture('q', TerminalModifiers.Ctrl), requestShutdown);

        // Polling loop so the update callback fires on a timer even when no input
        // arrives. Without this the callback is event-driven, so shutdown requests
        // (Quit button, Ctrl+C via lifetime) aren't observed until the next
        // keystroke — the process would hang indefinitely on an idle TUI.
        var runOptions = new TerminalRunOptions
        {
            LoopMode = TerminalLoopMode.Polling,
            UpdateWaitDuration = TimeSpan.FromMilliseconds(100),
        };

        Terminal.Run(root, _ =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return TerminalLoopResult.StopAndKeepVisual;
            }

            pumpMain();
            pumpContent();
            pumpDiagnostics();
            state.RenderTick.Value++;
            return TerminalLoopResult.Continue;
        }, runOptions);
    }
}