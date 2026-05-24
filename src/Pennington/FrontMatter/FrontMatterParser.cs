namespace Pennington.FrontMatter;

using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Parses YAML front matter from markdown content.
/// </summary>
public sealed class FrontMatterParser
{
    private const string DiagnosticSource = "FrontMatterParser";

    private readonly FrontMatterParserOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDeserializer _lenientDeserializer;
    private readonly IDeserializer _strictDeserializer;
    private readonly ConcurrentDictionary<Type, FrozenSet<string>> _knownKeyCache = new();

    /// <summary>
    /// Initializes the parser with a camelCase YAML deserializer that matches keys case-insensitively.
    /// In lenient mode (the default outside build) unknown keys are dropped silently after a warning is emitted;
    /// in strict mode unknown keys also throw a <see cref="YamlException"/>.
    /// </summary>
    /// <param name="options">Parser options controlling strict-mode behavior.</param>
    /// <param name="httpContextAccessor">Used to resolve the request-scoped <see cref="DiagnosticContext"/>.</param>
    public FrontMatterParser(FrontMatterParserOptions options, IHttpContextAccessor httpContextAccessor)
    {
        _options = options;
        _httpContextAccessor = httpContextAccessor;
        _lenientDeserializer = BuildDeserializer(strict: false);
        _strictDeserializer = BuildDeserializer(strict: true);
    }

    /// <summary>
    /// Convenience constructor for direct instantiation (tests, scripts) that defaults to
    /// lenient mode and emits no diagnostics. Production hosts should resolve the parser
    /// from DI so the configured <see cref="FrontMatterParserOptions"/> apply.
    /// </summary>
    public FrontMatterParser() : this(new FrontMatterParserOptions(), NullHttpContextAccessor.Instance) { }

    private sealed class NullHttpContextAccessor : IHttpContextAccessor
    {
        public static readonly NullHttpContextAccessor Instance = new();
        public HttpContext? HttpContext { get => null; set { } }
    }

    private static IDeserializer BuildDeserializer(bool strict)
    {
        var builder = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithCaseInsensitivePropertyMatching();
        if (!strict)
        {
            builder = builder.IgnoreUnmatchedProperties();
        }

        return builder.Build();
    }

    /// <summary>
    /// Parse front matter and return the metadata + remaining markdown body.
    /// Returns null metadata if no front matter block is present.
    /// </summary>
    /// <param name="content">Markdown content with optional <c>---</c>-delimited front-matter block.</param>
    /// <param name="sourcePath">Source file path used in diagnostic messages. Optional.</param>
    /// <param name="diagnostics">
    /// Diagnostic context to receive unknown-key warnings. When omitted, the parser falls back
    /// to the per-request <see cref="DiagnosticContext"/> from <see cref="IHttpContextAccessor"/>.
    /// </param>
    public FrontMatterResult<T> Parse<T>(string content, string? sourcePath = null, DiagnosticContext? diagnostics = null)
        where T : IFrontMatter, new()
    {
        if (!TryExtractYaml(content, out var yaml, out var body))
        {
            return new FrontMatterResult<T>(default, content);
        }

        diagnostics ??= ResolveAmbientDiagnostics();
        var metadata = DeserializeWithScan<T>(yaml, lineOffset: 1, sourcePath, diagnostics) ?? new T();
        return new FrontMatterResult<T>(metadata, body);
    }

    /// <summary>
    /// Deserialize raw YAML content (no <c>---</c> delimiters) into a front matter type.
    /// Used for sidecar metadata files.
    /// </summary>
    /// <param name="yaml">Raw YAML text.</param>
    /// <param name="sourcePath">Source file path used in diagnostic messages. Optional.</param>
    /// <param name="diagnostics">
    /// Diagnostic context to receive unknown-key warnings. When omitted, the parser falls back
    /// to the per-request <see cref="DiagnosticContext"/> from <see cref="IHttpContextAccessor"/>.
    /// </param>
    public T DeserializeYaml<T>(string yaml, string? sourcePath = null, DiagnosticContext? diagnostics = null)
        where T : IFrontMatter, new()
    {
        diagnostics ??= ResolveAmbientDiagnostics();
        return DeserializeWithScan<T>(yaml, lineOffset: 0, sourcePath, diagnostics) ?? new T();
    }

    private T? SafeDeserialize<T>(string yaml)
    {
        var parser = new SafeYamlParser(new Parser(new StringReader(yaml)));
        var deserializer = _options.StrictUnknownKeys ? _strictDeserializer : _lenientDeserializer;
        return deserializer.Deserialize<T>(parser);
    }

    // When diagnostics are inactive there is nothing to scan, so deserialize directly
    // (one character scan). When they are active, tokenize once into a buffer and replay
    // it for both the unknown-key scan and the deserialize, instead of scanning twice.
    private T? DeserializeWithScan<T>(string yaml, int lineOffset, string? sourcePath, DiagnosticContext? diagnostics)
        where T : IFrontMatter, new()
    {
        if (diagnostics is null || string.IsNullOrWhiteSpace(yaml))
        {
            return SafeDeserialize<T>(yaml);
        }

        List<ParsingEvent> events;
        try
        {
            events = BufferEvents(yaml);
        }
        catch (YamlException)
        {
            // Malformed or security-rejected YAML — fall back to a direct deserialize so
            // it surfaces the canonical error (matching the prior scan-then-deserialize flow).
            return SafeDeserialize<T>(yaml);
        }

        ScanUnknownKeys<T>(events, lineOffset, sourcePath, diagnostics);

        var deserializer = _options.StrictUnknownKeys ? _strictDeserializer : _lenientDeserializer;
        return deserializer.Deserialize<T>(new BufferedYamlParser(events));
    }

    // Drains a single SafeYamlParser pass into a replayable event buffer. The security
    // checks (anchors/aliases/tags) run here, once, and a YamlException propagates to the caller.
    private static List<ParsingEvent> BufferEvents(string yaml)
    {
        var events = new List<ParsingEvent>();
        var source = new SafeYamlParser(new Parser(new StringReader(yaml)));
        while (source.MoveNext())
        {
            events.Add(source.Current!);
        }

        return events;
    }

    private DiagnosticContext? ResolveAmbientDiagnostics()
        => _httpContextAccessor.HttpContext?.RequestServices.GetService<DiagnosticContext>();

    private void ScanUnknownKeys<T>(IReadOnlyList<ParsingEvent> events, int lineOffset, string? sourcePath, DiagnosticContext diagnostics)
    {
        YamlStream stream;
        try
        {
            stream = new YamlStream();
            stream.Load(new BufferedYamlParser(events));
        }
        catch (YamlException)
        {
            // Malformed YAML — let the deserialize step surface the canonical error
            // rather than emitting a partial unknown-key list here.
            return;
        }

        if (stream.Documents.Count == 0 || stream.Documents[0].RootNode is not YamlMappingNode mapping)
        {
            return;
        }

        var known = _knownKeyCache.GetOrAdd(typeof(T), BuildKnownKeySet);
        var location = sourcePath ?? "<unknown>";

        foreach (var entry in mapping.Children)
        {
            if (entry.Key is not YamlScalarNode keyNode || keyNode.Value is null)
            {
                continue;
            }

            var keyName = keyNode.Value;
            if (known.Contains(keyName))
            {
                continue;
            }

            var line = keyNode.Start.Line + lineOffset;
            diagnostics.AddWarning(
                $"Unknown front-matter key '{keyName}' in {location}:{line}",
                DiagnosticSource);
        }
    }

    private static FrozenSet<string> BuildKnownKeySet(Type t)
    {
        // Mirror what WithNamingConvention(CamelCase) + WithCaseInsensitivePropertyMatching
        // accept on the deserializer. Include declared and interface-default members from
        // both T and any IFrontMatter capability mixins T implements.
        var convention = CamelCaseNamingConvention.Instance;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            seen.Add(convention.Apply(prop.Name));
        }

        foreach (var iface in t.GetInterfaces())
        {
            foreach (var prop in iface.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                seen.Add(convention.Apply(prop.Name));
            }
        }

        return seen.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
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
        {
            return false;
        }

        var lines = content.Split('\n');

        // First line must be ---
        if (lines.Length == 0 || lines[0].Trim() != "---")
        {
            return false;
        }

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
/// <param name="Metadata">Deserialized front matter, or <c>null</c> when the content had no front matter block.</param>
/// <param name="Body">Markdown body with the front matter block stripped.</param>
public record FrontMatterResult<T>(T? Metadata, string Body) where T : IFrontMatter;