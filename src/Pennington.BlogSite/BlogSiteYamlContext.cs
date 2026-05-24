namespace Pennington.BlogSite;

using System.Text.Json.Serialization;
using SharpYaml.Serialization;

/// <summary>
/// Source-generated SharpYaml metadata for BlogSite's front-matter record, registered by
/// <see cref="BlogSiteServiceExtensions.AddBlogSite"/> so it deserializes without reflection
/// (NativeAOT/trim-friendly).
/// </summary>
[YamlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)]
[YamlSerializable(typeof(BlogSiteFrontMatter))]
internal partial class BlogSiteYamlContext : YamlSerializerContext
{
}
