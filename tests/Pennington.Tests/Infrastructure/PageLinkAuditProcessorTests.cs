using System.Collections.Immutable;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Pennington.Content;
using Pennington.Diagnostics;
using Pennington.Generation;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Infrastructure;

public class PageLinkAuditProcessorTests
{
    [Fact]
    public async Task ProcessAsync_BrokenInternalLink_AddsWarningToDiagnosticContext()
    {
        var verifier = BuildVerifier(knownRoutes: [MakeRoute("/page/")]);
        var diagnosticContext = new DiagnosticContext();
        var processor = new PageLinkAuditProcessor();
        var ctx = HtmlResponseContext("/page/", verifier, diagnosticContext);

        await processor.ProcessAsync("""<a href="/missing/">link</a>""", ctx);

        diagnosticContext.Diagnostics.Count.ShouldBe(1);
        diagnosticContext.Diagnostics[0].Severity.ShouldBe(DiagnosticSeverity.Warning);
        diagnosticContext.Diagnostics[0].Message.ShouldContain("/missing/");
        diagnosticContext.Diagnostics[0].Source.ShouldStartWith("content.links/Internal/");
    }

    [Fact]
    public async Task ProcessAsync_ValidLink_AddsNothing()
    {
        var verifier = BuildVerifier(knownRoutes: [MakeRoute("/page/"), MakeRoute("/other/")]);
        var diagnosticContext = new DiagnosticContext();
        var processor = new PageLinkAuditProcessor();
        var ctx = HtmlResponseContext("/page/", verifier, diagnosticContext);

        await processor.ProcessAsync("""<a href="/other/">link</a>""", ctx);

        diagnosticContext.Diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldProcess_NonHtmlResponse_ReturnsFalse()
    {
        var processor = new PageLinkAuditProcessor();
        var ctx = new DefaultHttpContext();
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "application/json";

        processor.ShouldProcess(ctx).ShouldBeFalse();
    }

    [Fact]
    public void ShouldProcess_HtmlError_ReturnsFalse()
    {
        var processor = new PageLinkAuditProcessor();
        var ctx = new DefaultHttpContext();
        ctx.Response.StatusCode = 500;
        ctx.Response.ContentType = "text/html";

        processor.ShouldProcess(ctx).ShouldBeFalse();
    }

    private static DefaultHttpContext HtmlResponseContext(
        string path,
        PageLinkVerifier verifier,
        DiagnosticContext diagnosticContext)
    {
        var services = new ServiceCollection();
        services.AddSingleton(verifier);
        services.AddSingleton(diagnosticContext);
        var sp = services.BuildServiceProvider();

        var ctx = new DefaultHttpContext { RequestServices = sp };
        ctx.Request.Path = path;
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "text/html";
        return ctx;
    }

    private static PageLinkVerifier BuildVerifier(IReadOnlyList<ContentRoute> knownRoutes) =>
        new(
            [new FakeService(knownRoutes)],
            new EmptyEndpointDataSource(),
            new OutputOptions { OutputDirectory = new FilePath("output") },
            new StubWebHostEnvironment());

    private sealed class StubWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ApplicationName { get; set; } = "Test";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = "";
        public string EnvironmentName { get; set; } = "Test";
    }

    private static ContentRoute MakeRoute(string canonical) => new()
    {
        CanonicalPath = new UrlPath(canonical),
        OutputFile = new FilePath(canonical.Trim('/') + ".html"),
    };

    private sealed class FakeService(IReadOnlyList<ContentRoute> routes) : IContentService
    {
        public string DefaultSectionLabel => "";
        public int SearchPriority => 0;

        public async IAsyncEnumerable<DiscoveredItem> DiscoverAsync()
        {
            await Task.Yield();
            foreach (var route in routes)
            {
                yield return new DiscoveredItem(route, new MarkdownFileSource(new FilePath("stub.md")));
            }
        }

        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() => Task.FromResult(ImmutableList<ContentToCopy>.Empty);
        public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync() => Task.FromResult(ImmutableList<ContentToCreate>.Empty);
        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() => Task.FromResult(ImmutableList<ContentTocItem>.Empty);
        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() => Task.FromResult(ImmutableList<CrossReference>.Empty);
    }

    private sealed class EmptyEndpointDataSource : EndpointDataSource
    {
        public override IReadOnlyList<Endpoint> Endpoints => [];
        public override IChangeToken GetChangeToken() => new CancellationChangeToken(CancellationToken.None);
    }
}
