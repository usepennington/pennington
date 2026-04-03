using System.Collections.Immutable;
using Penn.Pipeline;
using Penn.Routing;
using Penn.Search;

namespace Penn.Tests.Pipeline;

public class PipelineTypeTests
{
    [Fact]
    public void OutlineEntry_CreateAndVerifyProperties()
    {
        var entry = new OutlineEntry("intro", "Introduction", 2);
        entry.Id.ShouldBe("intro");
        entry.Text.ShouldBe("Introduction");
        entry.Level.ShouldBe(2);
    }

    [Fact]
    public void Tag_CreateAndVerifyProperties()
    {
        var tag = new Tag("C Sharp", "c-sharp");
        tag.Name.ShouldBe("C Sharp");
        tag.Slug.ShouldBe("c-sharp");
    }

    [Fact]
    public void CrossReference_CreateWithContentRouteAndVerify()
    {
        var route = new ContentRoute
        {
            CanonicalPath = "/docs/intro",
            OutputFile = "docs/intro/index.html"
        };
        var xref = new CrossReference("intro-uid", "Introduction", route);

        xref.Uid.ShouldBe("intro-uid");
        xref.Title.ShouldBe("Introduction");
        xref.Route.ShouldBe(route);
        xref.Route.CanonicalPath.Value.ShouldBe("/docs/intro");
    }

    [Fact]
    public void SocialMetadata_AllNullableFieldsNull()
    {
        var social = new SocialMetadata(null, null, null, null, null);
        social.Description.ShouldBeNull();
        social.ImageUrl.ShouldBeNull();
        social.Type.ShouldBeNull();
        social.PublishedTime.ShouldBeNull();
        social.Author.ShouldBeNull();
    }

    [Fact]
    public void SocialMetadata_AllFieldsSet()
    {
        var date = new DateTime(2026, 4, 1);
        var social = new SocialMetadata("A description", "https://example.com/image.png", "article", date, "Jane");
        social.Description.ShouldBe("A description");
        social.ImageUrl.ShouldBe("https://example.com/image.png");
        social.Type.ShouldBe("article");
        social.PublishedTime.ShouldBe(date);
        social.Author.ShouldBe("Jane");
    }

    [Fact]
    public void ContentError_MessageOnly()
    {
        var error = new ContentError("Something went wrong");
        error.Message.ShouldBe("Something went wrong");
        error.Exception.ShouldBeNull();
    }

    [Fact]
    public void ContentError_MessageAndException()
    {
        var ex = new InvalidOperationException("bad state");
        var error = new ContentError("Something went wrong", ex);
        error.Message.ShouldBe("Something went wrong");
        error.Exception.ShouldBe(ex);
    }

    [Fact]
    public void RenderedContent_AllProperties()
    {
        var outline = new[] { new OutlineEntry("h1", "Title", 1) };
        var tags = ImmutableList.Create(new Tag("csharp", "csharp"));
        var route = new ContentRoute
        {
            CanonicalPath = "/docs/api",
            OutputFile = "docs/api/index.html"
        };
        var xrefs = ImmutableList.Create(new CrossReference("api-uid", "API", route));
        var searchDoc = new SearchIndexDocument("Title", "Body text", "/docs/api", "docs", "en", 10);
        var social = new SocialMetadata("desc", null, "article", null, "Author");

        var rendered = new RenderedContent("<h1>Title</h1>", outline, tags, xrefs, searchDoc, social);

        rendered.Html.ShouldBe("<h1>Title</h1>");
        rendered.Outline.Length.ShouldBe(1);
        rendered.Outline[0].Text.ShouldBe("Title");
        rendered.Tags.Count.ShouldBe(1);
        rendered.Tags[0].Name.ShouldBe("csharp");
        rendered.CrossReferences.Count.ShouldBe(1);
        rendered.CrossReferences[0].Uid.ShouldBe("api-uid");
        rendered.SearchDocument.ShouldNotBeNull();
        rendered.SearchDocument!.Title.ShouldBe("Title");
        rendered.Social.ShouldNotBeNull();
        rendered.Social!.Description.ShouldBe("desc");
    }

    [Fact]
    public void RenderedContent_NullOptionalFields()
    {
        var rendered = new RenderedContent(
            "<p>Hello</p>",
            [],
            ImmutableList<Tag>.Empty,
            ImmutableList<CrossReference>.Empty,
            null,
            null
        );

        rendered.Html.ShouldBe("<p>Hello</p>");
        rendered.Outline.ShouldBeEmpty();
        rendered.Tags.ShouldBeEmpty();
        rendered.CrossReferences.ShouldBeEmpty();
        rendered.SearchDocument.ShouldBeNull();
        rendered.Social.ShouldBeNull();
    }

    [Fact]
    public void SearchIndexDocument_CreateAndVerifyAllProperties()
    {
        var doc = new SearchIndexDocument("My Page", "Full body text", "/search/page", "docs", "en", 5);

        doc.Title.ShouldBe("My Page");
        doc.Body.ShouldBe("Full body text");
        doc.Url.Value.ShouldBe("/search/page");
        doc.Section.ShouldBe("docs");
        doc.Locale.ShouldBe("en");
        doc.Priority.ShouldBe(5);
    }
}
