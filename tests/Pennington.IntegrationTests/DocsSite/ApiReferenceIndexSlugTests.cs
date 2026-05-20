namespace Pennington.IntegrationTests.DocsSite;

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
}