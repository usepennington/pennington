namespace ExtensibilityLabExample;

using System.Text;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

/// <summary>
/// A Markdig extension that teaches the pipeline a new inline token: the
/// <c>[[Target]]</c> / <c>[[Target|Label]]</c> wiki-link. Each match renders as an
/// internal anchor — <c>&lt;a class="wikilink" href="/notes/&lt;slug&gt;/"&gt;Label&lt;/a&gt;</c>
/// — so digital-garden cross-references resolve like ordinary links.
/// <para>
/// Registered through <see cref="Pennington.Infrastructure.PenningtonOptions.ConfigureMarkdownPipeline"/>
/// in <c>Program.cs</c>; that hook runs after Pennington's built-in extensions, so the
/// extension only adds the one parser the built-ins don't already supply.
/// </para>
/// <para>
/// Backs how-to 2.2.65 <c>/how-to/markdown-pipeline/markdig-extension</c>.
/// </para>
/// </summary>
public sealed class WikiLinkExtension : IMarkdownExtension
{
    /// <summary>Inserts the wiki-link inline parser ahead of the built-in link parser.</summary>
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        // Run before the CommonMark link parser so a leading "[[" is claimed as a
        // wiki-link instead of being read as the start of two nested "[...]" links.
        if (!pipeline.InlineParsers.Contains<WikiLinkInlineParser>())
        {
            pipeline.InlineParsers.InsertBefore<LinkInlineParser>(new WikiLinkInlineParser());
        }
    }

    /// <summary>No renderer wiring needed — the emitted <see cref="LinkInline"/> uses Markdig's own anchor renderer.</summary>
    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
    }
}

/// <summary>
/// Inline parser that claims the doubled-bracket <c>[[…]]</c> token and emits a
/// <see cref="LinkInline"/> pointing at <c>/notes/&lt;slug&gt;/</c>. A single <c>[</c>
/// is left for the built-in link parser.
/// </summary>
public sealed class WikiLinkInlineParser : InlineParser
{
    /// <summary>Registers <c>[</c> as the trigger; <see cref="Match"/> bails unless it doubles.</summary>
    public WikiLinkInlineParser()
    {
        OpeningCharacters = ['['];
    }

    /// <summary>Matches <c>[[Target]]</c> / <c>[[Target|Label]]</c> and emits the anchor inline.</summary>
    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        // Only a doubled opener is a wiki-link; "[" alone belongs to the link parser.
        if (slice.PeekChar() != '[')
        {
            return false;
        }

        var saved = slice;
        slice.SkipChar(); // first [
        slice.SkipChar(); // second [

        // Scan the inner text up to the closing "]]". Wiki-links never span lines
        // or nest, so a newline or a fresh "[" aborts the match.
        var contentStart = slice.Start;
        var contentEnd = -1;
        var c = slice.CurrentChar;
        while (c != '\0')
        {
            if (c == ']' && slice.PeekChar() == ']')
            {
                contentEnd = slice.Start - 1; // last char before "]]"
                slice.SkipChar(); // first ]
                slice.SkipChar(); // second ]
                break;
            }

            if (c is '\n' or '\r' or '[')
            {
                break;
            }

            c = slice.NextChar();
        }

        // Unterminated or empty ("[[]]"): restore the slice and let other parsers try.
        if (contentEnd < contentStart)
        {
            slice = saved;
            return false;
        }

        var inner = new StringSlice(slice.Text, contentStart, contentEnd).AsSpan().ToString();
        var (target, label) = SplitTargetAndLabel(inner);
        if (target.Length == 0)
        {
            slice = saved;
            return false;
        }

        var spanStart = processor.GetSourcePosition(saved.Start, out var line, out var column);
        var spanEnd = processor.GetSourcePosition(slice.Start - 1);

        var link = new LinkInline
        {
            Url = $"/notes/{Slugify(target)}/",
            IsClosed = true,
            Span = new SourceSpan(spanStart, spanEnd),
            Line = line,
            Column = column,
        };
        // The class is the contract downstream consumers key on. Internal hrefs like
        // the one above are still rewritten by the response pipeline (locale prefixing,
        // base-URL prefixing), so wiki-links stay portable across deploys and locales.
        link.GetAttributes().AddClass("wikilink");
        link.AppendChild(new LiteralInline(label));

        processor.Inline = link;
        return true;
    }

    // "Target|Label" → (Target, Label); "Target" → (Target, Target).
    private static (string Target, string Label) SplitTargetAndLabel(string inner)
    {
        var pipe = inner.IndexOf('|');
        if (pipe < 0)
        {
            var only = inner.Trim();
            return (only, only);
        }

        var target = inner[..pipe].Trim();
        var label = inner[(pipe + 1)..].Trim();
        return (target, label.Length == 0 ? target : label);
    }

    // Lowercase, collapse runs of non-alphanumerics to single dashes, trim trailing dashes.
    private static string Slugify(string value)
    {
        var sb = new StringBuilder(value.Length);
        var prevDash = false;
        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(ch);
                prevDash = false;
            }
            else if (!prevDash && sb.Length > 0)
            {
                sb.Append('-');
                prevDash = true;
            }
        }

        return sb.ToString().TrimEnd('-');
    }
}
