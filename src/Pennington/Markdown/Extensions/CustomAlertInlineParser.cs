namespace Pennington.Markdown.Extensions;

using Markdig.Extensions.Alerts;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

/// <summary>
/// Parses GitHub-style alert blocks (e.g. [!NOTE], [!TIP], [!WARNING]) within Markdown quote blocks.
/// </summary>
internal sealed class CustomAlertInlineParser : InlineParser
{
    private static readonly HashSet<string> KnownKinds = new(StringComparer.OrdinalIgnoreCase)
    {
        // CHECKPOINT closes the loop with HtmlToMarkdownConverter, which already emits
        // `> [!CHECKPOINT]` for the <Checkpoint> component's markdown-alert-checkpoint box.
        // Without it here, that round-trip (llms.txt, and re-rendered book pages) degrades the
        // checkpoint to a literal `[!CHECKPOINT]` blockquote. Markdig has no icon for the kind,
        // so it renders as a text-only "Checkpoint" label — matching the component exactly.
        "NOTE", "TIP", "IMPORTANT", "WARNING", "CAUTION", "CHECKPOINT",
    };

    public CustomAlertInlineParser()
    {
        OpeningCharacters = ['['];
    }

    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        if (slice.PeekChar() != '!')
        {
            return false;
        }

        if (processor.Block is not ParagraphBlock paragraphBlock ||
            paragraphBlock.Parent is not QuoteBlock quoteBlock ||
            paragraphBlock.Inline?.FirstChild != null ||
            quoteBlock is AlertBlock)
        {
            return false;
        }

        var saved = slice;

        slice.SkipChar(); // Skip [
        var c = slice.NextChar(); // Skip !

        var start = slice.Start;
        var end = start;
        while (c.IsAlpha())
        {
            end = slice.Start;
            c = slice.NextChar();
        }

        if (c != ']' || start == end)
        {
            slice = saved;
            return false;
        }

        var alertType = new StringSlice(slice.Text, start, end);
        if (!KnownKinds.Contains(alertType.AsSpan().ToString()))
        {
            slice = saved;
            return false;
        }

        c = slice.NextChar(); // Skip ]

        start = slice.Start;
        while (true)
        {
            if (c is '\0' or '\n' or '\r')
            {
                end = slice.Start;
                if (c == '\r')
                {
                    c = slice.NextChar();
                    if (c is '\0' or '\n')
                    {
                        end = slice.Start;
                        if (c == '\n')
                        {
                            slice.SkipChar();
                        }
                    }
                }
                else if (c == '\n')
                {
                    slice.SkipChar();
                }
                break;
            }
            else if (!c.IsSpaceOrTab())
            {
                slice = saved;
                return false;
            }

            c = slice.NextChar();
        }

        var alertBlock = new AlertBlock(alertType)
        {
            Span = quoteBlock.Span,
            TriviaSpaceAfterKind = new StringSlice(slice.Text, start, end),
            Line = quoteBlock.Line,
            Column = quoteBlock.Column,
        };

        var attributes = alertBlock.GetAttributes();
        attributes.AddClass("markdown-alert");
        attributes.AddClass($"markdown-alert-{alertType.AsSpan().ToString().ToLowerInvariant()}");
        // not-prose isolates the alert from page-prose typography (list bullets,
        // paragraph margins, link color) so the box's interior renders the same
        // here as inside the Mdazor <Checkpoint> component.
        attributes.AddClass("not-prose");

        quoteBlock.ReplaceBy(alertBlock);
        processor.ReplaceParentContainer(quoteBlock, alertBlock);

        return true;
    }
}