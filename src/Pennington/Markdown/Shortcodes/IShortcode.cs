namespace Pennington.Markdown.Shortcodes;

/// <summary>
/// Handler for a named shortcode invocation expanded before Markdig parsing.
/// Implementations are registered as DI services and dispatched by <see cref="Name"/>
/// (case-insensitive). The string returned by <see cref="ExecuteAsync"/> is spliced
/// into the markdown source and then parsed as markdown — return raw HTML when the
/// output should bypass markdown processing (use HTML block syntax so Markdig leaves
/// it intact).
/// </summary>
public interface IShortcode
{
    /// <summary>Case-insensitive name used to dispatch <c>&lt;?# Name ... ?&gt;</c> invocations.</summary>
    string Name { get; }

    /// <summary>
    /// Produces the replacement text for one invocation. <paramref name="invocation"/> carries the
    /// parsed arguments and inline content (null for self-closing tags); <paramref name="context"/>
    /// carries the host page's route and metadata.
    /// </summary>
    Task<string> ExecuteAsync(
        ShortcodeInvocation invocation,
        ShortcodeContext context,
        CancellationToken cancellationToken);
}
