namespace Pennington.DocSite;

using Pennington.Head;

/// <summary>
/// Contributes DocSite's site-invariant discovery meta to the document head: OpenGraph/Twitter site
/// identity, the site-wide default card image (when generated social cards are off), and the RSS
/// alternate link when the blog is active. Replaces the literal markup that lived in
/// <c>App.razor</c>'s head.
/// </summary>
internal sealed class DocSiteHeadContributor : IHeadContributor
{
    private readonly DocSiteOptions _options;
    private readonly BlogFeature _blog;

    /// <summary>Creates the contributor from the DocSite options and blog-feature flag.</summary>
    public DocSiteHeadContributor(DocSiteOptions options, BlogFeature blog)
    {
        _options = options;
        _blog = blog;
    }

    /// <inheritdoc/>
    public int Order => HeadOrder.Site;

    /// <inheritdoc/>
    public bool ShouldContribute(HeadContext context) => true;

    /// <inheritdoc/>
    public Task ContributeAsync(HeadContext context, HeadBuilder head)
    {
        head.Property("og:site_name", _options.SiteTitle);
        head.Meta("twitter:site", _options.SiteTitle);

        // Site-wide default card image when generated social cards are off; when they're on, each
        // page emits its own og:image which wins by head dedup, so the default steps aside.
        if (_options.SocialCards is null && !string.IsNullOrEmpty(_options.SocialImageUrl))
        {
            head.Property("og:image", _options.SocialImageUrl);
            head.Meta("twitter:image", _options.SocialImageUrl);
            head.Meta("twitter:card", "summary_large_image");
        }

        if (_blog.Enabled)
        {
            head.AddRepeatable(new HeadTag(new LinkTag("alternate", "/rss.xml")
            {
                Attributes = [new("type", "application/rss+xml"), new("title", _options.SiteTitle)],
            }));
        }

        return Task.CompletedTask;
    }
}
