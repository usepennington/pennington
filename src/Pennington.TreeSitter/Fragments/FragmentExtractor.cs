namespace Pennington.TreeSitter.Fragments;

using Resolution;
using TsNode = global::TreeSitter.Node;

/// <summary>Extracts and dedents source text for a resolved declaration node.</summary>
internal static class FragmentExtractor
{
    /// <summary>
    /// Returns the declaration's full source text, or its body when <paramref name="bodyOnly"/> is set, dedented
    /// so the fragment renders at column zero.
    /// </summary>
    public static string Extract(TsNode node, LanguageDeclarationConfig config, bool bodyOnly)
    {
        if (!bodyOnly)
        {
            return DedentByColumn(node.Text, node.StartPosition.Column);
        }

        var body = node.GetChildForField(config.BodyFieldName);
        if (body is null)
        {
            return DedentByColumn(node.Text, node.StartPosition.Column);
        }

        var text = DedentByColumn(body.Text, body.StartPosition.Column);
        var lines = text.Split('\n');
        if (lines.Length >= 2 && lines[0].Trim() == "{" && lines[^1].Trim() == "}")
        {
            // Brace-delimited body (C#, Rust, TypeScript, Go): drop the brace lines and re-dedent the interior.
            return DedentByMin(string.Join("\n", lines[1..^1]));
        }

        // Indentation-delimited body (Python suite): already starts at the statement, just normalize.
        return DedentByMin(text);
    }

    /// <summary>
    /// Removes up to <paramref name="column"/> leading whitespace characters from every line after the first.
    /// The node's own first line already starts at the declaration, so only continuation lines carry the
    /// declaration's original indentation.
    /// </summary>
    private static string DedentByColumn(string text, int column)
    {
        var normalized = text.Replace("\r\n", "\n");
        if (column <= 0)
        {
            return normalized;
        }

        var lines = normalized.Split('\n');
        for (var i = 1; i < lines.Length; i++)
        {
            var strip = 0;
            while (strip < column && strip < lines[i].Length && (lines[i][strip] == ' ' || lines[i][strip] == '\t'))
            {
                strip++;
            }

            lines[i] = lines[i][strip..];
        }

        return string.Join("\n", lines);
    }

    /// <summary>Removes the common leading whitespace shared by all non-empty lines.</summary>
    private static string DedentByMin(string text)
    {
        var lines = text.Replace("\r\n", "\n").Split('\n');

        var min = int.MaxValue;
        foreach (var line in lines)
        {
            if (line.Trim().Length == 0)
            {
                continue;
            }

            var indent = line.Length - line.TrimStart(' ', '\t').Length;
            min = Math.Min(min, indent);
        }

        if (min is int.MaxValue or 0)
        {
            return string.Join("\n", lines).Trim('\n');
        }

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Length >= min)
            {
                lines[i] = lines[i][min..];
            }
        }

        return string.Join("\n", lines).Trim('\n');
    }
}
