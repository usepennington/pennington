namespace Pennington.Markdown;

using System.IO.Abstractions;
using System.Text.RegularExpressions;
using Routing;

/// <summary>
/// Expands DocFX-style <c>[!INCLUDE [title](path)]</c> directives by splicing the referenced
/// Markdown file's content in place. Targets are resolved relative to the referencing file and
/// expanded recursively; a missing or cyclic target collapses to an HTML comment so the build
/// still completes. Directives inside fenced code blocks are left verbatim so syntax can be
/// documented.
/// </summary>
public static partial class IncludeExpander
{
    private const int MaxDepth = 16;

    [GeneratedRegex(@"\[!INCLUDE\s*\[[^\]\r\n]*\]\(\s*([^)\r\n]+?)\s*\)\]", RegexOptions.IgnoreCase)]
    private static partial Regex IncludePattern();

    /// <summary>
    /// Expands every include directive in <paramref name="markdown"/>. Paths resolve relative to
    /// <paramref name="sourceFile"/>; relative links inside included content are not rebased, so
    /// they resolve as if written in the host page.
    /// </summary>
    public static string Expand(string markdown, FilePath sourceFile, IFileSystem fileSystem)
    {
        if (!markdown.Contains("[!INCLUDE", StringComparison.OrdinalIgnoreCase))
        {
            return markdown;
        }

        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return ExpandCore(markdown, sourceFile.Value, fileSystem, visiting, 0);
    }

    private static string ExpandCore(
        string markdown, string sourceFilePath, IFileSystem fileSystem,
        HashSet<string> visiting, int depth)
    {
        var fencedRegions = GetFencedRegions(markdown);

        return IncludePattern().Replace(markdown, match =>
        {
            // A directive inside a fenced code block is documentation, not a call site.
            if (fencedRegions.Any(r => match.Index >= r.Start && match.Index < r.End))
            {
                return match.Value;
            }

            var rawPath = match.Groups[1].Value.Trim();

            // Only local files are spliced; an absolute URL is left as a comment.
            if (rawPath.Length == 0
                || rawPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || rawPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return $"<!-- Pennington: include skipped (not a local file): {rawPath} -->";
            }

            var sourceDir = fileSystem.Path.GetDirectoryName(sourceFilePath) ?? string.Empty;
            var resolved = fileSystem.Path.GetFullPath(fileSystem.Path.Combine(sourceDir, rawPath));

            if (depth >= MaxDepth || visiting.Contains(resolved))
            {
                return $"<!-- Pennington: include cycle broken: {rawPath} -->";
            }

            if (!fileSystem.File.Exists(resolved))
            {
                return $"<!-- Pennington: include not found: {rawPath} -->";
            }

            var included = StripFrontMatter(fileSystem.File.ReadAllText(resolved));

            visiting.Add(resolved);
            var expanded = ExpandCore(included, resolved, fileSystem, visiting, depth + 1);
            visiting.Remove(resolved);

            return expanded;
        });
    }

    /// <summary>
    /// Returns the <c>[start, end)</c> character ranges of every fenced code block (<c>```</c> or
    /// <c>~~~</c>) so include directives inside them can be skipped.
    /// </summary>
    private static List<(int Start, int End)> GetFencedRegions(string text)
    {
        var regions = new List<(int, int)>();
        var inFence = false;
        var fenceChar = ' ';
        var fenceLength = 0;
        var fenceStart = 0;
        var position = 0;

        while (position < text.Length)
        {
            var newline = text.IndexOf('\n', position);
            var lineEnd = newline < 0 ? text.Length : newline;
            var lineEndExclusive = newline < 0 ? text.Length : newline + 1;
            var line = text[position..lineEnd];

            var indent = 0;
            while (indent < line.Length && line[indent] == ' ')
            {
                indent++;
            }

            if (indent <= 3 && indent < line.Length && (line[indent] == '`' || line[indent] == '~'))
            {
                var marker = line[indent];
                var runLength = 0;
                while (indent + runLength < line.Length && line[indent + runLength] == marker)
                {
                    runLength++;
                }

                if (runLength >= 3)
                {
                    if (!inFence)
                    {
                        inFence = true;
                        fenceChar = marker;
                        fenceLength = runLength;
                        fenceStart = position;
                    }
                    else if (marker == fenceChar
                             && runLength >= fenceLength
                             && line[(indent + runLength)..].Trim().Length == 0)
                    {
                        inFence = false;
                        regions.Add((fenceStart, lineEndExclusive));
                    }
                }
            }

            position = lineEndExclusive;
        }

        if (inFence)
        {
            regions.Add((fenceStart, text.Length));
        }

        return regions;
    }

    /// <summary>
    /// Drops a leading YAML front-matter block so an included partial's metadata does not leak
    /// a stray thematic break into the host page. Content without front matter is returned as-is.
    /// </summary>
    private static string StripFrontMatter(string content)
    {
        var trimmed = content.TrimStart('﻿');
        var firstBreak = trimmed.IndexOf('\n');
        if (firstBreak < 0 || trimmed[..firstBreak].TrimEnd('\r') != "---")
        {
            return content;
        }

        var index = firstBreak + 1;
        while (index < trimmed.Length)
        {
            var lineBreak = trimmed.IndexOf('\n', index);
            var line = (lineBreak < 0 ? trimmed[index..] : trimmed[index..lineBreak]).TrimEnd('\r');
            if (line == "---")
            {
                return lineBreak < 0 ? string.Empty : trimmed[(lineBreak + 1)..];
            }

            if (lineBreak < 0)
            {
                break;
            }

            index = lineBreak + 1;
        }

        return content;
    }
}
