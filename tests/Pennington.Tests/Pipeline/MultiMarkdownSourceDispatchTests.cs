using Microsoft.Extensions.DependencyInjection;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Pipeline;

/// <summary>
/// Regression coverage for the multi-source dispatch bug: two <c>AddMarkdownContent&lt;T&gt;</c>
/// sources with different front-matter types must each parse with their own type. Before the
/// per-source format key, every source registered its parser under the constant <c>"markdown"</c>
/// key and the last registration clobbered the rest, so the <see cref="DispatchingContentParser"/>
/// parsed every page with the last source's type — dropping fields (serve) or throwing (build).
/// </summary>
public class MultiMarkdownSourceDispatchTests
{
    private sealed record BlogTestFrontMatter : IFrontMatter
    {
        public string Title { get; init; } = "";

        /// <summary>A field the doc front-matter type does not declare — its survival proves the right parser ran.</summary>
        public string? Category { get; init; }
    }

    private static ServiceProvider BuildProvider(MockFileSystem fs)
    {
        var services = new ServiceCollection();
        // Register the mock filesystem before AddPennington so its TryAdd respects ours.
        services.AddSingleton<System.IO.Abstractions.IFileSystem>(fs);
        services.AddLogging();

        services.AddPennington(penn =>
        {
            penn.SiteTitle = "Multi Source";

            // Registered first: docs as DocFrontMatter (has Order).
            penn.AddMarkdownContent<DocFrontMatter>(md =>
            {
                md.ContentPath = "/docs";
                md.BasePageUrl = "/docs";
            });

            // Registered last: blog as BlogTestFrontMatter. Pre-fix this parser would win the
            // single "markdown" key and parse the docs pages too.
            penn.AddMarkdownContent<BlogTestFrontMatter>(md =>
            {
                md.ContentPath = "/blog";
                md.BasePageUrl = "/blog";
            });
        });

        return services.BuildServiceProvider();
    }

    private static async Task<ParsedItem> ParseRouteAsync(ServiceProvider provider, string urlContains)
    {
        var services = provider.GetServices<IContentService>();
        var parser = provider.GetRequiredService<IContentParser>();

        await foreach (var item in services.DiscoverAllAsync())
        {
            if (!item.Route.CanonicalPath.Value.Contains(urlContains, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var result = await parser.ParseAsync(item);
            result.Value.ShouldBeOfType<ParsedItem>();
            return (ParsedItem)result.Value!;
        }

        throw new Xunit.Sdk.XunitException($"No discovered item matched '{urlContains}'.");
    }

    [Fact]
    public async Task DispatchingParser_RoutesEachSourceToItsOwnFrontMatterType()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/docs");
        fs.Directory.CreateDirectory("/blog");
        fs.File.WriteAllText("/docs/intro.md", "---\ntitle: Intro\norder: 1\n---\n# Intro");
        fs.File.WriteAllText("/blog/welcome.md", "---\ntitle: Welcome\ncategory: news\n---\n# Welcome");

        await using var provider = BuildProvider(fs);

        // The docs page must parse as DocFrontMatter with its order intact — not as the
        // last-registered BlogTestFrontMatter (which has no Order and would drop it).
        var doc = await ParseRouteAsync(provider, "/docs/intro");
        var docFm = doc.Metadata.ShouldBeOfType<DocFrontMatter>();
        docFm.Order.ShouldBe(1);

        // The blog page must parse as BlogTestFrontMatter with its own custom field intact.
        var blog = await ParseRouteAsync(provider, "/blog/welcome");
        var blogFm = blog.Metadata.ShouldBeOfType<BlogTestFrontMatter>();
        blogFm.Category.ShouldBe("news");
    }
}
