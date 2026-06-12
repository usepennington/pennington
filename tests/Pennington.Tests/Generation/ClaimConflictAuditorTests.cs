using System.Collections.Immutable;
using Pennington.Artifacts;
using Pennington.Content;
using Pennington.Diagnostics;
using Pennington.Generation;
using Pennington.Localization;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Generation;

public class ClaimConflictAuditorTests
{
    [Fact]
    public async Task ContentRouteInsideClaim_Warns()
    {
        var auditor = new ClaimConflictAuditor(
            [new FakeContentService(MakeRoute("/pdf/handbook.pdf"))],
            [new FakeArtifactService(new ArtifactClaim("book", new PrefixClaim(new UrlPath("/pdf/"), ".pdf"), "book PDFs"))]);

        var diagnostics = await auditor.AuditAsync(EmptyContext(), TestContext.Current.CancellationToken);

        var warning = diagnostics.ShouldHaveSingleItem();
        warning.Severity.ShouldBe(DiagnosticSeverity.Warning);
        warning.Message.ShouldContain("/pdf/handbook.pdf");
        warning.Message.ShouldContain("/pdf/**.pdf");
    }

    [Fact]
    public async Task ContentRouteOutsideClaims_Silent()
    {
        var auditor = new ClaimConflictAuditor(
            [new FakeContentService(MakeRoute("/docs/page/"))],
            [new FakeArtifactService(new ArtifactClaim("book", new PrefixClaim(new UrlPath("/pdf/"), ".pdf"), "book PDFs"))]);

        var diagnostics = await auditor.AuditAsync(EmptyContext(), TestContext.Current.CancellationToken);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task RouteNamedLikeSubtreeLlms_Warns()
    {
        // The suffix claim polices the whole tree: a markdown file someone names
        // llms.txt anywhere would be shadowed by the artifact router.
        var auditor = new ClaimConflictAuditor(
            [new FakeContentService(MakeRoute("/guides/llms.txt"))],
            [new FakeArtifactService(new ArtifactClaim("llms", new SuffixClaim("/llms.txt"), "per-subtree llms.txt indexes"))]);

        var diagnostics = await auditor.AuditAsync(EmptyContext(), TestContext.Current.CancellationToken);

        diagnostics.ShouldHaveSingleItem().Message.ShouldContain("**/llms.txt");
    }

    [Fact]
    public async Task CrossOwnerOverlappingClaims_Warn()
    {
        var auditor = new ClaimConflictAuditor(
            [],
            [
                new FakeArtifactService(new ArtifactClaim("alpha", new PrefixClaim(new UrlPath("/data/")), "alpha data")),
                new FakeArtifactService(new ArtifactClaim("beta", new ExactClaim(new UrlPath("/data/feed.json")), "beta feed")),
            ]);

        var diagnostics = await auditor.AuditAsync(EmptyContext(), TestContext.Current.CancellationToken);

        var warning = diagnostics.ShouldHaveSingleItem();
        warning.Message.ShouldContain("alpha");
        warning.Message.ShouldContain("beta");
    }

    [Fact]
    public async Task SameOwnerOverlappingClaims_Silent()
    {
        // The llms service legitimately claims both /llms.txt (exact) and **/llms.txt
        // (suffix); same-owner overlap is by design.
        var auditor = new ClaimConflictAuditor(
            [],
            [
                new FakeArtifactService(
                    new ArtifactClaim("llms", new ExactClaim(new UrlPath("/llms.txt")), "front door"),
                    new ArtifactClaim("llms", new SuffixClaim("/llms.txt"), "subtrees")),
            ]);

        var diagnostics = await auditor.AuditAsync(EmptyContext(), TestContext.Current.CancellationToken);

        diagnostics.ShouldBeEmpty();
    }

    private static BuildAuditContext EmptyContext() =>
        new(ImmutableList<ContentTocItem>.Empty, new LocalizationOptions());

    private static ContentRoute MakeRoute(string canonical) => new()
    {
        CanonicalPath = new UrlPath(canonical),
        OutputFile = new FilePath(canonical.Trim('/')),
    };

    private sealed class FakeContentService(params ContentRoute[] routes) : IContentService
    {
        public string DefaultSectionLabel => "";
        public int SearchPriority => 0;

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            await Task.Yield();
            foreach (var route in routes)
            {
                yield return new DiscoveredItem(route, new FileSource(new FilePath("stub.md"), "markdown"));
            }
        }

        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() => Task.FromResult(ImmutableList<ContentToCopy>.Empty);
        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() => Task.FromResult(ImmutableList<ContentTocItem>.Empty);
        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() => Task.FromResult(ImmutableList<CrossReference>.Empty);
    }

    private sealed class FakeArtifactService(params ArtifactClaim[] claims) : IArtifactContentService
    {
        public ImmutableList<ArtifactClaim> Claims { get; } = [.. claims];

        public Task<ArtifactContent?> ResolveAsync(string relativePath, CancellationToken cancellationToken)
            => Task.FromResult<ArtifactContent?>(null);

        public IAsyncEnumerable<DiscoveredItem> DiscoverAsync() => AsyncEnumerable.Empty<DiscoveredItem>();
    }
}
