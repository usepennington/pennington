using Penn.Islands;
using Penn.Routing;

namespace Penn.Tests.Islands;

public class SpaPageDataServiceTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    private static RenderContext MakeContext() => new(
        BaseUrl: new UrlPath("/"),
        SiteTitle: "Test Site",
        Locale: null
    );

    [Fact]
    public async Task GetPageDataAsync_WithRenderer_ReturnsEnvelopeWithIslandHtml()
    {
        var renderer = new StubIslandRenderer("content", "<article>Hello</article>");
        var service = new SpaPageDataService([renderer], MakeContext());

        var result = await service.GetPageDataAsync(MakeRoute(), "Test Page", "A description");

        result.ShouldNotBeNull();
        result.Title.ShouldBe("Test Page");
        result.Description.ShouldBe("A description");
        result.Islands.ShouldContainKey("content");
        result.Islands["content"].ShouldBe("<article>Hello</article>");
    }

    [Fact]
    public async Task GetPageDataAsync_NoRenderers_ReturnsNull()
    {
        var service = new SpaPageDataService([], MakeContext());

        var result = await service.GetPageDataAsync(MakeRoute(), "Empty Page");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetPageDataAsync_RendererReturnsEmpty_SkipsIsland()
    {
        var renderer = new StubIslandRenderer("sidebar", "");
        var service = new SpaPageDataService([renderer], MakeContext());

        var result = await service.GetPageDataAsync(MakeRoute(), "Page");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetPageDataAsync_MultipleRenderers_ProducesMultipleIslands()
    {
        var renderers = new IIslandRenderer[]
        {
            new StubIslandRenderer("content", "<article>Main</article>"),
            new StubIslandRenderer("toc", "<nav>TOC</nav>")
        };
        var service = new SpaPageDataService(renderers, MakeContext());

        var result = await service.GetPageDataAsync(MakeRoute(), "Full Page");

        result.ShouldNotBeNull();
        result.Islands.Count.ShouldBe(2);
        result.Islands["content"].ShouldBe("<article>Main</article>");
        result.Islands["toc"].ShouldBe("<nav>TOC</nav>");
    }

    private class StubIslandRenderer(string name, string html) : IIslandRenderer
    {
        public string IslandName => name;
        public Task<string> RenderAsync(ContentRoute route, RenderContext context)
            => Task.FromResult(html);
    }
}
