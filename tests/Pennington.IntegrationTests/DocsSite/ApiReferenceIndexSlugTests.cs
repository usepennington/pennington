namespace Pennington.IntegrationTests.DocsSite;

using System.Collections.Immutable;
using Microsoft.Extensions.Logging.Abstractions;
using Pennington.ApiMetadata;
using Pennington.DocSite.Api;

public sealed class ApiReferenceIndexSlugTests
{
    [Theory]
    [InlineData("FusionCache", "fusion-cache")]
    [InlineData("FusionCacheBuilderExtMethods", "fusion-cache-builder-ext-methods")]
    [InlineData("CacheKeyModifierMode", "cache-key-modifier-mode")]
    // Acronym runs stay glued together — the split rule only inserts a hyphen
    // before an upper-case letter when its neighbour is lower-case.
    [InlineData("FusionCacheXMLOptions", "fusion-cache-xml-options")]
    [InlineData("IOStream", "io-stream")]
    [InlineData("HTTPSConfig", "https-config")]
    [InlineData("ParseURL", "parse-url")]
    [InlineData("XML", "xml")]
    [InlineData("URLBuilder", "url-builder")]
    public void ToSlug_preserves_acronym_runs(string typeName, string expected)
    {
        ApiReferenceIndex.ToSlug(typeName).ShouldBe(expected);
    }

    [Theory]
    [InlineData("List<T>", "listt")]
    [InlineData("Dictionary<TKey, TValue>", "dictionaryt-keyt-value")]
    public void ToSlug_strips_generic_markers(string typeName, string expected)
    {
        ApiReferenceIndex.ToSlug(typeName).ShouldBe(expected);
    }

    // A non-generic type and its generic`N counterpart share both name and
    // namespace, so namespace qualification can never separate them. Folding the
    // arity (from the uid) into the slug keeps them distinct — and, crucially,
    // stops the index from throwing a duplicate-key exception while building.
    [Fact]
    public async Task Generic_and_nongeneric_twins_get_distinct_slugs()
    {
        var index = new ApiReferenceIndex(
            new StubProvider(
                Type("T:SixLabors.ImageSharp.Advanced.IRowIntervalOperation", "IRowIntervalOperation", "SixLabors.ImageSharp.Advanced"),
                Type("T:SixLabors.ImageSharp.Advanced.IRowIntervalOperation`1", "IRowIntervalOperation", "SixLabors.ImageSharp.Advanced")),
            "imagesharp",
            NullLogger<ApiReferenceIndex>.Instance);

        var entries = await index.GetEntriesAsync();

        // The non-generic keeps the clean slug; the arity-1 twin takes a -1 suffix.
        // No namespace qualification leaks in, and there is no duplicate-key throw.
        SlugFor(entries, "T:SixLabors.ImageSharp.Advanced.IRowIntervalOperation").ShouldBe("i-row-interval-operation");
        SlugFor(entries, "T:SixLabors.ImageSharp.Advanced.IRowIntervalOperation`1").ShouldBe("i-row-interval-operation-1");
    }

    [Fact]
    public async Task Multiple_arities_of_one_name_each_get_their_own_slug()
    {
        var index = new ApiReferenceIndex(
            new StubProvider(
                Type("T:N.Image", "Image", "N"),
                Type("T:N.Image`1", "Image", "N"),
                Type("T:N.Buffer`2", "Buffer", "N")),
            "lib",
            NullLogger<ApiReferenceIndex>.Instance);

        var entries = await index.GetEntriesAsync();

        SlugFor(entries, "T:N.Image").ShouldBe("image");
        SlugFor(entries, "T:N.Image`1").ShouldBe("image-1");
        SlugFor(entries, "T:N.Buffer`2").ShouldBe("buffer-2");
    }

    private static string SlugFor(ImmutableDictionary<string, ApiReferenceEntry> entries, string uid) =>
        entries.Values.Single(e => e.Uid == uid).Slug;

    private static ApiTypeSummary Type(string uid, string name, string ns) =>
        new(uid, name, ns, Assembly: "TestAsm", Kind: ApiTypeKind.Interface, Summary: null);

    private sealed class StubProvider(params ApiTypeSummary[] types) : IApiMetadataProvider
    {
        public Task<ImmutableArray<ApiTypeSummary>> GetTypesAsync() => Task.FromResult(types.ToImmutableArray());
        public Task<ApiTypeDetail?> GetTypeAsync(string uid) => Task.FromResult<ApiTypeDetail?>(null);
        public Task<ImmutableArray<ApiMember>> GetMembersAsync(string typeUid, MemberKind kind, AccessFilter access, MemberOrder order) => Task.FromResult(ImmutableArray<ApiMember>.Empty);
        public Task<ImmutableArray<ExtensionMethodEntry>> GetExtensionMethodsForAsync(string receiverTypeName) => Task.FromResult(ImmutableArray<ExtensionMethodEntry>.Empty);
        public Task<ParsedXmlDoc> GetXmldocAsync(string uid) => Task.FromResult(ParsedXmlDoc.Empty);
        public Task<ApiMember?> GetMemberAsync(string uid) => Task.FromResult<ApiMember?>(null);
    }
}
