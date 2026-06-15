using Pennington.Content;
using Pennington.FrontMatter;
using Pennington.Infrastructure;
using Pennington.Localization;
using Pennington.Routing;
using Pennington.Search;

namespace Pennington.Tests.Search;

public class SearchIndexBuilderTests
{
    private sealed record FacetedFrontMatter : IFrontMatter, IHasSearchFacets
    {
        public string Title { get; init; } = "";
        public IReadOnlyDictionary<string, string[]> SearchFacets { get; init; } =
            new Dictionary<string, string[]>();
    }

    private static ContentRoute MakeRoute(string path, string locale = "") => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html"),
        Locale = locale
    };

    private static ContentTocItem MakeToc(
        string title,
        string path,
        string? sectionLabel = null,
        string locale = "",
        string? description = null) =>
        new(
            Title: title,
            Route: MakeRoute(path, locale),
            Order: 0,
            HierarchyParts: [],
            SectionLabel: sectionLabel,
            Locale: string.IsNullOrEmpty(locale) ? null : locale
        )
        {
            Description = description,
        };

    private static HeadingSection Lead(string text = "intro text") => new(null, "", 1, [], text, IsLead: true);

    private static HeadingSection Heading(string anchor, string title, string text, int level = 2, string[]? crumbs = null) =>
        new(anchor, title, level, crumbs ?? [], text, IsLead: false);

    // Default facet set is area-only; localization is single-locale so the area is the first URL segment.
    private readonly SearchIndexBuilder _builder = new(new SearchIndexOptions(), new LocalizationOptions());

    [Fact]
    public void BuildSection_Lead_UsesPageUrlTitleAndDescription()
    {
        var toc = MakeToc("Introduction", "/docs/intro", description: "Overview.");

        var doc = _builder.BuildSection(toc, Lead("Welcome to the docs"));

        doc.Url.ShouldBe("/docs/intro/");           // page URL, no anchor
        doc.Title.ShouldBe("Introduction");          // page title
        doc.Description.ShouldBe("Overview.");        // page description on the lead only
        doc.Body.ShouldBe("Welcome to the docs");
        doc.Crumbs.ShouldBeEmpty();
        doc.Priority.ShouldBe(5);
    }

    [Fact]
    public void BuildSection_Heading_DeepLinksAndCarriesCrumbTrail()
    {
        var toc = MakeToc("Markdown Pipeline", "/reference/markdown");

        var doc = _builder.BuildSection(
            toc,
            Heading("extensions", "Extensions", "extension body", level: 3, crumbs: ["Configuration"]));

        doc.Url.ShouldBe("/reference/markdown/#extensions");      // deep-link anchor
        doc.Title.ShouldBe("Extensions");                          // heading text
        doc.Description.ShouldBeNull();                            // description is lead-only
        doc.Body.ShouldBe("extension body");
        doc.Crumbs.ShouldBe(["Markdown Pipeline", "Configuration"]); // page title + ancestor heading
        doc.Headings.ShouldBe("Markdown Pipeline Configuration");    // trail indexed at heading boost
    }

    [Fact]
    public void BuildSection_DefaultOptions_EmitAreaFacetOnly()
    {
        var toc = MakeToc("Auth", "/reference/auth", sectionLabel: "Reference");

        var doc = _builder.BuildSection(toc, Lead());

        doc.Facets.ShouldNotBeNull();
        doc.Facets!["area"].ShouldBe(["reference"]);
        doc.Facets.ShouldNotContainKey("section");
        doc.Facets.ShouldNotContainKey("tag");
    }

    [Fact]
    public void BuildSection_IncludesSectionFacet_WhenEnabled()
    {
        var builder = new SearchIndexBuilder(
            new SearchIndexOptions { Facets = SearchFacetField.Section },
            new LocalizationOptions());

        var doc = builder.BuildSection(MakeToc("Auth", "/api/auth", sectionLabel: "api"), Lead());

        doc.Facets.ShouldNotBeNull();
        doc.Facets!["section"].ShouldBe(["api"]);
    }

    [Fact]
    public void BuildSection_FacetsArePageLevel_SharedByLeadAndHeadings()
    {
        var toc = MakeToc("Page", "/guide/page");

        var lead = _builder.BuildSection(toc, Lead());
        var heading = _builder.BuildSection(toc, Heading("h", "H", "body"));

        lead.Facets!["area"].ShouldBe(["guide"]);
        heading.Facets!["area"].ShouldBe(["guide"]);
    }

    [Fact]
    public void BuildSection_EmitsCustomFacets_FromRecordMetadata()
    {
        // Custom facets emit even though the builder's enabled set is area-only — IHasSearchFacets
        // is the record author's explicit opt-in, not gated by SearchFacetField.
        var toc = MakeToc("Senior Engineer", "/jobs/senior-engineer");
        var metadata = new FacetedFrontMatter
        {
            Title = "Senior Engineer",
            SearchFacets = new Dictionary<string, string[]>
            {
                ["company"] = ["Acme"],
                ["language"] = ["en", "de"],
            },
        };

        var doc = _builder.BuildSection(toc, Lead(), metadata);

        doc.Facets.ShouldNotBeNull();
        doc.Facets!["area"].ShouldBe(["jobs"]);
        doc.Facets["company"].ShouldBe(["Acme"]);
        doc.Facets["language"].ShouldBe(["en", "de"]);
    }

    [Fact]
    public void BuildSection_CustomFacets_DoNotOverrideBuiltInDimensions()
    {
        var builder = new SearchIndexBuilder(
            new SearchIndexOptions { Facets = SearchFacetField.Tags },
            new LocalizationOptions());
        var toc = MakeToc("Post", "/blog/post") with { Tags = ["authoritative"] };
        var metadata = new FacetedFrontMatter
        {
            SearchFacets = new Dictionary<string, string[]> { ["tag"] = ["hijacked"] },
        };

        var doc = builder.BuildSection(toc, Lead(), metadata);

        // The built-in tag dimension stays authoritative; a colliding custom axis is dropped.
        doc.Facets!["tag"].ShouldBe(["authoritative"]);
    }

    [Fact]
    public void BuildSection_CustomFacets_NeverOccupyReservedNames_EvenWhenBuiltInDisabledOrEmpty()
    {
        // Default options enable Area only, so the built-in path never adds 'section' or 'tag'.
        // A custom axis named after a reserved dimension must still be dropped, and a custom 'area'
        // must not displace the real derived area.
        var toc = MakeToc("Item", "/catalog/item");
        var metadata = new FacetedFrontMatter
        {
            SearchFacets = new Dictionary<string, string[]>
            {
                ["section"] = ["sneaky"],
                ["tag"] = ["sneaky"],
                ["area"] = ["sneaky"],
                ["color"] = ["red"],
            },
        };

        var doc = _builder.BuildSection(toc, Lead(), metadata);

        doc.Facets.ShouldNotBeNull();
        doc.Facets!["area"].ShouldBe(["catalog"]); // the real derived area, not the custom one
        doc.Facets!.ShouldNotContainKey("section");
        doc.Facets!.ShouldNotContainKey("tag");
        doc.Facets!["color"].ShouldBe(["red"]);
    }

    [Fact]
    public void BuildSection_CustomFacets_SkipBlankAxesAndValues()
    {
        var toc = MakeToc("Item", "/catalog/item");
        var metadata = new FacetedFrontMatter
        {
            SearchFacets = new Dictionary<string, string[]>
            {
                ["  "] = ["ignored"],
                ["empty"] = [],
                ["region"] = ["  EU  ", "", "US"],
            },
        };

        var doc = _builder.BuildSection(toc, Lead(), metadata);

        doc.Facets.ShouldNotBeNull();
        doc.Facets!.ShouldNotContainKey("  ");
        doc.Facets!.ShouldNotContainKey("empty");
        doc.Facets!["region"].ShouldBe(["EU", "US"]);
    }

    [Fact]
    public void BuildSection_AppliesAreaPriority_WhenConfigured()
    {
        var options = new SearchIndexOptions
        {
            AreaPriorities = new(StringComparer.OrdinalIgnoreCase) { ["how-to"] = 10, ["reference"] = 6 },
        };
        var builder = new SearchIndexBuilder(options, new LocalizationOptions());

        builder.BuildSection(MakeToc("Auth", "/how-to/auth"), Lead()).Priority.ShouldBe(10);
        builder.BuildSection(MakeToc("Auth", "/reference/auth"), Lead()).Priority.ShouldBe(6);
        // An area without a configured priority falls back to DefaultPriority.
        builder.BuildSection(MakeToc("Post", "/blog/post"), Lead()).Priority.ShouldBe(5);
    }

    [Fact]
    public void BuildSection_PrefixPriority_OverridesArea_ReplacingNotStacking()
    {
        var options = new SearchIndexOptions
        {
            AreaPriorities = new(StringComparer.OrdinalIgnoreCase) { ["imagesharp"] = 15 },
            PrefixPriorities = new(StringComparer.OrdinalIgnoreCase) { ["/imagesharp/api/"] = 3 },
        };
        var builder = new SearchIndexBuilder(options, new LocalizationOptions());

        // A page under the registered prefix takes the prefix priority outright (not 15 + anything).
        builder.BuildSection(MakeToc("WebpEncoder", "/imagesharp/api/webp-encoder"), Lead()).Priority.ShouldBe(3);
        // A same-area page outside the prefix keeps its area priority.
        builder.BuildSection(MakeToc("WebP", "/imagesharp/imageformats/webp"), Lead()).Priority.ShouldBe(15);
    }

    [Fact]
    public void BuildSection_PrefixPriority_LongestMatchWins()
    {
        var options = new SearchIndexOptions
        {
            PrefixPriorities = new(StringComparer.OrdinalIgnoreCase)
            {
                ["/imagesharp/api/"] = 3,
                ["/imagesharp/api/internals/"] = 1,
            },
        };
        var builder = new SearchIndexBuilder(options, new LocalizationOptions());

        builder.BuildSection(MakeToc("Encoder", "/imagesharp/api/webp-encoder"), Lead()).Priority.ShouldBe(3);
        builder.BuildSection(MakeToc("Guts", "/imagesharp/api/internals/buffers"), Lead()).Priority.ShouldBe(1);
    }

    [Fact]
    public void BuildSection_PrefixPriority_NonMatchingUrl_FallsBackToAreaThenDefault()
    {
        var options = new SearchIndexOptions
        {
            AreaPriorities = new(StringComparer.OrdinalIgnoreCase) { ["imagesharp"] = 15 },
            PrefixPriorities = new(StringComparer.OrdinalIgnoreCase) { ["/imagesharp/api/"] = 3 },
        };
        var builder = new SearchIndexBuilder(options, new LocalizationOptions());

        // Non-empty prefix map but no match: resolve area, then default.
        builder.BuildSection(MakeToc("WebP", "/imagesharp/imageformats/webp"), Lead()).Priority.ShouldBe(15);
        builder.BuildSection(MakeToc("Post", "/blog/post"), Lead()).Priority.ShouldBe(5);
    }

    [Fact]
    public void BuildSection_EmptyPrefixMap_IdenticalToAreaOnly()
    {
        // Regression guard for the Count > 0 short-circuit: an empty prefix map changes nothing.
        var options = new SearchIndexOptions
        {
            AreaPriorities = new(StringComparer.OrdinalIgnoreCase) { ["how-to"] = 10 },
        };
        var builder = new SearchIndexBuilder(options, new LocalizationOptions());

        builder.BuildSection(MakeToc("Auth", "/how-to/auth"), Lead()).Priority.ShouldBe(10);
        builder.BuildSection(MakeToc("Post", "/blog/post"), Lead()).Priority.ShouldBe(5);
    }
}
