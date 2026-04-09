using Microsoft.Extensions.DependencyInjection;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Highlighting;
using Pennington.Infrastructure;
using Pennington.Islands;
using Pennington.Markdown;
using Pennington.Navigation;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Infrastructure;

public class PenningtonExtensionsTests
{
    private static ServiceProvider BuildProvider(Action<PenningtonOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddPennington(configure ?? (opts =>
        {
            opts.SiteTitle = "Test Site";
        }));
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddPennington_Registers_PenningtonOptions()
    {
        using var provider = BuildProvider();

        var options = provider.GetService<PenningtonOptions>();

        options.ShouldNotBeNull();
        options.SiteTitle.ShouldBe("Test Site");
    }

    [Fact]
    public void AddPennington_Registers_FrontMatterParser()
    {
        using var provider = BuildProvider();

        var parser = provider.GetService<FrontMatterParser>();

        parser.ShouldNotBeNull();
    }

    [Fact]
    public void AddPennington_Registers_IContentService_ForConfiguredSource()
    {
        using var provider = BuildProvider(opts =>
        {
            opts.AddMarkdownContent<DocFrontMatter>(o =>
            {
                o.ContentPath = "test-content";
                o.BasePageUrl = "/docs";
            });
        });

        var services = provider.GetServices<IContentService>().ToList();

        services.Count.ShouldBe(1);
    }

    [Fact]
    public void AddPennington_Registers_IContentParser()
    {
        using var provider = BuildProvider(opts =>
        {
            opts.AddMarkdownContent<DocFrontMatter>(o =>
            {
                o.ContentPath = "test-content";
            });
        });

        var parsers = provider.GetServices<IContentParser>().ToList();

        parsers.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void AddPennington_Registers_IContentRenderer()
    {
        using var provider = BuildProvider();

        var renderer = provider.GetService<IContentRenderer>();

        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<MarkdownContentRenderer>();
    }

    [Fact]
    public void AddPennington_Registers_NavigationBuilder()
    {
        using var provider = BuildProvider();

        var navBuilder = provider.GetService<NavigationBuilder>();

        navBuilder.ShouldNotBeNull();
    }

    [Fact]
    public void AddPennington_Registers_IslandRenderers_FromOptions()
    {
        using var provider = BuildProvider(opts =>
        {
            opts.Islands.Register<StubIsland>("test-island");
        });

        var islands = provider.GetServices<IIslandRenderer>().ToList();

        islands.Count.ShouldBe(1);
        islands[0].ShouldBeOfType<StubIsland>();
    }

    [Fact]
    public void AddPennington_Registers_CodeHighlighters_FromOptions()
    {
        using var provider = BuildProvider(opts =>
        {
            opts.Highlighting.AddHighlighter<StubHighlighter>();
        });

        var highlighters = provider.GetServices<ICodeHighlighter>().ToList();

        highlighters.OfType<StubHighlighter>().Count().ShouldBe(1);
    }

    private class StubIsland : IIslandRenderer
    {
        public string IslandName => "test";
        public Task<string> RenderAsync(ContentRoute route, RenderContext context)
            => Task.FromResult("<div>test</div>");
    }

    private class StubHighlighter : ICodeHighlighter
    {
        public IReadOnlySet<string> SupportedLanguages { get; } = new HashSet<string> { "stub" };
        public string Highlight(string code, string language) => code;
        public int Priority => 100;
    }
}
