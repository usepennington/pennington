using Pennington.Content;
using Pennington.Infrastructure;
using Pennington.Routing;
using Pennington.Search;

namespace Pennington.Tests.Search;

public class SearchIndexBuilderTests
{
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
}
