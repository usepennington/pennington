namespace Pennington.Cli;

/// <summary>
/// The mode the Pennington host runs in, derived from the CLI verb. Drives whether the host
/// serves live, performs a one-shot static build, or runs a one-shot diagnostic command.
/// <see cref="Build"/> and <see cref="Diag"/> are both headless one-shot modes; only
/// <see cref="Build"/> writes output and forces strict front-matter keys.
/// </summary>
internal enum PenningtonRunMode
{
    /// <summary>Dev server (default — no verb). Live reload, dev overlays, short shutdown timeout.</summary>
    Serve,

    /// <summary>One-shot static build (<c>build</c> verb). Crawl in-process, write to disk, then exit.</summary>
    Build,

    /// <summary>One-shot diagnostic command (<c>diag &lt;sub&gt;</c> verb). Inspect in-process, emit a report, then exit.</summary>
    Diag,
}
