namespace Pennington.Roslyn.Highlighting;

using System.Net;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using Utilities;

/// <summary>
/// Roslyn Classifier API wrapper that produces HTML with hljs-* and roslyn-* CSS classes.
/// </summary>
public sealed class SyntaxHighlighter : IDisposable
{
    private readonly AdhocWorkspace _adHocWorkspace;
    private readonly Project _csharpProject;
    private readonly Project _vbProject;
    private bool _disposed;

    /// <summary>Source language accepted by the highlighter.</summary>
    public enum Language
    {
        /// <summary>C# source code.</summary>
        CSharp,
        /// <summary>Visual Basic source code.</summary>
        VisualBasic,
    }

    /// <summary>Creates a new highlighter backed by a fresh <see cref="AdhocWorkspace"/> with C# and VB projects.</summary>
    public SyntaxHighlighter()
    {
        _adHocWorkspace = new AdhocWorkspace();
        _csharpProject = _adHocWorkspace.CurrentSolution.AddProject("csProjectName", "assemblyName", LanguageNames.CSharp);
        _vbProject = _adHocWorkspace.CurrentSolution.AddProject("vbProjectName", "assemblyName", LanguageNames.VisualBasic);
    }

    /// <summary>Classifies the supplied code using Roslyn and returns an HTML <c>&lt;pre&gt;&lt;code&gt;</c> block with hljs- and roslyn- CSS classes applied.</summary>
    public string Highlight(string codeContent, Language language = Language.CSharp)
    {
        var project = language switch
        {
            Language.CSharp => _csharpProject,
            Language.VisualBasic => _vbProject,
            _ => throw new NotSupportedException($"Language {language} is not supported.")
        };

        var lang = language switch
        {
            Language.CSharp => "csharp",
            Language.VisualBasic => "vb",
            _ => "text"
        };

        var highlightedCode = AsyncHelpers.RunSync(() => HighlightContent(codeContent, project));
        return $"""<pre><code class="language-{lang} highlighted">{highlightedCode}</code></pre>""";
    }

    private static async Task<string> HighlightContent(string codeContent, Project project)
    {
        var filename = $"name.{codeContent.GetHashCode()}.{Environment.CurrentManagedThreadId}.cs";
        var document = project.AddDocument(filename, codeContent);
        var text = await document.GetTextAsync();
        var textBounds = TextSpan.FromBounds(0, text.Length);
        return await HighlightTextSpan(document, textBounds, text);
    }

    private static async Task<string> HighlightTextSpan(Document document, TextSpan textSpan, SourceText fullText)
    {
        var targetText = fullText.GetSubText(textSpan);
        var classifiedSpans = await Classifier.GetClassifiedSpansAsync(document, textSpan);
        var adjustedSpans = AdjustClassifiedSpans(textSpan, classifiedSpans);
        var ranges = CreateRangesFromSpans(targetText, adjustedSpans);
        ranges = FillGaps(targetText, ranges);
        return BuildHighlightedOutput(ranges);
    }

    private static IEnumerable<ClassifiedSpan> AdjustClassifiedSpans(TextSpan textSpan,
        IEnumerable<ClassifiedSpan> classifiedSpans)
    {
        return classifiedSpans.Select(span =>
        {
            var adjustedStart = span.TextSpan.Start - textSpan.Start;
            var length = span.TextSpan.Length;
            var adjustedSpan = new TextSpan(adjustedStart, length);
            return new ClassifiedSpan(span.ClassificationType, adjustedSpan);
        });
    }

    private static IEnumerable<Range> CreateRangesFromSpans(SourceText targetText,
        IEnumerable<ClassifiedSpan> adjustedSpans)
    {
        return adjustedSpans.Select(span =>
        {
            var rangeText = targetText.GetSubText(span.TextSpan).ToString();
            return new Range(span, rangeText);
        });
    }

    private static string BuildHighlightedOutput(IEnumerable<Range> ranges)
    {
        var sb = new StringBuilder();

        foreach (var range in ranges)
        {
            var cssClass = ClassificationTypeToHighlightJsClass(range.ClassificationType);
            if (string.IsNullOrWhiteSpace(cssClass))
            {
                sb.Append(WebUtility.HtmlEncode(range.Text));
            }
            else
            {
                sb.Append($"""<span class="hljs-{cssClass} roslyn-{range.ClassificationType.Replace(" ", "-")}">{WebUtility.HtmlEncode(range.Text)}</span>""");
            }
        }

        return sb.ToString();
    }

    private static IEnumerable<Range> FillGaps(SourceText text, IEnumerable<Range> ranges)
    {
        const string whitespaceClassification = "";
        var current = 0;
        Range? previous = null;

        foreach (var range in ranges)
        {
            var start = range.TextSpan.Start;
            if (start > current)
            {
                yield return new Range(whitespaceClassification, TextSpan.FromBounds(current, start), text);
            }

            if (previous == null || range.TextSpan != previous.TextSpan)
            {
                yield return range;
            }

            previous = range;
            current = range.TextSpan.End;
        }

        if (current < text.Length)
        {
            yield return new Range(whitespaceClassification, TextSpan.FromBounds(current, text.Length), text);
        }
    }

    private sealed class Range
    {
        private readonly ClassifiedSpan _classifiedSpan;
        public string Text { get; }

        public Range(ClassifiedSpan classifiedSpan, string text)
        {
            _classifiedSpan = classifiedSpan;
            Text = text;
        }

        public Range(string classification, TextSpan span, SourceText text)
            : this(classification, span, text.GetSubText(span).ToString())
        {
        }

        private Range(string classification, TextSpan span, string text)
            : this(new ClassifiedSpan(classification, span), text)
        {
        }

        public string ClassificationType => _classifiedSpan.ClassificationType;

        public TextSpan TextSpan => _classifiedSpan.TextSpan;
    }

    private static string ClassificationTypeToHighlightJsClass(string classificationType)
    {
        return classificationType switch
        {
            // Variables and identifiers
            ClassificationTypeNames.Identifier => "function",
            ClassificationTypeNames.LocalName or ClassificationTypeNames.ParameterName => "variable",

            // Properties and constants
            ClassificationTypeNames.PropertyName or ClassificationTypeNames.EnumMemberName
                or ClassificationTypeNames.FieldName => "attr",

            // Types and classes
            ClassificationTypeNames.ClassName or ClassificationTypeNames.StructName
                or ClassificationTypeNames.RecordClassName or ClassificationTypeNames.RecordStructName
                or ClassificationTypeNames.InterfaceName or ClassificationTypeNames.DelegateName
                or ClassificationTypeNames.EnumName or ClassificationTypeNames.ModuleName => "type",

            // Type parameters
            ClassificationTypeNames.TypeParameterName => "type",

            // Methods and functions
            ClassificationTypeNames.MethodName or ClassificationTypeNames.ExtensionMethodName => "function",

            // Comments
            ClassificationTypeNames.Comment => "comment",

            // Keywords
            ClassificationTypeNames.Keyword or ClassificationTypeNames.ControlKeyword
                or ClassificationTypeNames.PreprocessorKeyword => "keyword",

            // Strings
            ClassificationTypeNames.StringLiteral or ClassificationTypeNames.VerbatimStringLiteral => "string",

            // Numbers
            ClassificationTypeNames.NumericLiteral => "number",

            // Operators
            ClassificationTypeNames.Operator or ClassificationTypeNames.StringEscapeCharacter => "operator",

            // Punctuation
            ClassificationTypeNames.Punctuation => "punctuation",
            ClassificationTypeNames.StaticSymbol => string.Empty,

            // XML Documentation comments
            ClassificationTypeNames.XmlDocCommentComment or ClassificationTypeNames.XmlDocCommentDelimiter
                or ClassificationTypeNames.XmlDocCommentName or ClassificationTypeNames.XmlDocCommentText
                or ClassificationTypeNames.XmlDocCommentAttributeName
                or ClassificationTypeNames.XmlDocCommentAttributeQuotes
                or ClassificationTypeNames.XmlDocCommentAttributeValue
                or ClassificationTypeNames.XmlDocCommentEntityReference
                or ClassificationTypeNames.XmlDocCommentProcessingInstruction
                or ClassificationTypeNames.XmlDocCommentCDataSection => "comment",

            _ => classificationType.ToLower().Replace(" ", "-")
        };
    }

    /// <summary>Disposes the underlying <see cref="AdhocWorkspace"/>.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _adHocWorkspace.Dispose();
        }

        _disposed = true;
    }

    /// <summary>Finalizer that releases the underlying workspace if <see cref="Dispose()"/> was not called.</summary>
    ~SyntaxHighlighter()
    {
        Dispose(false);
    }
}