namespace Pennington.Infrastructure;

using System.Text;

/// <summary>
/// Inserts word-break characters into long identifiers — at dots and at
/// lowercase/digit→uppercase transitions — for words that reach
/// <see cref="WordBreakOptions.MinimumCharacters"/>. Pure text transform;
/// <see cref="WordBreakHtmlRewriter"/> applies it to selected DOM text.
/// </summary>
internal sealed class WordBreakProcessor
{
    private readonly WordBreakOptions _options;

    public WordBreakProcessor(WordBreakOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Processes text by inserting word break characters at appropriate positions.
    /// </summary>
    public string ProcessText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // if we don't even have the minimum characters, no need to go forward
        if (text.Length < _options.MinimumCharacters)
            return text;

        var span = text.AsSpan();
        var result = new StringBuilder(text.Length + 100); // Pre-allocate with some extra space
        var wordStart = 0;

        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] != ' ') continue;
            // Process the word from wordStart to i
            if (i > wordStart)
            {
                var wordSpan = span.Slice(wordStart, i - wordStart);
                ProcessWord(wordSpan, result);
            }
            result.Append(' ');
            wordStart = i + 1;
        }

        // Process the last word if any
        if (wordStart >= span.Length) return result.ToString();
        {
            var wordSpan = span[wordStart..];
            ProcessWord(wordSpan, result);
        }

        return result.ToString();
    }

    private void ProcessWord(ReadOnlySpan<char> word, StringBuilder result)
    {
        if (word.Length < _options.MinimumCharacters)
        {
            result.Append(word);
            return;
        }

        // Check if word contains dots
        var dotIndex = word.IndexOf('.');
        if (dotIndex == -1)
        {
            // No dots, but still process uppercase breaks if word meets minimum length
            ProcessSegment(word, result);
            return;
        }

        var start = 0;

        while (dotIndex != -1)
        {
            // Process the segment before the dot
            var segment = word.Slice(start, dotIndex - start);
            ProcessSegment(segment, result);

            // Append the dot
            result.Append('.');

            // Add word break after the dot if there's more content
            if (dotIndex + 1 < word.Length)
            {
                result.Append(_options.WordBreakCharacters);
            }

            start = dotIndex + 1;
            if (start < word.Length)
            {
                dotIndex = word[start..].IndexOf('.');
                if (dotIndex != -1)
                {
                    dotIndex += start;
                }
            }
            else
            {
                break;
            }
        }

        // Process remaining part after last dot
        if (start < word.Length)
        {
            var segment = word[start..];
            ProcessSegment(segment, result);
        }
    }

    private void ProcessSegment(ReadOnlySpan<char> segment, StringBuilder result)
    {
        // If segment is shorter than minimum characters, don't process uppercase breaks
        if (segment.Length < _options.MinimumCharacters)
        {
            result.Append(segment);
            return;
        }

        // Process uppercase letter breaks within the segment
        var segmentStart = 0;

        for (var i = 1; i < segment.Length; i++)
        {
            // Check if current character is uppercase and previous is lowercase or digit
            if (char.IsUpper(segment[i]) && i > 0 && (char.IsLower(segment[i - 1]) || char.IsDigit(segment[i - 1])))
            {
                // Append text up to the uppercase letter
                result.Append(segment.Slice(segmentStart, i - segmentStart));
                result.Append(_options.WordBreakCharacters);
                segmentStart = i;
            }
        }

        // Append remaining part of segment
        if (segmentStart < segment.Length)
        {
            result.Append(segment[segmentStart..]);
        }
    }
}
