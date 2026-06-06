namespace Pennington.StandardSite;

using Head;

/// <summary>
/// Contributes the Standard Site verification <c>&lt;link&gt;</c> tags: the site-wide
/// <c>site.standard.publication</c> hint and, on pages that declare an rkey, the per-page
/// <c>site.standard.document</c> link. The <c>at://</c> hrefs are repeatable (never base-URL
/// prefixed) and ride the head subsystem's <c>data-head</c> stamping for SPA persistence.
/// </summary>
internal sealed class StandardSiteHeadContributor : IHeadContributor
{
    private readonly StandardSiteOptions _options;
    private readonly StandardSiteUriResolver _resolver;

    /// <summary>Creates the contributor from the options and the AT-URI resolver.</summary>
    public StandardSiteHeadContributor(StandardSiteOptions options, StandardSiteUriResolver resolver)
    {
        _options = options;
        _resolver = resolver;
    }

    /// <inheritdoc/>
    public int Order => HeadOrder.Discovery;

    /// <inheritdoc/>
    public bool ShouldContribute(HeadContext context) => _options.IsConfigured;

    /// <inheritdoc/>
    public async Task ContributeAsync(HeadContext context, HeadBuilder head)
    {
        if (_options.EmitPublicationLink)
        {
            head.AddRepeatable(new HeadTag(new LinkTag("site.standard.publication", _resolver.PublicationUri)));
        }

        if (await _resolver.DocumentUriAsync(context.FullPath) is { } documentUri)
        {
            head.AddRepeatable(new HeadTag(new LinkTag("site.standard.document", documentUri)));
        }
    }
}
