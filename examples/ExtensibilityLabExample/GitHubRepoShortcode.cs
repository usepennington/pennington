namespace ExtensibilityLabExample;

using System.Net;
using Pennington.Markdown.Shortcodes;

/// <summary>
/// Implements <see cref="IShortcode"/>. Turns <c>&lt;?# GitHubRepo "owner/repo" /?&gt;</c>
/// into an anchor pointing at <c>https://github.com/owner/repo</c>. Demonstrates a
/// positional argument and raw-HTML output — the contrast piece to the built-in
/// <c>&lt;?# Version /?&gt;</c> shortcode that ships with Pennington.
/// <para>
/// Backs how-to 2.3.25 <c>/how-to/markdown-pipeline/shortcodes</c>.
/// </para>
/// </summary>
public sealed class GitHubRepoShortcode : IShortcode
{
    /// <inheritdoc />
    public string Name => "GitHubRepo";

    /// <inheritdoc />
    public Task<string> ExecuteAsync(
        ShortcodeInvocation invocation,
        ShortcodeContext context,
        CancellationToken cancellationToken)
    {
        // Throw idiomatic guard clauses — the expander catches and degrades to a
        // build warning + HTML comment so the page still ships.
        if (invocation.PositionalArgs.Count == 0)
        {
            throw new ArgumentException("GitHubRepo requires a repo slug as the first positional argument.");
        }

        var slug = invocation.PositionalArgs[0];
        var encoded = WebUtility.HtmlEncode(slug);
        var html = $"""<a class="github-repo" data-extensibility-lab="github-repo-shortcode" href="https://github.com/{encoded}">{encoded}</a>""";
        return Task.FromResult(html);
    }
}
