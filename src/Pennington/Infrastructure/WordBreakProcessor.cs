namespace Pennington.Infrastructure;

using System.Buffers;

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
        if (string.IsNullOrWhiteSpace(text)) return text;
        if (text.Length < _options.MinimumCharacters) return text;

        // Two-pass: collect break positions in the original text first. If none,
        // return the input unchanged so the rewriter's `processed != TextContent`
        // check skips the InnerHtml assignment (and AngleSharp's re-parse). The
        // splice phase then writes exactly once into a pooled buffer — no
        // StringBuilder, no resize churn.
        Span<int> inline = stackalloc int[64];
        var positions = new PositionList(inline);
        try
        {
            var src = text.AsSpan();
            CollectBreakPositions(src, ref positions);
            if (positions.Count == 0) return text;
            return Splice(src, positions.AsSpan(), _options.WordBreakCharacters.AsSpan());
        }
        finally
        {
            positions.Dispose();
        }
    }

    private void CollectBreakPositions(ReadOnlySpan<char> text, ref PositionList positions)
    {
        var min = _options.MinimumCharacters;
        var wordStart = 0;

        for (var i = 0; i <= text.Length; i++)
        {
            var atEnd = i == text.Length;
            if (!atEnd && text[i] != ' ') continue;

            var wordLen = i - wordStart;
            if (wordLen >= min)
            {
                CollectWord(text, wordStart, i, min, ref positions);
            }
            wordStart = i + 1;
        }
    }

    private static void CollectWord(ReadOnlySpan<char> text, int start, int end, int min, ref PositionList positions)
    {
        var segStart = start;

        for (var i = start; i < end; i++)
        {
            if (text[i] != '.') continue;

            CollectSegment(text, segStart, i, min, ref positions);
            if (i + 1 < end)
            {
                positions.Add(i + 1);
            }
            segStart = i + 1;
        }

        if (segStart < end)
        {
            CollectSegment(text, segStart, end, min, ref positions);
        }
    }

    private static void CollectSegment(ReadOnlySpan<char> text, int start, int end, int min, ref PositionList positions)
    {
        if (end - start < min) return;

        for (var i = start + 1; i < end; i++)
        {
            if (char.IsUpper(text[i]) && (char.IsLower(text[i - 1]) || char.IsDigit(text[i - 1])))
            {
                positions.Add(i);
            }
        }
    }

    private static string Splice(ReadOnlySpan<char> text, ReadOnlySpan<int> positions, ReadOnlySpan<char> breakChars)
    {
        var totalLen = text.Length + positions.Length * breakChars.Length;
        var buffer = ArrayPool<char>.Shared.Rent(totalLen);
        try
        {
            var dest = buffer.AsSpan(0, totalLen);
            var srcIdx = 0;
            var dstIdx = 0;

            foreach (var pos in positions)
            {
                var run = pos - srcIdx;
                text.Slice(srcIdx, run).CopyTo(dest[dstIdx..]);
                dstIdx += run;
                breakChars.CopyTo(dest[dstIdx..]);
                dstIdx += breakChars.Length;
                srcIdx = pos;
            }
            text[srcIdx..].CopyTo(dest[dstIdx..]);

            return new string(dest);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    private ref struct PositionList
    {
        private Span<int> _span;
        private int[]? _rented;
        private int _count;

        public PositionList(Span<int> initial)
        {
            _span = initial;
            _rented = null;
            _count = 0;
        }

        public int Count => _count;

        public ReadOnlySpan<int> AsSpan() => _span[.._count];

        public void Add(int value)
        {
            if (_count == _span.Length) Grow();
            _span[_count++] = value;
        }

        private void Grow()
        {
            var next = ArrayPool<int>.Shared.Rent(_span.Length * 2);
            _span[.._count].CopyTo(next);
            if (_rented is not null) ArrayPool<int>.Shared.Return(_rented);
            _rented = next;
            _span = next;
        }

        public void Dispose()
        {
            if (_rented is not null) ArrayPool<int>.Shared.Return(_rented);
        }
    }
}
