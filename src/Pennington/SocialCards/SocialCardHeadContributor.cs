namespace Pennington.SocialCards;

using Head;
using Infrastructure;

/// <summary>
/// Emits the generated card's <c>og:image</c>/<c>twitter:image</c>/<c>twitter:card</c> for every
/// page that resolved a <see cref="Content.ContentRecord"/> — markdown on any host, Razor pages
/// with sidecar metadata — so social-card meta tags work without template-specific wiring. A page
/// that authors its own <c>og:image</c> (e.g. via an author-supplied image factory) wins through
/// head reconciliation. Registered only when <see cref="PenningtonOptions.SocialCards"/> is set.
/// </summary>
internal sealed class SocialCardHeadContributor : IHeadContributor
{
    private readonly SocialCardOptions _options;
    private readonly PenningtonOptions _site;

    /// <summary>Creates the contributor from the card and site options.</summary>
    public SocialCardHeadContributor(SocialCardOptions options, PenningtonOptions site)
    {
        _options = options;
        _site = site;
    }

    /// <inheritdoc/>
    public int Order => HeadOrder.Page;

    /// <inheritdoc/>
    /// <remarks>Pages without a content record have no discovered card route, so there is nothing to tag.</remarks>
    public bool ShouldContribute(HeadContext context) => context.Record is not null;

    /// <inheritdoc/>
    public Task ContributeAsync(HeadContext context, HeadBuilder head)
    {
        var url = SocialCardUrl.For(context.Record!.Route.CanonicalPath, _options.BaseUrl, _site.CanonicalBaseUrl);
        head.Property("og:image", url);
        head.Meta("twitter:image", url);
        head.Meta("twitter:card", "summary_large_image");
        return Task.CompletedTask;
    }
}
