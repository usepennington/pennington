namespace Pennington.Tests.ApiMetadata.Reflection;

using System.IO;
using System.Linq;
using Pennington.ApiMetadata;
using Pennington.ApiMetadata.Reflection;
using Pennington.Highlighting;
using Shouldly;

/// <summary>
/// Exercises the reflection-backed API provider against Pennington's own built assembly, so the
/// behaviors the docs site depends on — union cases, inheritdoc resolution, extension-method
/// cross-references, const defaults, and implicit-member exclusion — are validated end-to-end
/// over real metadata + xmldoc.
/// </summary>
public sealed class CompiledAssemblyApiMetadataProviderTests
{
    private static CompiledAssemblyApiMetadataProvider CreateProvider()
    {
        var options = new CompiledAssemblyApiOptions();
        options.AssemblyFiles.Add(Path.Combine(AppContext.BaseDirectory, "Pennington.dll"));
        return new CompiledAssemblyApiMetadataProvider(options, new XmlDocParser(), new PlainTextHighlighter());
    }

    [Fact]
    public async Task Enumerates_union_cases_for_a_union_type()
    {
        var provider = CreateProvider();

        var cases = await provider.GetMembersAsync(
            "T:Pennington.Pipeline.ContentItem", MemberKind.UnionCases, AccessFilter.Public, MemberOrder.Declaration);

        cases.ShouldAllBe(m => m.Kind == MemberKind.UnionCases);
        var names = cases.Select(m => m.Name).ToList();
        names.ShouldContain("DiscoveredItem");
        names.ShouldContain("ParsedItem");
        names.ShouldContain("RenderedItem");
        names.ShouldContain("FailedItem");
    }

    [Fact]
    public async Task Resolves_inheritdoc_from_an_implemented_interface()
    {
        var provider = CreateProvider();

        // PlainTextHighlighter.Highlight is `/// <inheritdoc/>`; the summary lives on
        // ICodeHighlighter.Highlight. Without resolution the member would have no summary.
        var members = await provider.GetMembersAsync(
            "T:Pennington.Highlighting.PlainTextHighlighter", MemberKind.Methods, AccessFilter.Public, MemberOrder.Declaration);

        var highlight = members.First(m => m.Name == "Highlight");
        highlight.HasInheritDocDirective.ShouldBeTrue();
        highlight.Xmldoc.HasSummary.ShouldBeTrue();
    }

    [Fact]
    public async Task Groups_extension_methods_by_receiver_type_name()
    {
        var provider = CreateProvider();

        var forServices = await provider.GetExtensionMethodsForAsync("IServiceCollection");

        forServices.ShouldNotBeEmpty();
        forServices.ShouldContain(e => e.Name == "AddPennington");
        forServices.ShouldAllBe(e => e.ReceiverTypeName == "IServiceCollection");
    }

    [Fact]
    public async Task Surfaces_const_field_default_values()
    {
        var provider = CreateProvider();

        var key = await provider.GetMemberAsync("F:Pennington.Pipeline.ReadingTimeEnricher.Key");

        key.ShouldNotBeNull();
        key.Kind.ShouldBe(MemberKind.Fields);
        key.DefaultValue.ShouldBe("\"reading_time_minutes\"");
    }

    [Fact]
    public async Task Documents_public_nested_types()
    {
        var provider = CreateProvider();

        // MarkdownFile is a public record nested in LlmsTxtService. Nested public types
        // carrying their own xmldoc must be documented, not silently dropped.
        var types = await provider.GetTypesAsync();

        types.ShouldContain(t => t.FullTypeName.Contains("MarkdownFile", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Excludes_operators_from_the_method_list()
    {
        var provider = CreateProvider();

        // UrlPath defines implicit string operators; the member table lists only ordinary methods,
        // so the reflection provider must drop op_* members too.
        var methods = await provider.GetMembersAsync(
            "T:Pennington.Routing.UrlPath", MemberKind.Methods, AccessFilter.Public, MemberOrder.Declaration);

        methods.ShouldNotBeEmpty();
        methods.ShouldNotContain(m => m.Name.StartsWith("op_", StringComparison.Ordinal));
    }
}
