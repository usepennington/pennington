namespace Pennington.Markdown;

/// <summary>
/// Shared helper for locating fenced code block byte ranges inside a markdown source. Pre-parse
/// transforms (<see cref="IncludeExpander"/>, <see cref="Shortcodes.ShortcodeExpander"/>) skip
/// matches inside fences so directives can be quoted verbatim in documentation.
/// </summary>
internal static class FencedRegions
{
    /// <summary>
    /// Returns the <c>[start, end)</c> character ranges of every fenced code block
    /// (<c>```</c> or <c>~~~</c>) in <paramref name="text"/>.
    /// </summary>
    public static List<(int Start, int End)> Compute(string text)
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

    /// <summary>True when <paramref name="offset"/> falls inside any region in <paramref name="regions"/>.</summary>
    public static bool Contains(IReadOnlyList<(int Start, int End)> regions, int offset)
    {
        for (var i = 0; i < regions.Count; i++)
        {
            var (start, end) = regions[i];
            if (offset >= start && offset < end)
            {
                return true;
            }
        }

        return false;
    }
}
