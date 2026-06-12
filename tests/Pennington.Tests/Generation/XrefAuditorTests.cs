using System.Collections.Immutable;
using Pennington.Content;
using Pennington.Generation;
using Pennington.Infrastructure;
using Pennington.Localization;
using Pennington.Pipeline;
using Pennington.Routing;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Generation;

public class XrefAuditorTests
{
    [Fact]
    public async Task AuditAsync_UnresolvedXref_EmitsWarningOnRoute()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/repo");
        fs.File.WriteAllText("/repo/page.md", """
            ---
            title: Sample
            ---

            See <xref:does-not-exist> for context.
            """);

        var route = MakeRoute("/page/", "/repo/page.md");
        var service = new FakeContentService([(route, "/repo/page.md")], []);
        var auditor = new XrefAuditor([service], new XrefResolver([service]), fs);

        var diagnostics = await auditor.AuditAsync(EmptyContext(), TestContext.Current.CancellationToken);

        diagnostics.Count.ShouldBe(1);
        var diag = diagnostics[0];
        diag.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diag.Route.ShouldBe(route);
        diag.SourceFile.ShouldBe("content.xref/does-not-exist");
        diag.Message.ShouldContain("does-not-exist");
        diag.Message.ShouldContain("/page/");
    }

    [Fact]
    public async Task AuditAsync_ResolvableXref_NoWarning()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/repo");
        fs.File.WriteAllText("/repo/page.md", "See <xref:target-page> here.");

        var route = MakeRoute("/page/", "/repo/page.md");
        var targetRoute = MakeRoute("/target/", "/repo/target.md");
        var service = new FakeContentService(
            [(route, "/repo/page.md")],
            [new CrossReference("target-page", "Target", targetRoute)]);
        var auditor = new XrefAuditor([service], new XrefResolver([service]), fs);

        var diagnostics = await auditor.AuditAsync(EmptyContext(), TestContext.Current.CancellationToken);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task AuditAsync_DuplicateUidPerPage_EmitsOnce()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/repo");
        fs.File.WriteAllText("/repo/page.md", """
            <xref:missing> ... <xref:missing> ... [link](xref:missing)
            """);

        var route = MakeRoute("/page/", "/repo/page.md");
        var service = new FakeContentService([(route, "/repo/page.md")], []);
        var auditor = new XrefAuditor([service], new XrefResolver([service]), fs);

        var diagnostics = await auditor.AuditAsync(EmptyContext(), TestContext.Current.CancellationToken);

        diagnostics.Count.ShouldBe(1);
    }

    [Fact]
    public async Task AuditAsync_XrefInFencedCodeBlock_Ignored()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/repo");
        fs.File.WriteAllText("/repo/page.md", """
            Real reference: <xref:real>.

            ```markdown
            <xref:looks-broken-but-is-just-a-sample>
            ```

            And inline `<xref:also-not-real>`.
            """);

        var route = MakeRoute("/page/", "/repo/page.md");
        var realTarget = MakeRoute("/real/", "/repo/real.md");
        var service = new FakeContentService(
            [(route, "/repo/page.md")],
            [new CrossReference("real", "Real", realTarget)]);
        var auditor = new XrefAuditor([service], new XrefResolver([service]), fs);

        var diagnostics = await auditor.AuditAsync(EmptyContext(), TestContext.Current.CancellationToken);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task AuditAsync_LinkSyntaxXref_AlsoChecked()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/repo");
        fs.File.WriteAllText("/repo/page.md", "See [the docs](xref:nope) please.");

        var route = MakeRoute("/page/", "/repo/page.md");
        var service = new FakeContentService([(route, "/repo/page.md")], []);
        var auditor = new XrefAuditor([service], new XrefResolver([service]), fs);

        var diagnostics = await auditor.AuditAsync(EmptyContext(), TestContext.Current.CancellationToken);

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Message.ShouldContain("nope");
    }

    [Fact]
    public async Task AuditAsync_MissingFile_NoCrash()
    {
        var fs = new MockFileSystem();
        // No file written.

        var route = MakeRoute("/page/", "/repo/missing.md");
        var service = new FakeContentService([(route, "/repo/missing.md")], []);
        var auditor = new XrefAuditor([service], new XrefResolver([service]), fs);

        var diagnostics = await auditor.AuditAsync(EmptyContext(), TestContext.Current.CancellationToken);

        diagnostics.ShouldBeEmpty();
    }

    private static ContentRoute MakeRoute(string canonical, string sourcePath) => new()
    {
        CanonicalPath = new UrlPath(canonical),
        OutputFile = new FilePath(canonical.Trim('/') + ".html"),
        SourceFile = new FilePath(sourcePath),
    };

    private static BuildAuditContext EmptyContext() =>
        new(ImmutableList<ContentTocItem>.Empty, new LocalizationOptions());

    private sealed class FakeContentService(
        IReadOnlyList<(ContentRoute Route, string SourcePath)> items,
        IReadOnlyList<CrossReference> crossRefs) : IContentService
    {
        public string DefaultSectionLabel => "";
        public int SearchPriority => 0;

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            await Task.Yield();
            foreach (var (route, sourcePath) in items)
            {
                yield return new DiscoveredItem(route, new FileSource(new FilePath(sourcePath), "markdown"));
            }
        }

        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() => Task.FromResult(ImmutableList<ContentToCopy>.Empty);
        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() => Task.FromResult(ImmutableList<ContentTocItem>.Empty);
        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() => Task.FromResult(crossRefs.ToImmutableList());
    }
}