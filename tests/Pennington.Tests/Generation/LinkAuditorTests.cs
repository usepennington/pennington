using System.Collections.Immutable;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Pennington.Content;
using Pennington.Generation;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Generation;

public class LinkAuditorTests
{
    [Fact]
    public async Task AuditAsync_BrokenInternalLink_EmitsWarning()
    {
        var page = MakeRoute("/page/", "/repo/page.md");
        var service = new FakeService([page], []);
        var auditor = new LinkAuditor(
            [service],
            [],
            new EmptyEndpointDataSource(),
            new OutputOptions { OutputDirectory = new FilePath("output") },
            StubEnv());

        var context = new RenderedAuditContext(
            ImmutableList.Create(page),
            new LocalizationOptions(),
            (route, ct) => Task.FromResult<string?>("""<a href="/missing/">link</a>"""));

        var diagnostics = await auditor.AuditAsync(context, TestContext.Current.CancellationToken);

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Severity.ShouldBe(DiagnosticSeverity.Warning);
        diagnostics[0].Route.ShouldBe(page);
        diagnostics[0].Message.ShouldContain("/missing/");
        diagnostics[0].SourceFile.ShouldStartWith("content.links/Internal/");
    }

    [Fact]
    public async Task AuditAsync_LinkResolvesToKnownRoute_NoWarning()
    {
        var page = MakeRoute("/page/", "/repo/page.md");
        var other = MakeRoute("/other/", "/repo/other.md");
        var service = new FakeService([page, other], []);
        var auditor = new LinkAuditor(
            [service],
            [],
            new EmptyEndpointDataSource(),
            new OutputOptions { OutputDirectory = new FilePath("output") },
            StubEnv());

        var context = new RenderedAuditContext(
            ImmutableList.Create(page),
            new LocalizationOptions(),
            (route, ct) => Task.FromResult<string?>("""<a href="/other/">link</a>"""));

        var diagnostics = await auditor.AuditAsync(context, TestContext.Current.CancellationToken);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task AuditAsync_LinkToCopiedAsset_NoWarning()
    {
        var page = MakeRoute("/page/", "/repo/page.md");
        var service = new FakeService(
            [page],
            [new ContentToCopy(new FilePath("/repo/media/foo.svg"), new FilePath("media/foo.svg"))]);
        var auditor = new LinkAuditor(
            [service],
            [],
            new EmptyEndpointDataSource(),
            new OutputOptions { OutputDirectory = new FilePath("output") },
            StubEnv());

        var context = new RenderedAuditContext(
            ImmutableList.Create(page),
            new LocalizationOptions(),
            (route, ct) => Task.FromResult<string?>("""<img src="/media/foo.svg">"""));

        var diagnostics = await auditor.AuditAsync(context, TestContext.Current.CancellationToken);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task AuditAsync_NullHtml_PageSkipped()
    {
        var page = MakeRoute("/page/", "/repo/page.md");
        var service = new FakeService([page], []);
        var auditor = new LinkAuditor(
            [service],
            [],
            new EmptyEndpointDataSource(),
            new OutputOptions { OutputDirectory = new FilePath("output") },
            StubEnv());

        var context = new RenderedAuditContext(
            ImmutableList.Create(page),
            new LocalizationOptions(),
            (route, ct) => Task.FromResult<string?>(null));

        var diagnostics = await auditor.AuditAsync(context, TestContext.Current.CancellationToken);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task AuditAsync_LinkToEmittedFile_NoWarning()
    {
        var page = MakeRoute("/page/", "/repo/page.md");
        var service = new FakeService([page], []);
        var emitter = new FakeEmitter([
            new ContentToCreate(
                new FilePath("utility/llms.txt"),
                () => Task.FromResult(Array.Empty<byte>()),
                "text/markdown"),
        ]);
        var auditor = new LinkAuditor(
            [service],
            [emitter],
            new EmptyEndpointDataSource(),
            new OutputOptions { OutputDirectory = new FilePath("output") },
            StubEnv());

        var context = new RenderedAuditContext(
            ImmutableList.Create(page),
            new LocalizationOptions(),
            (route, ct) => Task.FromResult<string?>("""<a href="/utility/llms.txt">subtree</a>"""));

        var diagnostics = await auditor.AuditAsync(context, TestContext.Current.CancellationToken);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task AuditAsync_DiagnosticEncodesLinkType()
    {
        var page = MakeRoute("/page/", "/repo/page.md");
        var service = new FakeService([page], []);
        var auditor = new LinkAuditor(
            [service],
            [],
            new EmptyEndpointDataSource(),
            new OutputOptions { OutputDirectory = new FilePath("output") },
            StubEnv());

        var context = new RenderedAuditContext(
            ImmutableList.Create(page),
            new LocalizationOptions(),
            (route, ct) => Task.FromResult<string?>("""<img src="/missing.png">"""));

        var diagnostics = await auditor.AuditAsync(context, TestContext.Current.CancellationToken);

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].SourceFile.ShouldBe("content.links/Image//missing.png");
    }

    [Fact]
    public async Task AuditAsync_LinkToWebRootAsset_NoWarning()
    {
        // wwwroot assets (the documented home for shared, absolute-URL assets) are copied by the
        // build but owned by no content service. The auditor must fold them in so they don't read
        // as broken.
        var page = MakeRoute("/page/", "/repo/page.md");
        var service = new FakeService([page], []);
        var auditor = new LinkAuditor(
            [service],
            [],
            new EmptyEndpointDataSource(),
            new OutputOptions { OutputDirectory = new FilePath("output") },
            StubEnv("logo.svg", "/wwwroot/logo.svg"));

        var context = new RenderedAuditContext(
            ImmutableList.Create(page),
            new LocalizationOptions(),
            (route, ct) => Task.FromResult<string?>("""<img src="/logo.svg">"""));

        var diagnostics = await auditor.AuditAsync(context, TestContext.Current.CancellationToken);

        diagnostics.ShouldBeEmpty();
    }

    private static ContentRoute MakeRoute(string canonical, string sourcePath) => new()
    {
        CanonicalPath = new UrlPath(canonical),
        OutputFile = new FilePath(canonical.Trim('/') + ".html"),
        SourceFile = new FilePath(sourcePath),
    };

    private sealed class FakeService(IReadOnlyList<ContentRoute> routes, IReadOnlyList<ContentToCopy> assets) : IContentService
    {
        public string DefaultSectionLabel => "";
        public int SearchPriority => 0;

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            await Task.Yield();
            foreach (var route in routes)
            {
                yield return new DiscoveredItem(route, new FileSource(route.SourceFile ?? new FilePath("stub.md"), "markdown"));
            }
        }

        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() => Task.FromResult(assets.ToImmutableList());
        public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync() => Task.FromResult(ImmutableList<ContentToCreate>.Empty);
        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() => Task.FromResult(ImmutableList<ContentTocItem>.Empty);
        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() => Task.FromResult(ImmutableList<CrossReference>.Empty);
    }

    private sealed class FakeEmitter(IReadOnlyList<ContentToCreate> items) : IContentEmitter
    {
        public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync() => Task.FromResult(items.ToImmutableList());
    }

    private sealed class EmptyEndpointDataSource : EndpointDataSource
    {
        public override IReadOnlyList<Endpoint> Endpoints => [];
        public override IChangeToken GetChangeToken() => new CancellationChangeToken(CancellationToken.None);
    }

    private static IWebHostEnvironment StubEnv(string? relativePath = null, string? physicalPath = null) =>
        new StubWebHostEnvironment
        {
            WebRootFileProvider = relativePath is null
                ? new NullFileProvider()
                : new SingleFileProvider(relativePath, physicalPath!),
        };

    private sealed class StubWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ApplicationName { get; set; } = "Test";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = "";
        public string EnvironmentName { get; set; } = "Test";
    }

    // Exposes a single root-level file; enough for the walker to surface it as a known asset.
    private sealed class SingleFileProvider(string name, string physicalPath) : IFileProvider
    {
        public IDirectoryContents GetDirectoryContents(string subpath) =>
            string.IsNullOrEmpty(subpath?.Trim('/'))
                ? new Contents(new FileInfo(name, physicalPath))
                : NotFoundDirectoryContents.Singleton;

        public IFileInfo GetFileInfo(string subpath) => new NotFoundFileInfo(subpath);
        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;

        private sealed class Contents(IFileInfo entry) : IDirectoryContents
        {
            public bool Exists => true;
            public IEnumerator<IFileInfo> GetEnumerator() { yield return entry; }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class FileInfo(string name, string physicalPath) : IFileInfo
        {
            public bool Exists => true;
            public long Length => 0;
            public string PhysicalPath => physicalPath;
            public string Name => name;
            public DateTimeOffset LastModified => DateTimeOffset.MinValue;
            public bool IsDirectory => false;
            public Stream CreateReadStream() => throw new NotSupportedException();
        }
    }
}