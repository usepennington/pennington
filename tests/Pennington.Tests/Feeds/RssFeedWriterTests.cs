using Pennington.Feeds;
using Pennington.Routing;

namespace Pennington.Tests.Feeds;

public class RssFeedWriterTests
{
    private static RssFeedItem Item(
        string title, string path, DateTime? date, string? description = null, string? author = null) =>
        new(title, description, new UrlPath(path), date, author);

    [Fact]
    public void WriteXml_EmitsChannelMetadata_AndAtomSelf_WhenCanonicalBaseSet()
    {
        var xml = RssFeedWriter.WriteXml("My Blog", "Notes and posts", "https://example.com/",
            [Item("Post", "/blog/post/", new DateTime(2026, 3, 1))]);

        xml.ShouldContain("<title>My Blog</title>");
        xml.ShouldContain("<description>Notes and posts</description>");
        xml.ShouldContain("<link>https://example.com/</link>");
        xml.ShouldContain("rel=\"self\"");
        xml.ShouldContain("https://example.com/rss.xml");
    }

    [Fact]
    public void WriteXml_NoCanonicalBase_UsesRelativeLinks_AndOmitsAtomSelf()
    {
        var xml = RssFeedWriter.WriteXml("My Blog", "Notes", null,
            [Item("Post", "/blog/post/", new DateTime(2026, 3, 1))]);

        xml.ShouldContain("<link>/</link>");
        xml.ShouldContain("<link>/blog/post/</link>");
        xml.ShouldNotContain("rel=\"self\"");
    }

    [Fact]
    public void WriteXml_EmitsItemFields()
    {
        var xml = RssFeedWriter.WriteXml("Blog", "desc", "https://example.com",
            [Item("My Post", "/blog/my-post/", new DateTime(2026, 3, 15), "A summary", "Ada")]);

        xml.ShouldContain("<title>My Post</title>");
        xml.ShouldContain("<link>https://example.com/blog/my-post/</link>");
        xml.ShouldContain("isPermaLink=\"true\">https://example.com/blog/my-post/</guid>");
        xml.ShouldContain("<description>A summary</description>");
        xml.ShouldContain("<author>Ada</author>");
        xml.ShouldContain("<pubDate>");
    }

    [Fact]
    public void WriteXml_ExcludesUndatedItems_AndOrdersNewestFirst()
    {
        var xml = RssFeedWriter.WriteXml("Blog", "desc", "https://example.com",
        [
            Item("Old", "/blog/old/", new DateTime(2025, 1, 1)),
            Item("New", "/blog/new/", new DateTime(2026, 6, 1)),
            Item("Undated", "/blog/undated/", null),
        ]);

        xml.ShouldNotContain("/blog/undated/");
        xml.IndexOf("New", StringComparison.Ordinal)
            .ShouldBeLessThan(xml.IndexOf("Old", StringComparison.Ordinal));
    }

    [Fact]
    public void WriteXml_OmitsAuthorAndItemDescription_WhenAbsent()
    {
        var xml = RssFeedWriter.WriteXml("Blog", "Site description", "https://example.com",
            [Item("No Extras", "/blog/x/", new DateTime(2026, 3, 1))]);

        xml.ShouldNotContain("<author>");
        // The channel keeps its description; the item adds none.
        xml.Split("<item>")[1].ShouldNotContain("<description>");
    }
}
