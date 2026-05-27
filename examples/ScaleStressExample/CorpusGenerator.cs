namespace ScaleStressExample;

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

internal static class CorpusGenerator
{
    private const int TargetCount = 5000;
    private const int Seed = 1138;

    private static readonly string[] TagPool =
    [
        "guide", "reference", "concept", "tutorial", "internals",
        "design", "performance", "errors", "tooling", "scripting",
        "syntax", "stdlib", "config", "blog", "language",
    ];

    private static readonly string[] CodeSamples =
    [
        """
        ```csharp
        public static class Calculator
        {
            public static int Add(int a, int b) => a + b;
            public static int Multiply(int a, int b) => a * b;
        }
        ```
        """,
        """
        ```python
        def fibonacci(n):
            a, b = 0, 1
            for _ in range(n):
                a, b = b, a + b
            return a
        ```
        """,
        """
        ```javascript
        const greet = (name) => `Hello, ${name}!`;
        console.log(greet("world"));
        ```
        """,
        """
        ```bash
        #!/usr/bin/env bash
        set -euo pipefail
        for file in *.txt; do
          echo "processing ${file}"
        done
        ```
        """,
    ];

    public static async Task EnsureAsync(string outputDir, string corpusDir)
    {
        var existing = Directory.Exists(outputDir)
            ? Directory.GetFiles(outputDir, "doc-*.md").Length
            : 0;

        if (existing >= TargetCount)
        {
            return;
        }

        Directory.CreateDirectory(outputDir);

        var sw = Stopwatch.StartNew();
        Console.WriteLine($"[ScaleStress] Generating {TargetCount - existing} markdown files into {Path.GetFullPath(outputDir)}");

        var chain = await TrainAsync(corpusDir);
        if (chain.IsEmpty)
        {
            throw new InvalidOperationException(
                $"Markov chain training produced no data. Corpus path not found or empty: {Path.GetFullPath(corpusDir)}");
        }

        for (var i = existing + 1; i <= TargetCount; i++)
        {
            var body = chain.GenerateDocument(i);
            await File.WriteAllTextAsync(Path.Combine(outputDir, $"doc-{i:0000}.md"), body);
            if (i % 500 == 0)
            {
                Console.WriteLine($"[ScaleStress]   wrote {i}/{TargetCount}");
            }
        }

        Console.WriteLine($"[ScaleStress] Generated {TargetCount} files in {sw.Elapsed.TotalSeconds:0.0}s");
    }

    private static async Task<MarkovChain> TrainAsync(string corpusDir)
    {
        var chain = new MarkovChain(Seed);
        if (!Directory.Exists(corpusDir))
        {
            return chain;
        }

        foreach (var file in Directory.EnumerateFiles(corpusDir, "*.md", SearchOption.AllDirectories))
        {
            var text = await File.ReadAllTextAsync(file);
            chain.Train(StripMarkdown(text));
        }

        return chain;
    }

    private static readonly Regex FrontMatterRx = new(@"^---\s*\n.*?\n---\s*\n", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex FenceRx = new(@"```.*?```", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex InlineCodeRx = new(@"`[^`]*`", RegexOptions.Compiled);
    private static readonly Regex LinkRx = new(@"\[([^\]]+)\]\([^)]+\)", RegexOptions.Compiled);
    private static readonly Regex XrefRx = new(@"<xref:[^>]+>|xref:[A-Za-z0-9._-]+", RegexOptions.Compiled);
    private static readonly Regex HeadingRx = new(@"^#+\s*", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex TableRx = new(@"^\|.*$", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex AlertRx = new(@"^>\s*\[![A-Z]+\]\s*", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex BlockquoteRx = new(@"^>\s*", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex ListRx = new(@"^[\s]*[-*]\s+", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex WhitespaceRx = new(@"\s+", RegexOptions.Compiled);

    private static string StripMarkdown(string source)
    {
        var s = FrontMatterRx.Replace(source, "");
        s = FenceRx.Replace(s, "");
        s = InlineCodeRx.Replace(s, "");
        s = LinkRx.Replace(s, "$1");
        s = XrefRx.Replace(s, "");
        s = HeadingRx.Replace(s, "");
        s = TableRx.Replace(s, "");
        s = AlertRx.Replace(s, "");
        s = BlockquoteRx.Replace(s, "");
        s = ListRx.Replace(s, "");
        return WhitespaceRx.Replace(s, " ").Trim();
    }

    private sealed class MarkovChain
    {
        private readonly Dictionary<(string, string), List<string>> _next = new();
        private readonly List<(string, string)> _sentenceStarts = new();
        private readonly Random _rng;

        public MarkovChain(int seed) => _rng = new Random(seed);

        public bool IsEmpty => _sentenceStarts.Count == 0;

        public void Train(string text)
        {
            var tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 3)
            {
                return;
            }

            _sentenceStarts.Add((tokens[0], tokens[1]));
            for (var i = 0; i < tokens.Length - 2; i++)
            {
                var key = (tokens[i], tokens[i + 1]);
                if (!_next.TryGetValue(key, out var list))
                {
                    list = new List<string>();
                    _next[key] = list;
                }
                list.Add(tokens[i + 2]);

                // A token that ends a sentence (`.`, `!`, `?`) makes the next pair a viable start.
                if (i + 2 < tokens.Length && EndsSentence(tokens[i + 1]))
                {
                    _sentenceStarts.Add((tokens[i + 2], i + 3 < tokens.Length ? tokens[i + 3] : tokens[i + 2]));
                }
            }
        }

        private static bool EndsSentence(string token) =>
            token.Length > 0 && (token[^1] == '.' || token[^1] == '!' || token[^1] == '?');

        private string GenerateWords(int approxCount)
        {
            var (a, b) = _sentenceStarts[_rng.Next(_sentenceStarts.Count)];
            var sb = new StringBuilder();
            sb.Append(a).Append(' ').Append(b);

            for (var produced = 2; produced < approxCount; produced++)
            {
                if (!_next.TryGetValue((a, b), out var options) || options.Count == 0)
                {
                    (a, b) = _sentenceStarts[_rng.Next(_sentenceStarts.Count)];
                    sb.Append(' ').Append(a).Append(' ').Append(b);
                    produced += 2;
                    continue;
                }

                var word = options[_rng.Next(options.Count)];
                sb.Append(' ').Append(word);
                a = b;
                b = word;
            }

            return sb.ToString();
        }

        private string GenerateSentence()
        {
            var raw = GenerateWords(_rng.Next(10, 22));
            var cleaned = raw.TrimEnd('.', ',', ';', ':', '!', '?', ' ');
            return cleaned + ".";
        }

        private string GenerateTitle(int min, int max)
        {
            var raw = GenerateWords(_rng.Next(min, max));
            var words = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(StripPunctuation)
                .Where(w => w.Length > 0)
                .Take(_rng.Next(min, max))
                .ToArray();
            if (words.Length == 0)
            {
                words = ["Document"];
            }
            for (var i = 0; i < words.Length; i++)
            {
                words[i] = char.ToUpperInvariant(words[i][0]) + words[i][1..];
            }
            return string.Join(' ', words);
        }

        private static string StripPunctuation(string word)
        {
            var span = word.AsSpan();
            var start = 0;
            var end = span.Length;
            while (start < end && !char.IsLetterOrDigit(span[start]))
            {
                start++;
            }
            while (end > start && !char.IsLetterOrDigit(span[end - 1]))
            {
                end--;
            }
            return span[start..end].ToString();
        }

        public string GenerateDocument(int n)
        {
            var sb = new StringBuilder();
            var title = SafeForYaml(GenerateTitle(3, 7));
            var description = SafeForYaml(GenerateSentence());
            var tagCount = _rng.Next(2, 4);
            var tags = Enumerable.Range(0, tagCount)
                .Select(_ => TagPool[_rng.Next(TagPool.Length)])
                .Distinct()
                .ToArray();

            sb.AppendLine("---");
            sb.AppendLine($"title: \"{title}\"");
            sb.AppendLine($"description: \"{description}\"");
            sb.AppendLine($"uid: stress.doc-{n:0000}");
            sb.AppendLine($"order: {n}");
            sb.AppendLine($"tags: [{string.Join(", ", tags)}]");
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine(GenerateSentence() + " " + GenerateSentence());
            sb.AppendLine();

            var sectionCount = _rng.Next(3, 6);
            for (var s = 0; s < sectionCount; s++)
            {
                sb.Append("## ").AppendLine(GenerateTitle(2, 5));
                sb.AppendLine();

                var paragraphs = _rng.Next(1, 4);
                for (var p = 0; p < paragraphs; p++)
                {
                    var sentenceCount = _rng.Next(2, 5);
                    for (var i = 0; i < sentenceCount; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(' ');
                        }
                        sb.Append(GenerateSentence());
                    }
                    sb.AppendLine();
                    sb.AppendLine();
                }

                if (_rng.NextDouble() < 0.55)
                {
                    sb.AppendLine(CodeSamples[_rng.Next(CodeSamples.Length)]);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private static string SafeForYaml(string value)
        {
            var cleaned = value.Replace('"', '\'').Replace('\\', '/').Trim();
            if (cleaned.Length > 140)
            {
                cleaned = cleaned[..140].TrimEnd('.', ',', ' ') + ".";
            }
            return cleaned;
        }
    }
}
