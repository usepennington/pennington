using System.Collections.Immutable;
using Pennington.Content;
using Pennington.Routing;

namespace Pennington.Tests.Content;

public class MarkdownSourceOverlapDetectorTests
{
    private sealed record FakeSource(
        string AbsoluteContentRoot,
        UrlPath BasePageUrl,
        ImmutableArray<string> ExcludePaths) : IMarkdownContentSource
    {
        public static FakeSource At(string path, params string[] excludes)
            => new(path, new UrlPath("/"), [.. excludes]);
    }

    [Fact]
    public void DetectOverlaps_Disjoint_NoWarning()
    {
        var sources = new[]
        {
            FakeSource.At("/repo/Content"),
            FakeSource.At("/repo/BlogContent"),
        };

        MarkdownSourceOverlapDetector.DetectOverlaps(sources).ShouldBeEmpty();
    }

    [Fact]
    public void DetectOverlaps_SingleSource_NoWarning()
    {
        MarkdownSourceOverlapDetector
            .DetectOverlaps([FakeSource.At("/repo/Content")])
            .ShouldBeEmpty();
    }

    [Fact]
    public void DetectOverlaps_StrictDescendant_WithoutExclusion_EmitsWarning()
    {
        // Shape of the NorthwindHandbookExample misconfiguration: outer source at
        // Content, inner source at Content/changelog, outer doesn't carve it out.
        var sources = new[]
        {
            FakeSource.At("/repo/Content"),
            FakeSource.At("/repo/Content/changelog"),
        };

        var warnings = MarkdownSourceOverlapDetector.DetectOverlaps(sources);

        warnings.Length.ShouldBe(1);
        warnings[0].ShouldContain("/repo/Content");
        warnings[0].ShouldContain("/repo/Content/changelog");
        warnings[0].ShouldContain("ExcludePaths");
        warnings[0].ShouldContain("changelog");
    }

    [Fact]
    public void DetectOverlaps_StrictDescendant_WithExclusion_NoWarning()
    {
        var sources = new[]
        {
            FakeSource.At("/repo/Content", "changelog"),
            FakeSource.At("/repo/Content/changelog"),
        };

        MarkdownSourceOverlapDetector.DetectOverlaps(sources).ShouldBeEmpty();
    }

    [Fact]
    public void DetectOverlaps_WithExclusion_SegmentBasedMatch_NoFalseNegative()
    {
        // Exclude "change" must NOT silence overlap on "changelog" — segment match.
        var sources = new[]
        {
            FakeSource.At("/repo/Content", "change"),
            FakeSource.At("/repo/Content/changelog"),
        };

        MarkdownSourceOverlapDetector.DetectOverlaps(sources).Length.ShouldBe(1);
    }

    [Fact]
    public void DetectOverlaps_DeeperExclusion_NestedSubtree_NoWarning()
    {
        // Exclude "a/b" should cover /Content/a/b/c as well.
        var sources = new[]
        {
            FakeSource.At("/repo/Content", "a/b"),
            FakeSource.At("/repo/Content/a/b/c"),
        };

        MarkdownSourceOverlapDetector.DetectOverlaps(sources).ShouldBeEmpty();
    }

    [Fact]
    public void DetectOverlaps_SameDirectory_NotReportedAsOverlap()
    {
        // Two sources at exactly the same path overlap in a degenerate way, but
        // the strict-descendant check excludes this case. That's fine: the
        // NavigationBuilder DistinctBy pass still prevents double-listing.
        var sources = new[]
        {
            FakeSource.At("/repo/Content"),
            FakeSource.At("/repo/Content"),
        };

        MarkdownSourceOverlapDetector.DetectOverlaps(sources).ShouldBeEmpty();
    }

    [Fact]
    public void DetectOverlaps_TrailingSlashesNormalized()
    {
        var sources = new[]
        {
            FakeSource.At("/repo/Content/"),
            FakeSource.At("/repo/Content/changelog/"),
        };

        MarkdownSourceOverlapDetector.DetectOverlaps(sources).Length.ShouldBe(1);
    }

    [Fact]
    public void DetectOverlaps_BackslashesNormalized()
    {
        // Windows-style paths should match lowercase forward-slash comparison.
        var sources = new[]
        {
            FakeSource.At(@"B:\Penn\examples\N\Content"),
            FakeSource.At(@"B:\Penn\examples\N\Content\changelog"),
        };

        MarkdownSourceOverlapDetector.DetectOverlaps(sources).Length.ShouldBe(1);
    }

    [Fact]
    public void DetectOverlaps_SimilarPrefixButNotDescendant_NoWarning()
    {
        // "/repo/Content" is NOT a parent of "/repo/ContentExtra" — the latter
        // just shares a prefix. Must not be reported as an overlap.
        var sources = new[]
        {
            FakeSource.At("/repo/Content"),
            FakeSource.At("/repo/ContentExtra"),
        };

        MarkdownSourceOverlapDetector.DetectOverlaps(sources).ShouldBeEmpty();
    }
}