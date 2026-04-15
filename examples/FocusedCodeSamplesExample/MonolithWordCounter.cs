namespace FocusedCodeSamplesExample;

using System.Text;

/// <summary>
/// Word-frequency counter whose parse, tally, and format phases all live
/// inline inside one long method. Pair with <see cref="ModularWordCounter"/>
/// to contrast a monolithic shape against a decomposed one.
/// </summary>
public static class MonolithWordCounter
{
    /// <summary>
    /// Returns a column-aligned report of the <paramref name="topN"/> most
    /// frequent words in <paramref name="text"/>, lower-cased and with
    /// surrounding punctuation stripped.
    /// </summary>
    /// <param name="text">Free-form text to analyse.</param>
    /// <param name="topN">Number of top-frequency words to include.</param>
    /// <returns>A multi-line string suitable for console output.</returns>
    public static string CountWords(string text, int topN)
    {
        // Tokenize: split on whitespace, lowercase, strip surrounding punctuation.
        var words = new List<string>();
        foreach (var raw in text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries))
        {
            var word = raw.Trim('.', ',', '!', '?', ';', ':', '"', '\'').ToLowerInvariant();
            if (word.Length > 0)
            {
                words.Add(word);
            }
        }

        // Tally: count occurrences and rank by frequency desc, then alphabetically.
        var counts = new Dictionary<string, int>();
        foreach (var w in words)
        {
            counts[w] = counts.GetValueOrDefault(w, 0) + 1;
        }
        var ranked = counts
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key)
            .Take(topN)
            .ToList();

        // Format: header line plus one word-count row per entry.
        var sb = new StringBuilder();
        sb.AppendLine($"Top {ranked.Count} words:");
        foreach (var kv in ranked)
        {
            sb.Append(kv.Key.PadRight(12));
            sb.Append(' ');
            sb.AppendLine(kv.Value.ToString());
        }
        return sb.ToString();
    }
}
