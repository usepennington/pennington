using System.Collections.Immutable;
using Penn.Islands;
using Penn.Pipeline;
using Penn.Routing;

namespace Penn.Tests.Islands;

public class IslandTests
{
    private static ContentRoute MakeRoute(string path = "/test") => new()
    {
        CanonicalPath = new UrlPath(path),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html")
    };

    [Fact]
    public void RenderContext_Create_VerifiesProperties()
    {
        var context = new RenderContext(
            BaseUrl: new UrlPath("/docs"),
            SiteTitle: "My Site",
            Locale: "en-US");

        context.BaseUrl.Value.ShouldBe("/docs");
        context.SiteTitle.ShouldBe("My Site");
        context.Locale.ShouldBe("en-US");
    }

    [Fact]
    public void RenderContext_NullLocale()
    {
        var context = new RenderContext(
            BaseUrl: new UrlPath("/"),
            SiteTitle: "Test",
            Locale: null);

        context.Locale.ShouldBeNull();
    }

    [Fact]
    public void SpaEnvelope_CreateWithIslands_VerifiesAllProperties()
    {
        var social = new SocialMetadata("A description", "/img/og.png", "article", null, "Author");
        var islands = ImmutableDictionary.CreateRange(new[]
        {
            KeyValuePair.Create("search", "<div id=\"search\">...</div>"),
            KeyValuePair.Create("toc", "<nav>...</nav>")
        });

        var envelope = new SpaEnvelope(
            Title: "My Page",
            Description: "Page description",
            Social: social,
            Islands: islands);

        envelope.Title.ShouldBe("My Page");
        envelope.Description.ShouldBe("Page description");
        envelope.Social.ShouldNotBeNull();
        envelope.Social.Author.ShouldBe("Author");
        envelope.Islands.Count.ShouldBe(2);
        envelope.Islands["search"].ShouldBe("<div id=\"search\">...</div>");
        envelope.Islands["toc"].ShouldBe("<nav>...</nav>");
    }

    [Fact]
    public void SpaEnvelope_CreateWithEmptyIslands()
    {
        var envelope = new SpaEnvelope(
            Title: "Empty",
            Description: null,
            Social: null,
            Islands: ImmutableDictionary<string, string>.Empty);

        envelope.Title.ShouldBe("Empty");
        envelope.Description.ShouldBeNull();
        envelope.Social.ShouldBeNull();
        envelope.Islands.ShouldBeEmpty();
    }

    [Fact]
    public async Task IIslandRenderer_StubRenderer_VerifiesInterfaceContract()
    {
        IIslandRenderer renderer = new StubIslandRenderer();
        var route = MakeRoute("/page");
        var context = new RenderContext(new UrlPath("/"), "Site", null);

        renderer.IslandName.ShouldBe("counter");

        var html = await renderer.RenderAsync(route, context);

        html.ShouldBe("<div data-island=\"counter\">0</div>");
    }

    private sealed class StubIslandRenderer : IIslandRenderer
    {
        public string IslandName => "counter";

        public Task<string> RenderAsync(ContentRoute route, RenderContext context)
            => Task.FromResult("<div data-island=\"counter\">0</div>");
    }
}
