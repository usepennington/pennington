namespace Pennington.Data;

using System.IO.Abstractions;
using System.Text.Json;
using YamlDotNet.RepresentationModel;
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

    /// <summary>
    /// Reads <paramref name="absolutePath"/> through <paramref name="fileSystem"/> and deserializes
    /// it into a list of <typeparamref name="TItem"/>. A file whose root is an array yields its
    /// elements; a file whose root is a single record yields a one-element list.
    /// </summary>
    /// <exception cref="FileNotFoundException">The file does not exist.</exception>
    /// <exception cref="NotSupportedException">The file extension is not <c>.yml</c>, <c>.yaml</c>, or <c>.json</c>.</exception>
    public static IReadOnlyList<TItem> LoadMany<TItem>(string absolutePath, IFileSystem fileSystem)
    {
        if (!fileSystem.File.Exists(absolutePath))
            throw new FileNotFoundException($"Data file not found: {absolutePath}", absolutePath);

        var content = fileSystem.File.ReadAllText(absolutePath);
        var ext = Path.GetExtension(absolutePath).ToLowerInvariant();

        return ext switch
        {
            ".yml" or ".yaml" => LoadManyYaml<TItem>(content, absolutePath),
            ".json" => LoadManyJson<TItem>(content, absolutePath),
            _ => throw new NotSupportedException(
                $"Unsupported data file extension '{ext}' for {absolutePath}. Use .yml, .yaml, or .json."),
        };
    }

    private static IReadOnlyList<TItem> LoadManyYaml<TItem>(string content, string path)
    {
        if (RootIsYamlSequence(content, path))
            return DeserializeYaml<List<TItem>>(content, path);

        return [DeserializeYaml<TItem>(content, path)];
    }

    private static bool RootIsYamlSequence(string content, string path)
    {
        try
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(content));
            return stream.Documents.Count > 0 && stream.Documents[0].RootNode is YamlSequenceNode;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Failed to parse YAML data file {path}: {ex.Message}", ex);
        }
    }

    private static IReadOnlyList<TItem> LoadManyJson<TItem>(string content, string path)
    {
        bool rootIsArray;
        try
        {
            using var document = JsonDocument.Parse(content, new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            });
            rootIsArray = document.RootElement.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Failed to parse JSON data file {path}: {ex.Message}", ex);
        }

        return rootIsArray
            ? DeserializeJson<List<TItem>>(content, path)
            : [DeserializeJson<TItem>(content, path)];
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
