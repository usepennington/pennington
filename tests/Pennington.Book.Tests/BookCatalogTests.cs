namespace Pennington.Book.Tests;

using Pennington.Book;
using Pennington.Infrastructure;

public sealed class BookCatalogTests
{
    private static BookCatalog Create(BookOptions options, string siteTitle = "My Docs", Action<PenningtonOptions>? configure = null)
    {
        var penn = new PenningtonOptions { SiteTitle = siteTitle, SiteDescription = "All about it" };
        configure?.Invoke(penn);
        return new BookCatalog(options, penn, penn.Localization, penn.Translations);
    }

    [Fact]
    public void Empty_books_yields_a_single_whole_site_link()
    {
        var catalog = Create(new BookOptions());

        var links = catalog.GetLinks();

        links.Count.ShouldBe(1);
        links[0].Label.ShouldBe("Download as PDF");
        links[0].RoutePrefix.ShouldBe("/");
        links[0].Url.ShouldBe("pdf/book.pdf");
    }

    [Fact]
    public void Explicit_books_map_to_slug_urls()
    {
        var options = new BookOptions();
        options.Books.Add(new BookDefinition("Getting Started", "/tutorials/"));
        options.Books.Add(new BookDefinition("Reference", "/reference/"));

        var links = Create(options).GetLinks();

        links.Count.ShouldBe(2);
        links[0].Url.ShouldBe("pdf/tutorials.pdf");
        links[0].RoutePrefix.ShouldBe("/tutorials/");
        links[1].Url.ShouldBe("pdf/reference.pdf");
    }

    [Fact]
    public void Non_default_locale_shards_the_url_under_the_locale()
    {
        var options = new BookOptions();
        options.Books.Add(new BookDefinition("Getting Started", "/tutorials/"));

        var catalog = Create(options, configure: p =>
        {
            p.Localization.AddLocale("en", "English");
            p.Localization.AddLocale("fr", "Français");
        });

        catalog.GetLinks("fr")[0].Url.ShouldBe("pdf/fr/tutorials.pdf");
        catalog.GetLinks("en")[0].Url.ShouldBe("pdf/tutorials.pdf");
        catalog.GetLinks(null)[0].Url.ShouldBe("pdf/tutorials.pdf");
    }

    [Fact]
    public void Explicit_slug_overrides_the_route_prefix_derivation()
    {
        var options = new BookOptions();
        options.Books.Add(new BookDefinition("Guides", "/how-to/") { Slug = "guides" });

        Create(options).GetLinks()[0].Url.ShouldBe("pdf/guides.pdf");
    }

    [Fact]
    public void Registered_translation_overrides_the_default_label()
    {
        var catalog = Create(new BookOptions(), configure: p =>
            p.Translations.Add("en", BookCatalog.DownloadLabelKey, "Grab the PDF"));

        catalog.GetLinks()[0].Label.ShouldBe("Grab the PDF");
    }

    [Fact]
    public void Label_resolves_per_locale_with_default_locale_fallback()
    {
        var catalog = Create(new BookOptions(), configure: p =>
        {
            p.Localization.AddLocale("en", "English");
            p.Localization.AddLocale("fr", "Français");
            p.Localization.AddLocale("de", "Deutsch");
            p.Translations.Add("en", BookCatalog.DownloadLabelKey, "Download as PDF (en)");
            p.Translations.Add("fr", BookCatalog.DownloadLabelKey, "Télécharger le PDF");
        });

        catalog.GetLinks("fr")[0].Label.ShouldBe("Télécharger le PDF");
        catalog.GetLinks("de")[0].Label.ShouldBe("Download as PDF (en)");
    }
}

public sealed class BookOptionsTests
{
    [Fact]
    public void Resolve_books_synthesizes_a_whole_site_book_from_the_site_options()
    {
        var penn = new PenningtonOptions { SiteTitle = "My Docs", SiteDescription = "All about it" };

        var book = new BookOptions().ResolveBooks(penn).ShouldHaveSingleItem();

        book.Title.ShouldBe("My Docs");
        book.Subtitle.ShouldBe("All about it");
        book.RoutePrefix.ShouldBe("/");
    }

    [Fact]
    public void Whole_site_book_falls_back_to_documentation_when_site_title_blank()
    {
        var penn = new PenningtonOptions { SiteTitle = "" };

        new BookOptions().ResolveBooks(penn).ShouldHaveSingleItem().Title.ShouldBe("Documentation");
    }
}

public sealed class BookDefinitionTests
{
    [Theory]
    [InlineData("/tutorials/", "tutorials")]
    [InlineData("tutorials", "tutorials")]
    [InlineData("/how-to/nested/", "how-to-nested")]
    [InlineData("/", "book")]
    public void Effective_slug_flattens_the_route_prefix(string prefix, string expected)
        => new BookDefinition("T", prefix).EffectiveSlug.ShouldBe(expected);

    [Theory]
    [InlineData("/tutorials/", "/tutorials/")]
    [InlineData("tutorials", "/tutorials/")]
    [InlineData("/", "/")]
    [InlineData("", "/")]
    public void Normalized_route_prefix_has_leading_and_trailing_slash(string prefix, string expected)
        => new BookDefinition("T", prefix).NormalizedRoutePrefix.ShouldBe(expected);
}
