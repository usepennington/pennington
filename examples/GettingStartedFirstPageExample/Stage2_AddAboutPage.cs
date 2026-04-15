namespace GettingStartedFirstPageExample;

/// <summary>
/// Stage 2 — a second markdown file (<c>Content/about.md</c>) is added on disk.
/// The host code is byte-for-byte identical to <see cref="Stage1"/>: nothing
/// in <c>Program.cs</c> changes when a new content file appears. The tutorial
/// uses this stage to show that the nav strip picks up the second entry for
/// free, which is the whole pedagogical point. Tutorial prose extracts the
/// body of <see cref="Run"/> via <c>xmldocid,bodyonly</c>. This class is never
/// instantiated.
/// </summary>
/// <remarks>
/// The on-disk content progression at this stage is:
/// <list type="bullet">
///   <item><description><c>Content/index.md</c> — title: Welcome</description></item>
///   <item><description><c>Content/about.md</c> — title: About</description></item>
/// </list>
/// </remarks>
public static class Stage2
{
    /// <summary>
    /// Run a Pennington host whose Content folder now holds two markdown files.
    /// No code change is needed vs <see cref="Stage1"/>.
    /// </summary>
    public static Task Run(string[] args) => Stage1.Run(args);
}