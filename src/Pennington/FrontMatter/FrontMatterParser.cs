namespace Pennington.FrontMatter;

using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Parses YAML front matter from markdown content.
/// </summary>
public sealed class FrontMatterParser
{
    private readonly IDeserializer _deserializer;

    public FrontMatterParser()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>
    /// Parse front matter and return the metadata + remaining markdown body.
    /// Returns null metadata if no front matter block is present.
    /// </summary>
    public FrontMatterResult<T> Parse<T>(string content) where T : IFrontMatter, new()
    {
        if (!TryExtractYaml(content, out var yaml, out var body))
        {
            return new FrontMatterResult<T>(default, content);
        }

        var metadata = SafeDeserialize<T>(yaml) ?? new T();
        return new FrontMatterResult<T>(metadata, body);
    }

    /// <summary>
    /// Deserialize raw YAML content (no --- delimiters) into a front matter type.
    /// Used for sidecar metadata files.
    /// </summary>
    public T DeserializeYaml<T>(string yaml) where T : IFrontMatter, new()
        => SafeDeserialize<T>(yaml) ?? new T();

    private T? SafeDeserialize<T>(string yaml)
    {
        var parser = new SafeYamlParser(new Parser(new StringReader(yaml)));
        return _deserializer.Deserialize<T>(parser);
    }

    /// <summary>
    /// Try to extract the YAML block between --- delimiters.
    /// Returns true if front matter was found.
    /// </summary>
    private static bool TryExtractYaml(string content, out string yaml, out string body)
    {
        yaml = "";
        body = content;

        if (string.IsNullOrEmpty(content))
            return false;

        var lines = content.Split('\n');

        // First line must be ---
        if (lines.Length == 0 || lines[0].Trim() != "---")
            return false;

        // Find the closing ---
        for (var i = 1; i < lines.Length; i++)
        {
            if (lines[i].Trim() == "---")
            {
                yaml = string.Join('\n', lines[1..i]);
                body = string.Join('\n', lines[(i + 1)..]).TrimStart('\n', '\r');
                return true;
            }
        }

        return false; // No closing delimiter
    }
}

/// <summary>
/// Result of front matter parsing.
/// </summary>
public record FrontMatterResult<T>(T? Metadata, string Body) where T : IFrontMatter;
