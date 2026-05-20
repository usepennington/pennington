namespace Pennington.Roslyn.Utilities;

/// <summary>
/// Text formatting utilities for code fragments.
/// </summary>
internal static class TextFormatter
{
    /// <summary>
    /// Strips common leading whitespace from all non-empty lines,
    /// preserving relative indentation between lines, and trims
    /// leading/trailing blank lines.
    /// </summary>
    public static string NormalizeIndents(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return code;
        }

        var lines = code.Split('\n');
        var minIndent = int.MaxValue;
        var first = -1;
        var last = -1;

        // Find the minimum indentation and the first/last non-empty lines
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (first < 0)
            {
                first = i;
            }

            last = i;

            var indent = 0;
            foreach (var ch in line)
            {
                if (ch is ' ' or '\t')
                {
                    indent++;
                }
                else
                {
                    break;
                }
            }

            minIndent = Math.Min(minIndent, indent);
        }

        if (first < 0)
        {
            // No non-empty lines — return unchanged so callers can distinguish
            return code;
        }

        // Dedent (when minIndent > 0) and trim leading/trailing blank lines.
        // Body-only extraction starts with "\n<indent>..." and ends with the
        // close-brace indent, so stripping the surrounding blanks produces a
        // clean block.
        var kept = last - first + 1;
        var result = new string[kept];
        for (var i = 0; i < kept; i++)
        {
            var line = lines[first + i];
            if (string.IsNullOrWhiteSpace(line))
            {
                result[i] = string.Empty;
            }
            else
            {
                result[i] = minIndent > 0 && line.Length > minIndent ? line[minIndent..] : line;
            }
        }

        return string.Join('\n', result);
    }
}