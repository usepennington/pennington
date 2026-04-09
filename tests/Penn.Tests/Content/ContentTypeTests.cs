using Pennington.Content;
using Pennington.Routing;

namespace Pennington.Tests.Content;

public class ContentTypeTests
{
    [Fact]
    public async Task ContentToCreate_GeneratorProducesContent()
    {
        var expected = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
        var create = new ContentToCreate(
            new FilePath("output/data.bin"),
            () => Task.FromResult(expected),
            "application/octet-stream"
        );

        var result = await create.ContentGenerator();
        result.ShouldBe(expected);
    }

    [Fact]
    public void ContentTocItem_HierarchyPartsPreservedCorrectly()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/a/b/c/"),
            OutputFile = new FilePath("a/b/c/index.html")
        };

        var parts = new[] { "root", "section", "page" };
        var tocItem = new ContentTocItem("Page", route, 1, parts, null, null);

        tocItem.HierarchyParts.Length.ShouldBe(3);
        tocItem.HierarchyParts[0].ShouldBe("root");
        tocItem.HierarchyParts[1].ShouldBe("section");
        tocItem.HierarchyParts[2].ShouldBe("page");
    }
}
