namespace Pennington.FrontMatter;

using System.Text.Json;
using SharpYaml;

/// <summary>
/// Shared SharpYaml configuration. <see cref="ReflectionOptions"/> mirrors the front-matter
/// conventions (camelCase keys, case-insensitive matching) and pins an explicit
/// <see cref="ReflectionYamlTypeInfoResolver"/> so reflection-based deserialization works
/// regardless of SharpYaml's <c>IsReflectionEnabledByDefault</c> switch — this is the
/// reflection fallback used for any type no source-generated context covers.
/// </summary>
internal static class PenningtonYaml
{
    /// <summary>Reflection-backed options used for types not registered with a serializer context.</summary>
    public static YamlSerializerOptions ReflectionOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = ReflectionYamlTypeInfoResolver.Default,
    };
}
