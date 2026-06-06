namespace Pennington.BlogSite;

using Pennington.Head;
using Pennington.Infrastructure;

/// <summary>
/// Contributes BlogSite's RSS alternate link to the document head when the feed is enabled. Replaces
/// the literal markup that lived in <c>App.razor</c>'s head. The default <c>&lt;title&gt;</c> and the
/// site description stay in the layout (Blazor owns the title via <c>HeadOutlet</c>).
/// </summary>
internal sealed class BlogSiteHeadContributor : IHeadContributor
{
    private readonly BlogSiteOptions _options;
    private readonly PenningtonOptions _pennington;

    /// <summary>Creates the contributor from the BlogSite and core options.</summary>
    public BlogSiteHeadContributor(BlogSiteOptions options, PenningtonOptions pennington)
    {
        _options = options;
        _pennington = pennington;
    }

    /// <inheritdoc/>
    public int Order => HeadOrder.Site;

    /// <inheritdoc/>
    public bool ShouldContribute(HeadContext context) => _options.EnableRss;

    /// <inheritdoc/>
    public Task ContributeAsync(HeadContext context, HeadBuilder head)
    {
        head.AddRepeatable(new HeadTag(new LinkTag("alternate", "/rss.xml")
        {
            Attributes = [new("type", "application/rss+xml"), new("title", _pennington.SiteTitle)],
        }));
        return Task.CompletedTask;
    }
}
