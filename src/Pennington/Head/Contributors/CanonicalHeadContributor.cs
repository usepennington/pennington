namespace Pennington.Head;

using Infrastructure;

/// <summary>
/// Emits the default <c>&lt;link rel="canonical"&gt;</c> when
/// <see cref="PenningtonOptions.CanonicalBaseUrl"/> is set, unless the page already supplies its own
/// (the head reconciler leaves an authored canonical untouched). Replaces the former
/// <c>CanonicalLinkHtmlRewriter</c>.
/// </summary>
internal sealed class CanonicalHeadContributor : IHeadContributor
{
    private readonly PenningtonOptions _options;

    /// <summary>Creates the contributor from the site options.</summary>
    public CanonicalHeadContributor(PenningtonOptions options) => _options = options;

    /// <inheritdoc/>
    public int Order => HeadOrder.Site;

    /// <inheritdoc/>
    public bool ShouldContribute(HeadContext context) => !string.IsNullOrEmpty(_options.CanonicalBaseUrl);

    /// <inheritdoc/>
    public Task ContributeAsync(HeadContext context, HeadBuilder head)
    {
        // PathBase carries the stripped locale segment for non-default locales; FullPath reattaches
        // it, so the canonical URL matches the address the visitor typed.
        var path = string.IsNullOrEmpty(context.FullPath) ? "/" : context.FullPath;
        head.Link("canonical", Combine(_options.CanonicalBaseUrl!, path));
        return Task.CompletedTask;
    }

    private static string Combine(string baseUrl, string path)
    {
        var trimmedBase = baseUrl.TrimEnd('/');
        var trimmedPath = path.StartsWith('/') ? path : "/" + path;
        return trimmedBase + trimmedPath;
    }
}
