namespace Pennington.Favicon;

using Head;

/// <summary>
/// Emits the configured favicon / icon <c>&lt;link&gt;</c> tags into the document head. Each icon is a
/// repeatable link (multiple <c>rel="icon"</c> of different sizes must not deduplicate), carrying an
/// inferred or explicit <c>type</c> plus optional <c>sizes</c>/<c>color</c>. The root-relative hrefs are
/// sub-path prefixed by the base-URL rewriter exactly as literal head markup is, and ride the head
/// subsystem's <c>data-head</c> stamping for SPA persistence.
/// </summary>
internal sealed class FaviconHeadContributor : IHeadContributor
{
    private readonly FaviconOptions _options;

    /// <summary>Creates the contributor from the favicon options.</summary>
    public FaviconHeadContributor(FaviconOptions options) => _options = options;

    /// <inheritdoc/>
    public int Order => HeadOrder.Site;

    /// <inheritdoc/>
    public bool ShouldContribute(HeadContext context) => _options.Icons.Length > 0;

    /// <inheritdoc/>
    public Task ContributeAsync(HeadContext context, HeadBuilder head)
    {
        foreach (var icon in _options.Icons)
        {
            var attributes = new List<KeyValuePair<string, string?>>();

            var type = icon.Type ?? FaviconMimeTypes.InferFromHref(icon.Href);
            if (type is not null)
            {
                attributes.Add(new("type", type));
            }

            if (icon.Sizes is { } sizes)
            {
                attributes.Add(new("sizes", sizes));
            }

            if (icon.Color is { } color)
            {
                attributes.Add(new("color", color));
            }

            head.AddRepeatable(new HeadTag(new LinkTag(icon.Rel, icon.Href)
            {
                Attributes = [.. attributes],
            }));
        }

        return Task.CompletedTask;
    }
}
