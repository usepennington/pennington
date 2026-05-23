using AngleSharp.Html.Parser;
using Pennington.Search;

namespace Pennington.Tests.Search;

public class HeadingSectionExtractorTests
{
    private static IReadOnlyList<HeadingSection> Extract(string bodyHtml, bool excludeCode = true)
    {
        var doc = new HtmlParser().ParseDocument($"<html><body>{bodyHtml}</body></html>");
        return new HeadingSectionExtractor().Extract(doc.Body!, excludeCode);
    }

    [Fact]
    public void Extract_SplitsByHeading_WithAnchorsAndBodies()
    {
        var sections = Extract(
            "<p>Intro text.</p>" +
            "<h2 id=\"install\">Install</h2><p>Install body.</p>" +
            "<h2 id=\"usage\">Usage</h2><p>Usage body.</p>");

        sections.Count.ShouldBe(3); // lead + two headings

        sections[0].IsLead.ShouldBeTrue();
        sections[0].AnchorId.ShouldBeNull();
        sections[0].Text.ShouldBe("Intro text.");

        sections[1].AnchorId.ShouldBe("install");
        sections[1].Title.ShouldBe("Install");
        sections[1].Text.ShouldBe("Install body.");
        sections[1].Crumbs.ShouldBeEmpty();

        sections[2].AnchorId.ShouldBe("usage");
        sections[2].Text.ShouldBe("Usage body.");
    }

    [Fact]
    public void Extract_BuildsCrumbTrail_ForNestedHeadings()
    {
        var sections = Extract(
            "<h2 id=\"a\">Alpha</h2><p>a</p>" +
            "<h3 id=\"b\">Bravo</h3><p>b</p>" +
            "<h2 id=\"c\">Charlie</h2><p>c</p>");

        var bravo = sections.Single(s => s.AnchorId == "b");
        bravo.Level.ShouldBe(3);
        bravo.Crumbs.ShouldBe(["Alpha"]);

        var charlie = sections.Single(s => s.AnchorId == "c");
        charlie.Crumbs.ShouldBeEmpty(); // back up to h2 level — Alpha popped off the trail
    }

    [Fact]
    public void Extract_ExcludesCodeBlocks_WhenRequested()
    {
        var sections = Extract(
            "<h2 id=\"x\">X</h2><p>Prose here.</p><pre><code>SECRETTOKEN var y = 1;</code></pre>");

        var x = sections.Single(s => s.AnchorId == "x");
        x.Text.ShouldContain("Prose here.");
        x.Text.ShouldNotContain("SECRETTOKEN");
    }

    [Fact]
    public void Extract_KeepsCodeBlocks_WhenNotExcluded()
    {
        var sections = Extract(
            "<h2 id=\"x\">X</h2><pre><code>SECRETTOKEN</code></pre>",
            excludeCode: false);

        sections.Single(s => s.AnchorId == "x").Text.ShouldContain("SECRETTOKEN");
    }

    [Fact]
    public void Extract_NoHeadings_ProducesSingleLeadSection()
    {
        var sections = Extract("<p>Just prose, no headings.</p>");

        var lead = sections.ShouldHaveSingleItem();
        lead.IsLead.ShouldBeTrue();
        lead.AnchorId.ShouldBeNull();
        lead.Text.ShouldBe("Just prose, no headings.");
    }

    [Fact]
    public void Extract_HeadingWithoutId_DoesNotStartSection()
    {
        // No anchor id means nothing to deep-link to, so the heading stays in the body flow.
        var sections = Extract("<p>intro</p><h2>No Anchor</h2><p>body</p>");

        sections.ShouldHaveSingleItem().IsLead.ShouldBeTrue();
    }
}
