namespace Pennington.DocSite;

using System.Text.Json.Serialization;
using SharpYaml.Serialization;

/// <summary>
/// Source-generated SharpYaml metadata for DocSite's front-matter records, registered by
/// <see cref="DocSiteServiceExtensions.AddDocSite"/> so they deserialize without reflection
/// (NativeAOT/trim-friendly).
/// </summary>
[YamlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)]
[YamlSerializable(typeof(DocSiteFrontMatter))]
[YamlSerializable(typeof(BlogPostFrontMatter))]
internal partial class DocSiteYamlContext : YamlSerializerContext
{
}
