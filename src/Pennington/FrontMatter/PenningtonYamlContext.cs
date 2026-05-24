namespace Pennington.FrontMatter;

using System.Text.Json.Serialization;
using SharpYaml.Serialization;

/// <summary>
/// Source-generated SharpYaml metadata for Pennington's built-in front-matter records, so they
/// deserialize without reflection (NativeAOT/trim-friendly). Registered automatically by
/// <see cref="Infrastructure.PenningtonExtensions.AddPennington"/>. Types not covered by any
/// registered context fall back to reflection — see <see cref="PenningtonYamlContextProvider"/>.
/// </summary>
[YamlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true)]
[YamlSerializable(typeof(DocFrontMatter))]
[YamlSerializable(typeof(BlogFrontMatter))]
internal partial class PenningtonYamlContext : YamlSerializerContext
{
}
