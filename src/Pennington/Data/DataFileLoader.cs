namespace Pennington.Data;

using System.IO.Abstractions;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Deserializes a data file's bytes into a typed value. Format is chosen from the
/// extension (<c>.yml</c> / <c>.yaml</c> use YamlDotNet, <c>.json</c> uses System.Text.Json),
/// both configured with camelCase naming and case-insensitive property matching to mirror
/// <see cref="FrontMatter.FrontMatterParser"/>'s behavior.
/// </summary>
public static class DataFileLoader
{
    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithCaseInsensitivePropertyMatching()
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    /// <summary>
    /// Reads <paramref name="absolutePath"/> through <paramref name="fileSystem"/> and deserializes
    /// it into <typeparamref name="T"/>.
    /// </summary>
    /// <exception cref="FileNotFoundException">The file does not exist.</exception>
    /// <exception cref="NotSupportedException">The file extension is not <c>.yml</c>, <c>.yaml</c>, or <c>.json</c>.</exception>
    public static T Load<T>(string absolutePath, IFileSystem fileSystem)
    {
        if (!fileSystem.File.Exists(absolutePath))
            throw new FileNotFoundException($"Data file not found: {absolutePath}", absolutePath);

        var content = fileSystem.File.ReadAllText(absolutePath);
        var ext = Path.GetExtension(absolutePath).ToLowerInvariant();

        return ext switch
        {
            ".yml" or ".yaml" => DeserializeYaml<T>(content, absolutePath),
            ".json" => DeserializeJson<T>(content, absolutePath),
            _ => throw new NotSupportedException(
                $"Unsupported data file extension '{ext}' for {absolutePath}. Use .yml, .yaml, or .json."),
        };
    }

    private static T DeserializeYaml<T>(string content, string path)
    {
        try
        {
            return YamlDeserializer.Deserialize<T>(content)
                ?? throw new InvalidDataException($"YAML in {path} deserialized to null");
        }
        catch (Exception ex) when (ex is not InvalidDataException)
        {
            throw new InvalidDataException($"Failed to deserialize YAML data file {path}: {ex.Message}", ex);
        }
    }

    private static T DeserializeJson<T>(string content, string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(content, JsonOptions)
                ?? throw new InvalidDataException($"JSON in {path} deserialized to null");
        }
        catch (Exception ex) when (ex is not InvalidDataException)
        {
            throw new InvalidDataException($"Failed to deserialize JSON data file {path}: {ex.Message}", ex);
        }
    }
}
