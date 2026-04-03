using Penn.Feeds;
using Penn.Routing;

namespace Penn.Tests.Feeds;

public class FeedTests
{
    [Fact]
    public void SitemapEntry_CreateWithAllFields()
    {
        var entry = new SitemapEntry(
            Url: new UrlPath("/docs/getting-started"),
            LastModified: new DateTime(2026, 1, 15),
            ChangeFrequency: "weekly",
            Priority: 0.8);

        entry.Url.Value.ShouldBe("/docs/getting-started");
        entry.LastModified.ShouldBe(new DateTime(2026, 1, 15));
        entry.ChangeFrequency.ShouldBe("weekly");
        entry.Priority.ShouldBe(0.8);
    }

    [Fact]
    public void SitemapEntry_CreateWithNullOptionalFields()
    {
        var entry = new SitemapEntry(
            Url: new UrlPath("/about"),
            LastModified: null,
            ChangeFrequency: null,
            Priority: null);

        entry.Url.Value.ShouldBe("/about");
        entry.LastModified.ShouldBeNull();
        entry.ChangeFrequency.ShouldBeNull();
        entry.Priority.ShouldBeNull();
    }

    [Fact]
    public void RssFeedItem_CreateWithAllFields()
    {
        var item = new RssFeedItem(
            Title: "New Release",
            Description: "We are excited to announce version 2.0",
            Url: new UrlPath("/blog/new-release"),
            PublishDate: new DateTime(2026, 3, 20),
            Author: "Jane Doe");

        item.Title.ShouldBe("New Release");
        item.Description.ShouldBe("We are excited to announce version 2.0");
        item.Url.Value.ShouldBe("/blog/new-release");
        item.PublishDate.ShouldBe(new DateTime(2026, 3, 20));
        item.Author.ShouldBe("Jane Doe");
    }

    [Fact]
    public void RssFeedItem_CreateWithNullOptionalFields()
    {
        var item = new RssFeedItem(
            Title: "Minimal Post",
            Description: null,
            Url: new UrlPath("/blog/minimal"),
            PublishDate: null,
            Author: null);

        item.Title.ShouldBe("Minimal Post");
        item.Description.ShouldBeNull();
        item.Url.Value.ShouldBe("/blog/minimal");
        item.PublishDate.ShouldBeNull();
        item.Author.ShouldBeNull();
    }
}
