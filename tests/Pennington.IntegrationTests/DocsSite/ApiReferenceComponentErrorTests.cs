namespace Pennington.IntegrationTests.DocsSite;

using System.Collections.Immutable;
using Markdig.Renderers;
using Mdazor;
using Microsoft.Extensions.DependencyInjection;
using Pennington.ApiMetadata;
using Pennington.DocSite.Api.Components.Reference;
using Pennington.Highlighting;
using Pennington.Markdown;
using Pennington.Markdown.Extensions;

/// <summary>
/// Regression coverage for F102: every API-reference component routes its provider call through
/// <c>ApiReferenceComponentBase.Fetch</c>, so a throwing provider now renders an inline <c>diag-error</c>
/// instead of letting the exception escape the render. Before the consolidation, ApiRemarks/ApiReturns/
/// ApiSeeAlso/ExtensionMethods had no try/catch and a provider failure became an unhandled 500.
/// </summary>
public sealed class ApiReferenceComponentErrorTests
{
    [Theory]
    [InlineData("<ApiSummary XmlDocId=\"T:Foo\" />")]
    [InlineData("<ApiRemarks XmlDocId=\"T:Foo\" />")]
    [InlineData("<ApiReturns XmlDocId=\"T:Foo\" />")]
    [InlineData("<ApiSeeAlso XmlDocId=\"T:Foo\" />")]
    [InlineData("<ExtensionMethods Receiver=\"Foo\" />")]
    public void Component_RendersDiagError_WhenProviderThrows(string markup)
    {
        // Must not throw — the guarded fetch turns the provider exception into inline error HTML.
        var html = Render(markup, new ThrowingApiMetadataProvider());

        html.ShouldContain("diag-error");
    }

    [Fact]
    public void ApiSeeAlso_RendersList_WhenProviderSucceeds()
    {
        var xmldoc = ParsedXmlDoc.Empty with { SeeAlso = ["T:Some.Other.Type"] };

        var html = Render("<ApiSeeAlso XmlDocId=\"T:Foo\" />", new StubApiMetadataProvider(xmldoc));

        html.ShouldContain("<ul>");
        html.ShouldContain("</code>");
        html.ShouldNotContain("diag-error");
    }

    private static string Render(string markdown, IApiMetadataProvider provider)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMdazor();
        services.AddMdazorComponent<ApiSummary>()
                .AddMdazorComponent<ApiRemarks>()
                .AddMdazorComponent<ApiReturns>()
                .AddMdazorComponent<ApiSeeAlso>()
                .AddMdazorComponent<ExtensionMethods>();
        services.AddHttpContextAccessor();
        services.AddSingleton<IXmlDocHtmlRenderer, XmlDocHtmlRenderer>();
        services.AddKeyedSingleton<IApiMetadataProvider>("default", (_, _) => provider);
        using var sp = services.BuildServiceProvider();

        var pipeline = MarkdownPipelineFactory.CreateWithExtensions(
            sp, new CodeBlockRenderingService(new HighlightingService([])));

        using var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        pipeline.Setup(renderer);
        var document = Markdig.Markdown.Parse(markdown, pipeline);
        renderer.Render(document);
        writer.Flush();
        return writer.ToString();
    }

    private sealed class ThrowingApiMetadataProvider : IApiMetadataProvider
    {
        public Task<ImmutableArray<ApiTypeSummary>> GetTypesAsync() => throw new InvalidOperationException("boom");
        public Task<ApiTypeDetail?> GetTypeAsync(string uid) => throw new InvalidOperationException("boom");
        public Task<ImmutableArray<ApiMember>> GetMembersAsync(string typeUid, MemberKind kind, AccessFilter access, MemberOrder order) => throw new InvalidOperationException("boom");
        public Task<ImmutableArray<ExtensionMethodEntry>> GetExtensionMethodsForAsync(string receiverTypeName) => throw new InvalidOperationException("boom");
        public Task<ParsedXmlDoc> GetXmldocAsync(string uid) => throw new InvalidOperationException("boom");
        public Task<ApiMember?> GetMemberAsync(string uid) => throw new InvalidOperationException("boom");
    }

    private sealed class StubApiMetadataProvider(ParsedXmlDoc xmldoc) : IApiMetadataProvider
    {
        public Task<ImmutableArray<ApiTypeSummary>> GetTypesAsync() => Task.FromResult(ImmutableArray<ApiTypeSummary>.Empty);
        public Task<ApiTypeDetail?> GetTypeAsync(string uid) => Task.FromResult<ApiTypeDetail?>(null);
        public Task<ImmutableArray<ApiMember>> GetMembersAsync(string typeUid, MemberKind kind, AccessFilter access, MemberOrder order) => Task.FromResult(ImmutableArray<ApiMember>.Empty);
        public Task<ImmutableArray<ExtensionMethodEntry>> GetExtensionMethodsForAsync(string receiverTypeName) => Task.FromResult(ImmutableArray<ExtensionMethodEntry>.Empty);
        public Task<ParsedXmlDoc> GetXmldocAsync(string uid) => Task.FromResult(xmldoc);
        public Task<ApiMember?> GetMemberAsync(string uid) => Task.FromResult<ApiMember?>(null);
    }
}
