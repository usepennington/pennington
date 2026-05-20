using System.Collections.Immutable;
using Pennington.Content;
using Pennington.Generation;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Generation;

public class OverlapAuditorTests
{
    [Fact]
    public async Task AuditAsync_OverlappingMarkdownSources_EmitsWarning()
    {
        var services = new IContentService[]
        {
            new FakeMarkdownSource("/repo/Content"),
            new FakeMarkdownSource("/repo/Content/changelog"),
        };

        var auditor = new OverlapAuditor(services);
        var diagnostics = await auditor.AuditAsync(EmptyContext(), TestContext.Current.CancellationToken);

        diagnostics.Count.ShouldBe(1);
        var diag = diagnostics[0];
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Route.ShouldBeNull();
        diag.SourceFile.ShouldBe("content.overlap");
        diag.Message.ShouldContain("/repo/Content");
        diag.Message.ShouldContain("/repo/Content/changelog");
        diag.Message.ShouldContain("ExcludePaths");
    }

    [Fact]
    public async Task AuditAsync_DisjointSources_EmitsNothing()
    {
        var services = new IContentService[]
        {
            new FakeMarkdownSource("/repo/Content"),
            new FakeMarkdownSource("/repo/Blog"),
        };

        var auditor = new OverlapAuditor(services);
        var diagnostics = await auditor.AuditAsync(EmptyContext(), TestContext.Current.CancellationToken);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task AuditAsync_NonMarkdownContentServices_Ignored()
    {
        var services = new IContentService[] { new NonMarkdownContentService() };

        var auditor = new OverlapAuditor(services);
        var diagnostics = await auditor.AuditAsync(EmptyContext(), TestContext.Current.CancellationToken);

        diagnostics.ShouldBeEmpty();
    }

    private static BuildAuditContext EmptyContext() =>
        new(ImmutableList<ContentTocItem>.Empty, new LocalizationOptions());

    private sealed class FakeMarkdownSource(string root) : IContentService, IMarkdownContentSource
    {
        public string AbsoluteContentRoot { get; } = root;
        public UrlPath BasePageUrl { get; } = new("/");
        public ImmutableArray<string> ExcludePaths { get; } = [];
        public string DefaultSectionLabel => "";
        public int SearchPriority => 0;

        public IAsyncEnumerable<DiscoveredItem> DiscoverAsync() => AsyncEnumerable.Empty<DiscoveredItem>();
        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() => Task.FromResult(ImmutableList<ContentToCopy>.Empty);
        public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync() => Task.FromResult(ImmutableList<ContentToCreate>.Empty);
        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() => Task.FromResult(ImmutableList<ContentTocItem>.Empty);
        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() => Task.FromResult(ImmutableList<CrossReference>.Empty);
    }

    private sealed class NonMarkdownContentService : IContentService
    {
        public string DefaultSectionLabel => "";
        public int SearchPriority => 0;
        public IAsyncEnumerable<DiscoveredItem> DiscoverAsync() => AsyncEnumerable.Empty<DiscoveredItem>();
        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() => Task.FromResult(ImmutableList<ContentToCopy>.Empty);
        public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync() => Task.FromResult(ImmutableList<ContentToCreate>.Empty);
        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() => Task.FromResult(ImmutableList<ContentTocItem>.Empty);
        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() => Task.FromResult(ImmutableList<CrossReference>.Empty);
    }
}