using Pennington.BlogSite;
using Pennington.BlogSite.Services;
using Pennington.Content;
using Pennington.FrontMatter;

namespace Pennington.Tests.BlogSite;

public class BlogSiteContentServiceRecordsTests
{
    [Fact]
    public async Task GetRecordsAsync_ProjectsHomePageRecord_WithSiteIdentity()
    {
        var options = new BlogSiteOptions
        {
            SiteTitle = "My Blog",
            SiteDescription = "Posts about things",
            ContentRootPath = ".",
        };
        var service = new BlogSiteContentService(options, new FrontMatterParser());

        var records = new List<ContentRecord>();
        await foreach (var record in service.GetRecordsAsync())
        {
            records.Add(record);
        }

        var home = records.ShouldHaveSingleItem();
        home.Route.CanonicalPath.Value.ShouldBe("/");
        home.Metadata.Title.ShouldBe("My Blog");
        home.Metadata.Description.ShouldBe("Posts about things");
    }
}
