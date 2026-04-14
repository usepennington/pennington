namespace GettingStartedFirstPageExample;

/// <summary>
/// Stage 3 — a third markdown file (<c>Content/contact.md</c>) lands on disk.
/// <c>order: 30</c> in its front matter places it after <c>about.md</c>
/// (<c>order: 20</c>) in the auto-assembled nav. As in <see cref="Stage2"/>,
/// no code changes — only content does. Tutorial prose extracts the body of
/// <see cref="Run"/> via <c>xmldocid,bodyonly</c>. This class is never
/// instantiated.
/// </summary>
/// <remarks>
/// The on-disk content progression at this stage is:
/// <list type="bullet">
///   <item><description><c>Content/index.md</c> — title: Welcome</description></item>
///   <item><description><c>Content/about.md</c> — title: About, order: 20</description></item>
///   <item><description><c>Content/contact.md</c> — title: Contact, order: 30</description></item>
/// </list>
/// </remarks>
public static class Stage3
{
    /// <summary>
    /// Run a Pennington host with three markdown files. The host code is still
    /// the same code as <see cref="Stage1"/> — front matter drives everything.
    /// </summary>
    public static Task Run(string[] args) => Stage1.Run(args);
}
