using Penn.Content;
using Penn.Routing;

namespace Penn.Tests.Content;

public class ContentTypeTests
{
    [Fact]
    public void ContentToCopy_CreatesWithSourceAndOutputPaths()
    {
        var copy = new ContentToCopy(new FilePath("images/logo.png"), new FilePath("output/images/logo.png"));

        copy.SourcePath.Value.ShouldBe("images/logo.png");
        copy.OutputPath.Value.ShouldBe("output/images/logo.png");
    }

    [Fact]
    public void ContentToCreate_CreatesWithGeneratorFunc()
    {
        var generator = () => Task.FromResult(new byte[] { 1, 2, 3 });
        var create = new ContentToCreate(new FilePath("output/search.json"), generator, "application/json");

        create.OutputPath.Value.ShouldBe("output/search.json");
        create.ContentGenerator.ShouldBe(generator);
        create.ContentType.ShouldBe("application/json");
    }

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
    public void ContentTocItem_CreatesWithAllProperties()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/docs/getting-started"),
            OutputFile = new FilePath("docs/getting-started/index.html")
        };

        var tocItem = new ContentTocItem(
            Title: "Getting Started",
            Route: route,
            Order: 10,
            HierarchyParts: ["docs", "getting-started"],
            Section: "Documentation",
            Locale: "en"
        );

        tocItem.Title.ShouldBe("Getting Started");
        tocItem.Route.ShouldBe(route);
        tocItem.Order.ShouldBe(10);
        tocItem.Section.ShouldBe("Documentation");
        tocItem.Locale.ShouldBe("en");
    }

    [Fact]
    public void ContentTocItem_HierarchyPartsPreservedCorrectly()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/a/b/c"),
            OutputFile = new FilePath("a/b/c/index.html")
        };

        var parts = new[] { "root", "section", "page" };
        var tocItem = new ContentTocItem("Page", route, 1, parts, null, null);

        tocItem.HierarchyParts.Length.ShouldBe(3);
        tocItem.HierarchyParts[0].ShouldBe("root");
        tocItem.HierarchyParts[1].ShouldBe("section");
        tocItem.HierarchyParts[2].ShouldBe("page");
    }

    [Fact]
    public void ContentTocItem_NullableSectionAndLocale()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/page"),
            OutputFile = new FilePath("page/index.html")
        };

        var tocItem = new ContentTocItem("Page", route, 0, [], null, null);

        tocItem.Section.ShouldBeNull();
        tocItem.Locale.ShouldBeNull();
    }
}
