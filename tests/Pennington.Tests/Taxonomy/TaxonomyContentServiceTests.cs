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

    /// <summary>
    /// A content service that projects records the way the real <see cref="MarkdownContentService{T}"/>
    /// does — attaching parsed front matter as <see cref="DiscoveredItem.Metadata"/> so the default
    /// <see cref="IContentService.GetRecordsAsync"/> bridge surfaces them. Taxonomy reads records,
    /// not files, so no file system is involved.
    /// </summary>
    private sealed class FakeRecordContentService : IContentService
    {
        private readonly List<(string url, RecipeFrontMatter fm)> _items;

        public FakeRecordContentService(IEnumerable<(string url, RecipeFrontMatter fm)> items) =>
            _items = items.ToList();

        public string DefaultSectionLabel => "fake";
        public int SearchPriority => 1;

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            foreach (var (url, fm) in _items)
            {
                yield return new DiscoveredItem(
                    ContentRouteFactory.FromUrl(new UrlPath(url)),
                    new MarkdownFileSource(new FilePath($"/content{url}index.md")))
                {
                    Metadata = fm,
                };
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

    private static TaxonomyContentService<RecipeFrontMatter, string> BuildService(
        Action<TaxonomyOptions<RecipeFrontMatter, string>> configure,
        params (string url, RecipeFrontMatter fm)[] records)
    {
        var fake = new FakeRecordContentService(records);

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

        return new TaxonomyContentService<RecipeFrontMatter, string>(
            options,
            sp,
            new FileWatcher(new MockFileSystem()));
    }

    [Fact]
    public async Task SingleValuedKey_GroupsItems_ByCuisine()
    {
        var taxonomy = BuildService(opts => opts.SelectKey = fm => fm.Cuisine,
            ("/recipes/carbonara/", new RecipeFrontMatter { Title = "Carbonara", Cuisine = "italian" }),
            ("/recipes/pizza/", new RecipeFrontMatter { Title = "Pizza", Cuisine = "italian" }),
            ("/recipes/sushi/", new RecipeFrontMatter { Title = "Sushi", Cuisine = "japanese" }));

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
        var taxonomy = BuildService(opts => opts.SelectKeys = fm => fm.Tags,
            ("/recipes/pasta/", new RecipeFrontMatter { Title = "Pasta", Tags = ["italian", "vegetarian"] }),
            ("/recipes/salad/", new RecipeFrontMatter { Title = "Salad", Tags = ["vegetarian", "healthy"] }));

        var terms = await taxonomy.GetTermsAsync();

        terms.Select(t => t.Key).ShouldBe(["italian", "vegetarian", "healthy"], ignoreOrder: true);

        var vegetarian = terms.Single(t => t.Key == "vegetarian");
        vegetarian.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task DiscoverAsync_EmitsIndexAndOnePerTerm()
    {
        var taxonomy = BuildService(opts => opts.SelectKey = fm => fm.Cuisine,
            ("/recipes/a/", new RecipeFrontMatter { Title = "A", Cuisine = "italian" }),
            ("/recipes/b/", new RecipeFrontMatter { Title = "B", Cuisine = "french" }));

        var items = new List<DiscoveredItem>();
        await foreach (var item in taxonomy.DiscoverAsync())
        {
            items.Add(item);
        }

        items.Count.ShouldBe(3); // index + italian + french
        items[0].Route.CanonicalPath.Value.TrimEnd('/').ShouldBe("/cuisine");
        items.All(i => i.Source.Value is EndpointSource).ShouldBeTrue();
    }

    [Fact]
    public async Task GetRecordsAsync_IsEmpty_SoSiblingTaxonomiesDoNotRecurse()
    {
        var taxonomy = BuildService(opts => opts.SelectKey = fm => fm.Cuisine,
            ("/recipes/a/", new RecipeFrontMatter { Title = "A", Cuisine = "italian" }));

        var records = new List<ContentRecord>();
        await foreach (var record in taxonomy.GetRecordsAsync())
        {
            records.Add(record);
        }

        records.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetContentTocEntries_EmitsSectionHeaderAndTermEntries()
    {
        var taxonomy = BuildService(opts => opts.SelectKey = fm => fm.Cuisine,
            ("/recipes/a/", new RecipeFrontMatter { Title = "A", Cuisine = "italian" }));

        var entries = await taxonomy.GetContentTocEntriesAsync();

        entries.Count.ShouldBe(2);
        entries[0].Title.ShouldBe("Cuisine");
        entries[0].SectionLabel.ShouldBe("Cuisine");
        entries[1].Title.ShouldBe("italian");
    }

    [Fact]
    public async Task GetCrossReferences_EmitsOnePerTerm_WhenEnabled()
    {
        var taxonomy = BuildService(opts =>
            {
                opts.SelectKey = fm => fm.Cuisine;
                opts.EmitCrossReferences = true;
            },
            ("/recipes/a/", new RecipeFrontMatter { Title = "A", Cuisine = "italian" }));

        var refs = await taxonomy.GetCrossReferencesAsync();

        refs.Single().Uid.ShouldBe("cuisine-italian");
        refs.Single().Title.ShouldBe("italian");
    }

    [Fact]
    public async Task GetCrossReferences_EmpyList_WhenDisabled()
    {
        var taxonomy = BuildService(opts =>
            {
                opts.SelectKey = fm => fm.Cuisine;
                opts.EmitCrossReferences = false;
            },
            ("/recipes/a/", new RecipeFrontMatter { Title = "A", Cuisine = "italian" }));

        (await taxonomy.GetCrossReferencesAsync()).Count.ShouldBe(0);
    }

    [Fact]
    public async Task Discover_SkipsDrafts()
    {
        var taxonomy = BuildService(opts => opts.SelectKey = fm => fm.Cuisine,
            ("/recipes/a/", new RecipeFrontMatter { Title = "A", Cuisine = "italian", IsDraft = true }),
            ("/recipes/b/", new RecipeFrontMatter { Title = "B", Cuisine = "italian" }));

        var terms = await taxonomy.GetTermsAsync();

        terms.Single().Items.Count.ShouldBe(1);
        terms.Single().Items.Single().FrontMatter.Title.ShouldBe("B");
    }

    [Fact]
    public async Task Discover_SkipsItemsWithEmptyKey()
    {
        var taxonomy = BuildService(opts => opts.SelectKey = fm => fm.Cuisine,
            ("/recipes/a/", new RecipeFrontMatter { Title = "A", Cuisine = "" }),
            ("/recipes/b/", new RecipeFrontMatter { Title = "B", Cuisine = "italian" }));

        var terms = await taxonomy.GetTermsAsync();

        terms.Single().Items.Single().FrontMatter.Title.ShouldBe("B");
    }

    [Fact]
    public async Task TryGetTermAsync_ReturnsTermBySlug()
    {
        var taxonomy = BuildService(opts => opts.SelectKey = fm => fm.Cuisine,
            ("/recipes/a/", new RecipeFrontMatter { Title = "A", Cuisine = "Northern Italian" }));

        var term = await taxonomy.TryGetTermAsync("northern-italian");

        term.ShouldNotBeNull();
        term!.Label.ShouldBe("Northern Italian");
        term.Items.Single().FrontMatter.Title.ShouldBe("A");
    }

    [Fact]
    public async Task TryGetTermAsync_ReturnsNull_WhenSlugMissing()
    {
        var taxonomy = BuildService(opts => opts.SelectKey = fm => fm.Cuisine,
            ("/recipes/a/", new RecipeFrontMatter { Title = "A", Cuisine = "italian" }));

        (await taxonomy.TryGetTermAsync("french")).ShouldBeNull();
    }

    [Fact]
    public async Task LabelFor_OverridesDefault()
    {
        var taxonomy = BuildService(opts =>
            {
                opts.SelectKey = fm => fm.Cuisine;
                opts.LabelFor = key => key.ToUpperInvariant();
            },
            ("/recipes/a/", new RecipeFrontMatter { Title = "A", Cuisine = "italian" }));

        var terms = await taxonomy.GetTermsAsync();
        terms.Single().Label.ShouldBe("ITALIAN");
    }
}