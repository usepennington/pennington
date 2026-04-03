using Microsoft.Extensions.DependencyInjection;
using Penn.Content;
using Penn.FrontMatter;
using Penn.Highlighting;
using Penn.Infrastructure;
using Penn.Markdown;
using Penn.Navigation;
using Penn.Pipeline;

namespace Penn.Tests.Infrastructure;

public class PennExtensionsTests
{
    private static ServiceProvider BuildProvider(Action<PennOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddPenn(configure ?? (opts =>
        {
            opts.SiteTitle = "Test Site";
        }));
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddPenn_Registers_PennOptions()
    {
        using var provider = BuildProvider();

        var options = provider.GetService<PennOptions>();

        options.ShouldNotBeNull();
        options.SiteTitle.ShouldBe("Test Site");
    }

    [Fact]
    public void AddPenn_Registers_FrontMatterParser()
    {
        using var provider = BuildProvider();

        var parser = provider.GetService<FrontMatterParser>();

        parser.ShouldNotBeNull();
    }

    [Fact]
    public void AddPenn_Registers_IContentService_ForConfiguredSource()
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
    public void AddPenn_Registers_IContentParser()
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
    public void AddPenn_Registers_IContentRenderer()
    {
        using var provider = BuildProvider();

        var renderer = provider.GetService<IContentRenderer>();

        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<MarkdownContentRenderer>();
    }

    [Fact]
    public void AddPenn_Registers_NavigationBuilder()
    {
        using var provider = BuildProvider();

        var navBuilder = provider.GetService<NavigationBuilder>();

        navBuilder.ShouldNotBeNull();
    }
}
