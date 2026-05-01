namespace Pennington.Generation;

using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text;
using System.Text.RegularExpressions;
using Content;
using Diagnostics;
using Infrastructure;
using Pipeline;

/// <summary>
/// <see cref="IBuildAuditor"/> that scans markdown source files for
/// <c>xref:</c> references whose UID does not resolve through the live
/// <see cref="XrefResolver"/>. Catches typos and stale links pre-render so
/// the dev overlay flags them on the page that contains them and the build
/// report lists them once per route.
/// </summary>
public sealed partial class XrefAuditor : IBuildAuditor
{
    private readonly IEnumerable<IContentService> _contentServices;
    private readonly XrefResolver _resolver;
    private readonly IFileSystem _fileSystem;

    /// <summary>Stable identifier surfaced on every diagnostic this auditor emits.</summary>
    public string Code => "content.xref";

    /// <summary>Wires the auditor to the content discovery surface, the xref resolver, and the file system.</summary>
    public XrefAuditor(IEnumerable<IContentService> contentServices, XrefResolver resolver, IFileSystem fileSystem)
    {
        _contentServices = contentServices;
        _resolver = resolver;
        _fileSystem = fileSystem;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BuildDiagnostic>> AuditAsync(BuildAuditContext context, CancellationToken cancellationToken)
    {
        // Cache scan results per markdown file. Fallback routes synthesized for
        // non-default locales reuse the same default-locale source file, so the
        // same body would otherwise be parsed once per locale.
        var brokenByFile = new Dictionary<string, ImmutableList<string>>(StringComparer.OrdinalIgnoreCase);
        var diagnostics = ImmutableList.CreateBuilder<BuildDiagnostic>();

        foreach (var service in _contentServices)
        {
            await foreach (var item in service.DiscoverAsync().WithCancellation(cancellationToken))
            {
                if (item.Source.Value is not MarkdownFileSource mfs) continue;
                var path = mfs.Path.Value;

                if (!brokenByFile.TryGetValue(path, out var brokenUids))
                {
                    brokenUids = await ScanFileAsync(path, cancellationToken);
                    brokenByFile[path] = brokenUids;
                }

                foreach (var uid in brokenUids)
                {
                    diagnostics.Add(new BuildDiagnostic(
                        Severity: DiagnosticSeverity.Warning,
                        Route: item.Route,
                        Message: $"Cannot resolve <xref:{uid}> in {item.Route.CanonicalPath.Value}: no content with this UID is registered.",
                        SourceFile: $"{Code}/{uid}"));
                }
            }
        }

        return diagnostics.ToImmutable();
    }

    private async Task<ImmutableList<string>> ScanFileAsync(string path, CancellationToken cancellationToken)
    {
        if (!_fileSystem.File.Exists(path)) return ImmutableList<string>.Empty;

        string body;
        try
        {
            body = await _fileSystem.File.ReadAllTextAsync(path, cancellationToken);
        }
        catch (IOException)
        {
            return ImmutableList<string>.Empty;
        }

        var scannable = StripCode(body);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var broken = ImmutableList.CreateBuilder<string>();

        foreach (Match match in XrefMatcher().Matches(scannable))
        {
            var uid = match.Groups[1].Value.TrimEnd('.', ',', ';', ':');
            if (string.IsNullOrEmpty(uid)) continue;
            if (!seen.Add(uid)) continue;

            var resolved = await _resolver.ResolveAsync(uid);
            if (resolved is null) broken.Add(uid);
        }

        return broken.ToImmutable();
    }

    /// <summary>
    /// Removes fenced and inline code from a markdown body so a literal
    /// <c>xref:</c> sample inside ` ``` ` doesn't get reported as broken.
    /// </summary>
    private static string StripCode(string body)
    {
        var sb = new StringBuilder(body.Length);
        string? fence = null;
        foreach (var line in body.Split('\n'))
        {
            var trimmed = line.TrimStart();
            if (fence is not null)
            {
                if (trimmed.StartsWith(fence, StringComparison.Ordinal)) fence = null;
                sb.Append('\n');
                continue;
            }
            if (trimmed.StartsWith("```", StringComparison.Ordinal)) { fence = "```"; sb.Append('\n'); continue; }
            if (trimmed.StartsWith("~~~", StringComparison.Ordinal)) { fence = "~~~"; sb.Append('\n'); continue; }
            sb.AppendLine(InlineCodeRegex().Replace(line, ""));
        }
        return sb.ToString();
    }

    [GeneratedRegex(@"xref:([^\s\)\]>\}""']+)", RegexOptions.IgnoreCase)]
    private static partial Regex XrefMatcher();

    [GeneratedRegex(@"`[^`\n]+`")]
    private static partial Regex InlineCodeRegex();
}
