namespace FocusedCodeSamplesExample;

using System.Text;

/// <summary>
/// Word-frequency counter whose parse, tally, and format phases are named
/// public helpers rather than inline blocks. Pair with
/// <see cref="MonolithWordCounter"/> to contrast a decomposed shape against
/// a monolithic one.
/// </summary>
public static class ModularWordCounter
{
    /// <summary>
    /// Returns a column-aligned report of the <paramref name="topN"/> most
    /// frequent words in <paramref name="text"/> by orchestrating the three
    /// helpers below.
    /// </summary>
    /// <param name="text">Free-form text to analyse.</param>
    /// <param name="topN">Number of top-frequency words to include.</param>
    /// <returns>A multi-line string suitable for console output.</returns>
    public static string CountWords(string text, int topN)
    {
        var words = Tokenize(text);
        var ranked = Tally(words, topN);
        return Format(ranked);
    }

    /// <summary>
    /// Splits <paramref name="text"/> on whitespace, lower-cases every token,
    /// and strips surrounding punctuation. Empty tokens are dropped.
    /// </summary>
    public static List<string> Tokenize(string text)
    {
        var words = new List<string>();
        foreach (var raw in text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries))
        {
            var word = raw.Trim('.', ',', '!', '?', ';', ':', '"', '\'').ToLowerInvariant();
            if (word.Length > 0)
            {
                words.Add(word);
            }
        }
        return words;
    }

    /// <summary>
    /// Groups <paramref name="words"/>, counts occurrences, and returns the
    /// top <paramref name="topN"/> ranked by frequency descending then
    /// alphabetically.
    /// </summary>
    public static List<KeyValuePair<string, int>> Tally(List<string> words, int topN)
    {
        var counts = new Dictionary<string, int>();
        foreach (var w in words)
        {
            counts[w] = counts.GetValueOrDefault(w, 0) + 1;
        }
        return counts
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key)
            .Take(topN)
            .ToList();
    }

    /// <summary>
    /// Renders <paramref name="ranked"/> as a header line plus one
    /// column-aligned row per entry.
    /// </summary>
    public static string Format(List<KeyValuePair<string, int>> ranked)
    {
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

    /// <summary>
    /// Same output as <see cref="Format"/>, but rents its
    /// <see cref="StringBuilder"/> from <see cref="StringBuilderPool"/>
    /// instead of allocating a fresh one each call. Exists to pair with
    /// <see cref="Format"/> inside an <c>xmldocid-diff</c> fence so the
    /// delta is small and focused on one mechanical change.
    /// </summary>
    public static string FormatV2(List<KeyValuePair<string, int>> ranked)
    {
        var sb = StringBuilderPool.Get();
        sb.AppendLine($"Top {ranked.Count} words:");
        foreach (var kv in ranked)
        {
            sb.Append(kv.Key.PadRight(12));
            sb.Append(' ');
            sb.AppendLine(kv.Value.ToString());
        }
        var result = sb.ToString();
        StringBuilderPool.Return(sb);
        return result;
    }
}
