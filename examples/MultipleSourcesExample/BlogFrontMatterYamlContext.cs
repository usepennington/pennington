namespace MultipleSourcesExample;

using System.Text.Json.Serialization;
using SharpYaml.Serialization;

/// <summary>
/// Source-generated SharpYaml metadata for this host's custom <see cref="BlogFrontMatter"/>,
/// registered via <c>AddYamlContext</c> so it deserializes without reflection. Types
/// no registered context covers (here, anything but <see cref="BlogFrontMatter"/>) fall back to reflection.
/// </summary>
[YamlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)]
[YamlSerializable(typeof(BlogFrontMatter))]
internal partial class BlogFrontMatterYamlContext : YamlSerializerContext
{
}
