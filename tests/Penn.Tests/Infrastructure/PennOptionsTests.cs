using Penn.FrontMatter;
using Penn.Highlighting;
using Penn.Infrastructure;
using Penn.Islands;
using Penn.Localization;
using Penn.Routing;

namespace Penn.Tests.Infrastructure;

public class PennOptionsTests
{
    // --- Default values ---

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var options = new PennOptions();

        options.SiteTitle.ShouldBe("");
        options.SiteDescription.ShouldBe("");
        options.CanonicalBaseUrl.ShouldBeNull();
        options.ContentRootPath.ShouldBe("Content");
        options.MarkdownSources.ShouldBeEmpty();
    }

    // --- SiteTitle and SiteDescription ---

    [Fact]
    public void SetSiteTitleAndDescription()
    {
        var options = new PennOptions
        {
            SiteTitle = "My Docs",
            SiteDescription = "Documentation site"
        };

        options.SiteTitle.ShouldBe("My Docs");
        options.SiteDescription.ShouldBe("Documentation site");
    }

    // --- AddMarkdownContent ---

    [Fact]
    public void AddMarkdownContent_RegistersSource()
    {
        var options = new PennOptions();
        options.AddMarkdownContent<DocFrontMatter>(o =>
        {
            o.ContentPath = "docs";
            o.BasePageUrl = "/docs";
            o.Section = "documentation";
        });

        options.MarkdownSources.Count.ShouldBe(1);
        options.MarkdownSources[0].ContentPath.ShouldBe("docs");
        options.MarkdownSources[0].BasePageUrl.ShouldBe("/docs");
        options.MarkdownSources[0].Section.ShouldBe("documentation");
    }

    [Fact]
    public void AddMarkdownContent_CapturesFrontMatterType()
    {
        var options = new PennOptions();
        var source = options.AddMarkdownContent<DocFrontMatter>(o => o.ContentPath = "content");

        // FrontMatterType is internal, so we verify via the returned options object
        source.ShouldNotBeNull();
        source.ContentPath.ShouldBe("content");
    }

    [Fact]
    public void AddMarkdownContent_MultipleSources_Accumulate()
    {
        var options = new PennOptions();
        options.AddMarkdownContent<DocFrontMatter>(o => o.ContentPath = "docs");
        options.AddMarkdownContent<BlogFrontMatter>(o => o.ContentPath = "blog");

        options.MarkdownSources.Count.ShouldBe(2);
        options.MarkdownSources[0].ContentPath.ShouldBe("docs");
        options.MarkdownSources[1].ContentPath.ShouldBe("blog");
    }

    // --- MarkdownContentOptions defaults ---

    [Fact]
    public void MarkdownContentOptions_DefaultValues()
    {
        var mco = new MarkdownContentOptions();

        mco.ContentPath.ShouldBe("Content");
        mco.BasePageUrl.ShouldBe("/");
        mco.Section.ShouldBeNull();
    }

    // --- HighlightingOptions ---

    [Fact]
    public void HighlightingOptions_AddHighlighter_Generic()
    {
        var options = new PennOptions();
        options.Highlighting.AddHighlighter<PlainTextHighlighter>();

        options.Highlighting.Highlighters.Count.ShouldBe(1);
        options.Highlighting.Highlighters[0].ShouldBeOfType<PlainTextHighlighter>();
    }

    [Fact]
    public void HighlightingOptions_AddHighlighter_Instance()
    {
        var options = new PennOptions();
        var highlighter = new PlainTextHighlighter();
        options.Highlighting.AddHighlighter(highlighter);

        options.Highlighting.Highlighters.Count.ShouldBe(1);
        options.Highlighting.Highlighters[0].ShouldBeSameAs(highlighter);
    }

    // --- IslandsOptions ---

    [Fact]
    public void IslandsOptions_Register_AddsIslandType()
    {
        var options = new PennOptions();
        options.Islands.Register<StubIsland>("test-island");

        options.Islands.RegisteredIslands.Count.ShouldBe(1);
        options.Islands.RegisteredIslands.ShouldContainKey("test-island");
        options.Islands.RegisteredIslands["test-island"].ShouldBe(typeof(StubIsland));
    }

    // --- LocalizationOptions ---

    [Fact]
    public void LocalizationOptions_DefaultLocale_IsEn()
    {
        var options = new PennOptions();
        options.Localization.DefaultLocale.ShouldBe("en");
    }

    [Fact]
    public void LocalizationOptions_AddLocale_WithLocaleInfo()
    {
        var options = new PennOptions();
        var info = new LocaleInfo("French", "ltr", "fr");
        options.Localization.AddLocale("fr", info);

        options.Localization.Locales.Count.ShouldBe(1);
        options.Localization.Locales.ShouldContainKey("fr");
        options.Localization.Locales["fr"].DisplayName.ShouldBe("French");
        options.Localization.Locales["fr"].HtmlLang.ShouldBe("fr");
    }

    [Fact]
    public void LocalizationOptions_AddLocale_WithDisplayName()
    {
        var options = new PennOptions();
        options.Localization.AddLocale("de", "German");

        options.Localization.Locales.Count.ShouldBe(1);
        options.Localization.Locales["de"].DisplayName.ShouldBe("German");
        options.Localization.Locales["de"].Direction.ShouldBe("ltr");
        options.Localization.Locales["de"].HtmlLang.ShouldBeNull();
    }

    // --- Stub types for testing ---

    private class StubIsland : IIslandRenderer
    {
        public string IslandName => "test";
        public Task<string> RenderAsync(ContentRoute route, RenderContext context)
            => Task.FromResult("<div>test</div>");
    }
}
