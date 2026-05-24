namespace Pennington.FrontMatter;

using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using System.Text.Json;
using Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SharpYaml;
using SharpYaml.Events;

/// <summary>
/// Parses YAML front matter from markdown content.
/// </summary>
public sealed class FrontMatterParser
{
    private const string DiagnosticSource = "FrontMatterParser";

    // Standard YAML tags permitted in front matter. Any other (custom/local) tag is rejected,
    // along with anchors and aliases, to block billion-laughs expansion and arbitrary type
    // instantiation. SharpYaml's serializer does not reject these on its own, so a single
    // event pass enforces the policy before deserialization.
    private static readonly FrozenSet<string> AllowedTags = new[]
    {
        "tag:yaml.org,2002:str",
        "tag:yaml.org,2002:int",
        "tag:yaml.org,2002:float",
        "tag:yaml.org,2002:bool",
        "tag:yaml.org,2002:null",
        "tag:yaml.org,2002:seq",
        "tag:yaml.org,2002:map",
        "tag:yaml.org,2002:timestamp",
    }.ToFrozenSet(StringComparer.Ordinal);

    private readonly FrontMatterParserOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly PenningtonYamlContextProvider _yaml;
    private readonly ConcurrentDictionary<Type, FrozenSet<string>> _knownKeyCache = new();

    /// <summary>
    /// Initializes the parser. Built-in front-matter types deserialize through the source-generated
    /// <see cref="PenningtonYamlContext"/>; other types fall back to reflection. Keys are camelCase
    /// matched case-insensitively. In lenient mode (the default outside build) unknown keys are
    /// dropped after a warning; in strict mode they additionally throw a <see cref="YamlException"/>.
    /// </summary>
    /// <param name="options">Parser options controlling strict-mode behavior.</param>
    /// <param name="httpContextAccessor">Used to resolve the request-scoped <see cref="DiagnosticContext"/>.</param>
    /// <param name="yaml">Supplies source-generated contexts with reflection fallback for deserialization.</param>
    public FrontMatterParser(
        FrontMatterParserOptions options,
        IHttpContextAccessor httpContextAccessor,
        PenningtonYamlContextProvider yaml)
    {
        _options = options;
        _httpContextAccessor = httpContextAccessor;
        _yaml = yaml;
    }

    /// <summary>
    /// Convenience constructor for direct instantiation (tests, scripts) that defaults to
    /// lenient mode, emits no diagnostics, and uses only the built-in serializer context.
    /// Production hosts should resolve the parser from DI so the configured
    /// <see cref="FrontMatterParserOptions"/> and any registered contexts apply.
    /// </summary>
    public FrontMatterParser()
        : this(new FrontMatterParserOptions(), NullHttpContextAccessor.Instance, PenningtonYamlContextProvider.Default) { }

    private sealed class NullHttpContextAccessor : IHttpContextAccessor
    {
        public static readonly NullHttpContextAccessor Instance = new();
        public HttpContext? HttpContext { get => null; set { } }
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

    // A single event pass enforces the security policy (always) and, when diagnostics are active
    // or strict mode is on, collects the top-level keys so unknown ones can be reported. The
    // deserialize itself reads the text again (SharpYaml has no parse-from-events overload); for
    // tiny front-matter blocks the second parse is negligible.
    private T? DeserializeWithScan<T>(string yaml, int lineOffset, string? sourcePath, DiagnosticContext? diagnostics)
        where T : IFrontMatter, new()
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return default;
        }

        var collectKeys = diagnostics is not null || _options.StrictUnknownKeys;
        var keys = ScanYaml(yaml, lineOffset, collectKeys, out var malformed);

        if (!malformed && keys is not null)
        {
            ReportUnknownKeys<T>(keys, sourcePath, diagnostics);
        }

        return _yaml.Deserialize<T>(yaml);
    }

    private void ReportUnknownKeys<T>(List<(string Name, int Line)> keys, string? sourcePath, DiagnosticContext? diagnostics)
        where T : IFrontMatter, new()
    {
        var known = _knownKeyCache.GetOrAdd(typeof(T), BuildKnownKeySet);
        var location = sourcePath ?? "<unknown>";
        var unknownFound = false;

        foreach (var (name, line) in keys)
        {
            if (known.Contains(name))
            {
                continue;
            }

            unknownFound = true;
            diagnostics?.AddWarning($"Unknown front-matter key '{name}' in {location}:{line}", DiagnosticSource);
        }

        // Strict mode throws after the warnings are emitted, matching the prior scan-then-throw flow.
        if (unknownFound && _options.StrictUnknownKeys)
        {
            throw new YamlException($"Unknown front-matter key(s) in {location}.");
        }
    }

    // One linear pass over the YAML events. Enforces the security policy on every event and,
    // when requested, records the root mapping's keys with 1-based line numbers (SharpYaml marks
    // are 0-based). A parser failure means malformed YAML — bail and let the deserialize step
    // surface the canonical error; a security violation throws immediately.
    private List<(string Name, int Line)>? ScanYaml(string yaml, int lineOffset, bool collectKeys, out bool malformed)
    {
        malformed = false;
        var keys = collectKeys ? new List<(string, int)>() : null;
        var parser = Parser.CreateParser(new StringReader(yaml));

        var depth = 0;
        var rootIsMapping = false;
        var rootExpectKey = false;

        while (true)
        {
            bool moved;
            try
            {
                moved = parser.MoveNext();
            }
            catch (YamlException)
            {
                malformed = true;
                return null;
            }

            if (!moved)
            {
                break;
            }

            var current = parser.Current!;
            EnforceSecurity(current);

            // Root-mapping children alternate key/value. A key is recorded only when we are
            // directly inside the root mapping (depth == 1) and expecting a key.
            if (collectKeys && rootIsMapping && depth == 1 && current is Scalar or MappingStart or SequenceStart)
            {
                if (rootExpectKey)
                {
                    if (current is Scalar { Value: { } value })
                    {
                        keys!.Add((value, current.Start.Line + 1 + lineOffset));
                    }

                    rootExpectKey = false;
                }
                else
                {
                    rootExpectKey = true;
                }
            }

            switch (current)
            {
                case MappingStart:
                    if (depth == 0)
                    {
                        rootIsMapping = true;
                        rootExpectKey = true;
                    }

                    depth++;
                    break;
                case SequenceStart:
                    depth++;
                    break;
                case MappingEnd:
                case SequenceEnd:
                    depth--;
                    break;
            }
        }

        return keys;
    }

    // Rejects YAML anchors, aliases, and non-standard type tags — preventing billion-laughs
    // expansion and arbitrary type instantiation in front matter.
    private static void EnforceSecurity(ParsingEvent current)
    {
        switch (current)
        {
            case AnchorAlias alias:
                throw new YamlException(alias.Start, alias.End,
                    "YAML aliases are not permitted in front matter.");

            case NodeEvent { Anchor: { Length: > 0 } } node:
                throw new YamlException(node.Start, node.End,
                    "YAML anchors are not permitted in front matter.");

            case NodeEvent node when node.Tag is { Length: > 0 } tag && !AllowedTags.Contains(tag):
                throw new YamlException(node.Start, node.End,
                    $"YAML type tags are not permitted in front matter. Tag: {tag}");
        }
    }

    private DiagnosticContext? ResolveAmbientDiagnostics()
        => _httpContextAccessor.HttpContext?.RequestServices.GetService<DiagnosticContext>();

    private static FrozenSet<string> BuildKnownKeySet(Type t)
    {
        // Mirror PropertyNamingPolicy.CamelCase + case-insensitive matching on the deserializer.
        // Include declared and interface-default members from both T and any IFrontMatter mixins.
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            seen.Add(JsonNamingPolicy.CamelCase.ConvertName(prop.Name));
        }

        foreach (var iface in t.GetInterfaces())
        {
            foreach (var prop in iface.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                seen.Add(JsonNamingPolicy.CamelCase.ConvertName(prop.Name));
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
