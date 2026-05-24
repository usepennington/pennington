namespace Pennington.Pipeline;

/// <summary>
/// Estimates reading time from the markdown body and contributes it as
/// <c>reading_time_minutes</c>. A pure function of <see cref="ParsedItem.RawMarkdown"/>
/// — no file access, no external dependencies.
/// </summary>
public sealed class ReadingTimeEnricher : IMetadataEnricher
{
    /// <summary>Words read per minute used to derive the estimate.</summary>
    private const int WordsPerMinute = 200;

    /// <summary>Key written into <see cref="ParsedItem.Derived"/>.</summary>
    public const string Key = "reading_time_minutes";

    /// <inheritdoc/>
    public Task<IReadOnlyDictionary<string, object?>> EnrichAsync(ParsedItem item)
    {
        var words = CountWords(item.RawMarkdown);
        if (words == 0)
        {
            return Task.FromResult<IReadOnlyDictionary<string, object?>>(
                new Dictionary<string, object?>());
        }

        var minutes = Math.Max(1, (int)Math.Ceiling(words / (double)WordsPerMinute));
        return Task.FromResult<IReadOnlyDictionary<string, object?>>(
            new Dictionary<string, object?> { [Key] = minutes });
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        var count = 0;
        var inWord = false;
        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                inWord = false;
            }
            else if (!inWord)
            {
                inWord = true;
                count++;
            }
        }

        return count;
    }
}
