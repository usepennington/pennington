using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;
using Pennington.Taxonomy;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Taxonomy;

public class TaxonomyContentServiceTests
{
    public sealed class IndexComponent : ComponentBase { }
    public sealed class TermComponent : ComponentBase { }

    public record RecipeFrontMatter : IFrontMatter, ITaggable
    {
        public string Title { get; init; } = "";
        public string? Description { get; init; }
        public bool IsDraft { get; init; }
        public string[] Tags { get; init; } = [];
        public string? Uid { get; init; }
        public bool Search { get; init; } = true;
        public bool Llms { get; init; } = true;
        public bool SearchOnly { get; init; }

        public string Cuisine { get; init; } = "";
    }

    /// <summary>Yields DiscoveredItems pointing at MockFileSystem files for taxonomy to parse.</summary>
    private sealed class FakeMarkdownContentService : IContentService
    {
        private readonly List<(string url, string filePath)> _items;

        public FakeMarkdownContentService(IEnumerable<(string url, string filePath)> items) =>
            _items = items.ToList();

        public string DefaultSectionLabel => "fake";
        public int SearchPriority => 1;

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            foreach (var (url, filePath) in _items)
            {
                yield return new DiscoveredItem(
                    ContentRouteFactory.FromUrl(new UrlPath(url)),
                    new MarkdownFileSource(new FilePath(filePath)));
            }
            await Task.CompletedTask;
        }

        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync()
            => Task.FromResult(ImmutableList<ContentToCopy>.Empty);

        public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync()
            => Task.FromResult(ImmutableList<ContentToCreate>.Empty);

        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync()
            => Task.FromResult(ImmutableList<ContentTocItem>.Empty);

        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync()
            => Task.FromResult(ImmutableList<CrossReference>.Empty);
    }

    private static (TaxonomyContentService<RecipeFrontMatter, string> service, MockFileSystem fs)
        BuildService(
            Action<TaxonomyOptions<RecipeFrontMatter, string>> configure,
            params (string url, string path, string body)[] files)
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/content");

        var fakeItems = new List<(string url, string filePath)>();
        foreach (var (url, path, body) in files)
        {
            var full = $"/content/{path}";
            var dir = fs.Path.GetDirectoryName(full);
            if (!string.IsNullOrEmpty(dir)) fs.Directory.CreateDirectory(dir);
            fs.File.WriteAllText(full, body);
            fakeItems.Add((url, full));
        }

        var fake = new FakeMarkdownContentService(fakeItems);

        var options = new TaxonomyOptions<RecipeFrontMatter, string>
        {
            BaseUrl = "/cuisine",
            IndexPage = typeof(IndexComponent),
            TermPage = typeof(TermComponent),
        };
        configure(options);
        options.Validate();

        var services = new ServiceCollection();
        services.AddSingleton<IContentService>(fake);
        var sp = services.BuildServiceProvider();

        var taxonomy = new TaxonomyContentService<RecipeFrontMatter, string>(
            options,
            sp,
            new FrontMatterParser(),
            fs,
            new FileWatcher(fs));

        return (taxonomy, fs);
    }

    [Fact]
    public async Task SingleValuedKey_GroupsItems_ByCuisine()
    {
        var (taxonomy, _) = BuildService(opts => opts.SelectKey = fm => fm.Cuisine,
            ("/recipes/carbonara/", "carbonara.md", "---\ntitle: Carbonara\ncuisine: italian\n---\n"),
            ("/recipes/pizza/", "pizza.md", "---\ntitle: Pizza\ncuisine: italian\n---\n"),
            ("/recipes/sushi/", "sushi.md", "---\ntitle: Sushi\ncuisine: japanese\n---\n"));

        var terms = await taxonomy.GetTermsAsync();

        terms.Count.ShouldBe(2);
        var italian = terms.Single(t => t.Key == "italian");
        italian.Items.Count.ShouldBe(2);
        italian.Items.Select(i => i.FrontMatter.Title).ShouldBe(["Carbonara", "Pizza"], ignoreOrder: true);

        var japanese = terms.Single(t => t.Key == "japanese");
        japanese.Items.Single().FrontMatter.Title.ShouldBe("Sushi");
    }

    [Fact]
    public async Task MultiValuedKey_PutsItem_InEveryMatchingTerm()
    {
        var (taxonomy, _) = BuildService(opts => opts.SelectKeys = fm => fm.Tags,
            ("/recipes/pasta/", "pasta.md", "---\ntitle: Pasta\ntags: [italian, vegetarian]\n---\n"),
            ("/recipes/salad/", "salad.md", "---\ntitle: Salad\ntags: [vegetarian, healthy]\n---\n"));

        var terms = await taxonomy.GetTermsAsync();

        terms.Select(t => t.Key).ShouldBe(["italian", "vegetarian", "healthy"], ignoreOrder: true);

        var vegetarian = terms.Single(t => t.Key == "vegetarian");
        vegetarian.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task DiscoverAsync_EmitsIndexAndOnePerTerm()
    {
        var (taxonomy, _) = BuildService(opts => opts.SelectKey = fm => fm.Cuisine,
            ("/recipes/a/", "a.md", "---\ntitle: A\ncuisine: italian\n---\n"),
            ("/recipes/b/", "b.md", "---\ntitle: B\ncuisine: french\n---\n"));

        var items = new List<DiscoveredItem>();
        await foreach (var item in taxonomy.DiscoverAsync()) items.Add(item);

        items.Count.ShouldBe(3); // index + italian + french
        items[0].Route.CanonicalPath.Value.TrimEnd('/').ShouldBe("/cuisine");
        items.All(i => i.Source.Value is EndpointSource).ShouldBeTrue();
    }

    [Fact]
    public async Task GetContentTocEntries_EmitsSectionHeaderAndTermEntries()
    {
        var (taxonomy, _) = BuildService(opts => opts.SelectKey = fm => fm.Cuisine,
            ("/recipes/a/", "a.md", "---\ntitle: A\ncuisine: italian\n---\n"));

        var entries = await taxonomy.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(2);
        entries[0].Title.ShouldBe("Cuisine");
        entries[0].SectionLabel.ShouldBe("Cuisine");
        entries[1].Title.ShouldBe("italian");
    }

    [Fact]
    public async Task GetCrossReferences_EmitsOnePerTerm_WhenEnabled()
    {
        var (taxonomy, _) = BuildService(opts =>
            {
                opts.SelectKey = fm => fm.Cuisine;
                opts.EmitCrossReferences = true;
            },
            ("/recipes/a/", "a.md", "---\ntitle: A\ncuisine: italian\n---\n"));

        var refs = await taxonomy.GetCrossReferencesAsync();

        refs.Single().Uid.ShouldBe("cuisine-italian");
        refs.Single().Title.ShouldBe("italian");
    }

    [Fact]
    public async Task GetCrossReferences_EmpyList_WhenDisabled()
    {
        var (taxonomy, _) = BuildService(opts =>
            {
                opts.SelectKey = fm => fm.Cuisine;
                opts.EmitCrossReferences = false;
            },
            ("/recipes/a/", "a.md", "---\ntitle: A\ncuisine: italian\n---\n"));

        (await taxonomy.GetCrossReferencesAsync()).Count.ShouldBe(0);
    }

    [Fact]
    public async Task Discover_SkipsDrafts()
    {
        var (taxonomy, _) = BuildService(opts => opts.SelectKey = fm => fm.Cuisine,
            ("/recipes/a/", "a.md", "---\ntitle: A\ncuisine: italian\nisDraft: true\n---\n"),
            ("/recipes/b/", "b.md", "---\ntitle: B\ncuisine: italian\n---\n"));

        var terms = await taxonomy.GetTermsAsync();

        terms.Single().Items.Count.ShouldBe(1);
        terms.Single().Items.Single().FrontMatter.Title.ShouldBe("B");
    }

    [Fact]
    public async Task Discover_SkipsItemsWithEmptyKey()
    {
        var (taxonomy, _) = BuildService(opts => opts.SelectKey = fm => fm.Cuisine,
            ("/recipes/a/", "a.md", "---\ntitle: A\ncuisine: ''\n---\n"),
            ("/recipes/b/", "b.md", "---\ntitle: B\ncuisine: italian\n---\n"));

        var terms = await taxonomy.GetTermsAsync();

        terms.Single().Items.Single().FrontMatter.Title.ShouldBe("B");
    }

    [Fact]
    public async Task TryGetTermAsync_ReturnsTermBySlug()
    {
        var (taxonomy, _) = BuildService(opts => opts.SelectKey = fm => fm.Cuisine,
            ("/recipes/a/", "a.md", "---\ntitle: A\ncuisine: 'Northern Italian'\n---\n"));

        var term = await taxonomy.TryGetTermAsync("northern-italian");

        term.ShouldNotBeNull();
        term!.Label.ShouldBe("Northern Italian");
        term.Items.Single().FrontMatter.Title.ShouldBe("A");
    }

    [Fact]
    public async Task TryGetTermAsync_ReturnsNull_WhenSlugMissing()
    {
        var (taxonomy, _) = BuildService(opts => opts.SelectKey = fm => fm.Cuisine,
            ("/recipes/a/", "a.md", "---\ntitle: A\ncuisine: italian\n---\n"));

        (await taxonomy.TryGetTermAsync("french")).ShouldBeNull();
    }

    [Fact]
    public async Task LabelFor_OverridesDefault()
    {
        var (taxonomy, _) = BuildService(opts =>
            {
                opts.SelectKey = fm => fm.Cuisine;
                opts.LabelFor = key => key.ToUpperInvariant();
            },
            ("/recipes/a/", "a.md", "---\ntitle: A\ncuisine: italian\n---\n"));

        var terms = await taxonomy.GetTermsAsync();
        terms.Single().Label.ShouldBe("ITALIAN");
    }
}
