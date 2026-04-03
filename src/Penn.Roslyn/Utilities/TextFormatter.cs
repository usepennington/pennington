namespace Penn.Roslyn.Utilities;

/// <summary>
/// Text formatting utilities for code fragments.
/// </summary>
internal static class TextFormatter
{
    /// <summary>
    /// Strips common leading whitespace from all non-empty lines,
    /// preserving relative indentation between lines.
    /// </summary>
    public static string NormalizeIndents(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return code;
        }

        var lines = code.Split('\n');
        var minIndent = int.MaxValue;

        // Find the minimum indentation among non-empty lines
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

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

        if (minIndent is 0 or int.MaxValue)
        {
            return code;
        }

        // Strip the common indentation from each line
        var result = new string[lines.Length];
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                result[i] = line;
            }
            else
            {
                result[i] = line.Length > minIndent ? line[minIndent..] : string.Empty;
            }
        }

        return string.Join('\n', result);
    }
}
