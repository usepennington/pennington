using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Content;
using Pennington.Infrastructure;
using Pennington.Pipeline;
using Pennington.Routing;

namespace Pennington.Tests.Infrastructure;

public class XrefResolvingServiceTests
{
    private static ContentRoute MakeRoute(string path) => new()
    {
        CanonicalPath = new UrlPath(path).EnsureTrailingSlash(),
        OutputFile = new FilePath($"{path.TrimStart('/')}/index.html"),
    };

    private static XrefResolvingService CreateService(params CrossReference[] refs)
    {
        var contentService = new StubContentService(refs.ToImmutableList());
        var services = new ServiceCollection();
        services.AddSingleton<IContentService>(contentService);
        services.AddSingleton<XrefResolver>();
        var provider = services.BuildServiceProvider();
        return new XrefResolvingService(provider);
    }

    // --- Resolved xref tag: title with angle brackets is encoded ---

    [Fact]
    public async Task ResolvedXrefTag_TitleWithAngleBrackets_IsHtmlEncoded()
    {
        var service = CreateService(
            new CrossReference("MyType", "List<string>", MakeRoute("/api/list")));

        var result = await service.ResolveAsync("<p>See <xref:MyType> for details.</p>");

        result.ShouldContain("&gt;");
        result.ShouldContain("&lt;");
        result.ShouldNotContain(">List<string><");
    }

    // --- Resolved xref tag: title with ampersand is encoded ---

    [Fact]
    public async Task ResolvedXrefTag_TitleWithAmpersand_IsHtmlEncoded()
    {
        var service = CreateService(
            new CrossReference("util", "Save & Load", MakeRoute("/api/util")));

        var result = await service.ResolveAsync("<p><xref:util></p>");

        result.ShouldContain("Save &amp; Load");
        result.ShouldNotContain("Save & Load");
    }

    // --- Resolved xref tag: title with double quotes is encoded ---

    [Fact]
    public async Task ResolvedXrefTag_TitleWithDoubleQuotes_IsHtmlEncoded()
    {
        var service = CreateService(
            new CrossReference("q", """The "Quoted" Type""", MakeRoute("/api/q")));

        var result = await service.ResolveAsync("<p><xref:q></p>");

        result.ShouldContain("&quot;Quoted&quot;");
    }

    // --- Resolved xref tag: title with single quotes is encoded ---

    [Fact]
    public async Task ResolvedXrefTag_TitleWithSingleQuotes_IsHtmlEncoded()
    {
        var service = CreateService(
            new CrossReference("apos", "It's a type", MakeRoute("/api/apos")));

        var result = await service.ResolveAsync("<p><xref:apos></p>");

        result.ShouldContain("It&#39;s a type");
    }

    // --- Resolved xref tag: path with special chars in href is attribute-encoded ---

    [Fact]
    public async Task ResolvedXrefTag_PathWithSpecialChars_IsAttributeEncoded()
    {
        var route = new ContentRoute
        {
            CanonicalPath = new UrlPath("/api/q&a/"),
            OutputFile = new FilePath("api/q&a/index.html"),
        };
        var service = CreateService(new CrossReference("qa", "Q&A", route));

        var result = await service.ResolveAsync("<p><xref:qa></p>");

        result.ShouldContain("""href="/api/q&amp;a/""");
    }

    // --- Unresolved xref tag: uid with special chars is encoded ---

    [Fact]
    public async Task UnresolvedXrefTag_UidWithAngleBrackets_IsHtmlEncoded()
    {
        var service = CreateService(); // no refs registered

        var result = await service.ResolveAsync("""<p><xref:List<int>></p>""");

        // The uid "List<int" is captured by regex (stops at first >)
        // Verify encoding in attributes and content
        result.ShouldContain("&lt;");
    }

    [Fact]
    public async Task UnresolvedXrefTag_UidWithAmpersand_IsHtmlEncoded()
    {
        var service = CreateService();

        var result = await service.ResolveAsync("<p><xref:Save&Load></p>");

        result.ShouldContain("Save&amp;Load");
        result.ShouldContain("""data-xref-uid="Save&amp;Load""");
    }

    [Fact]
    public async Task UnresolvedXrefTag_UidWithQuotes_IsHtmlEncoded()
    {
        var service = CreateService();

        var result = await service.ResolveAsync("""<p><xref:My"Type"></p>""");

        result.ShouldContain("&quot;");
    }

    // --- Normal resolution still works ---

    [Fact]
    public async Task ResolvedXrefTag_NormalTitle_ProducesCleanAnchor()
    {
        var service = CreateService(
            new CrossReference("MyClass", "MyClass", MakeRoute("/api/myclass")));

        var result = await service.ResolveAsync("<p>See <xref:MyClass> here.</p>");

        result.ShouldContain("""<a href="/api/myclass/">MyClass</a>""");
    }

    // --- Href-based xref links (AngleSharp path) still work ---

    [Fact]
    public async Task ResolvedXrefLink_HrefPath_IsResolved()
    {
        var service = CreateService(
            new CrossReference("MyClass", "MyClass", MakeRoute("/api/myclass")));

        var result = await service.ResolveAsync("""<a href="xref:MyClass">xref:MyClass</a>""");

        result.ShouldContain("""href="/api/myclass/""");
        result.ShouldContain("MyClass");
        result.ShouldNotContain("xref:");
    }

    // --- Multiple special characters combined ---

    [Fact]
    public async Task ResolvedXrefTag_MultipleSpecialChars_AllEncoded()
    {
        var service = CreateService(
            new CrossReference("combo", "<Func<T>> & \"More\"", MakeRoute("/api/combo")));

        var result = await service.ResolveAsync("<p><xref:combo></p>");

        result.ShouldContain("&lt;");
        result.ShouldContain("&gt;");
        result.ShouldContain("&amp;");
        result.ShouldContain("&quot;");
    }

    private sealed class StubContentService(ImmutableList<CrossReference> refs) : IContentService
    {
        public IAsyncEnumerable<DiscoveredItem> DiscoverAsync() =>
            AsyncEnumerable.Empty<DiscoveredItem>();

        public Task<ImmutableList<ContentToCopy>> GetContentToCopyAsync() =>
            Task.FromResult(ImmutableList<ContentToCopy>.Empty);

        public Task<ImmutableList<ContentToCreate>> GetContentToCreateAsync() =>
            Task.FromResult(ImmutableList<ContentToCreate>.Empty);

        public Task<ImmutableList<ContentTocItem>> GetContentTocEntriesAsync() =>
            Task.FromResult(ImmutableList<ContentTocItem>.Empty);

        public Task<ImmutableList<CrossReference>> GetCrossReferencesAsync() =>
            Task.FromResult(refs);

        public string DefaultSection => "";
        public int SearchPriority => 0;
    }
}
