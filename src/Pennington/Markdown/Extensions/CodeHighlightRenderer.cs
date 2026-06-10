namespace Pennington.Markdown.Extensions;

using System.Text;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Tabs;

/// <summary>
/// Markdig renderer for fenced code blocks. Delegates the full
/// preprocess → highlight → transform → wrap pipeline to
/// <see cref="CodeBlockRenderingService"/>.
/// </summary>
internal sealed class CodeHighlightRenderer : HtmlObjectRenderer<CodeBlock>
{
    private readonly CodeBlockRenderingService _renderingService;
    private readonly Func<CodeHighlightRenderOptions> _optionsFactory;

    public CodeHighlightRenderer(
        CodeBlockRenderingService renderingService,
        Func<CodeHighlightRenderOptions>? optionsFactory = null)
    {
        _renderingService = renderingService ?? throw new ArgumentNullException(nameof(renderingService));
        _optionsFactory = optionsFactory ?? (() => CodeHighlightRenderOptions.Default);
    }

    protected override void Write(HtmlRenderer renderer, CodeBlock codeBlock)
    {
        // This renderer replaces Markdig's default CodeBlockRenderer for every CodeBlock,
        // so indented code blocks (a CodeBlock that is not a FencedCodeBlock) must render too.
        // They carry no info string, so they take the empty-language path — identical to a
        // fenced block opened with no language tag.
        var languageId = ExtractFenceLanguage(codeBlock);
        var code = ExtractCode(codeBlock);

        var isInTabGroup = codeBlock.Parent is TabbedCodeBlock;
        var html = _renderingService.Render(code, languageId, _optionsFactory(), isInTabGroup);
        renderer.Write(html);
    }

    private static string ExtractFenceLanguage(CodeBlock codeBlock)
    {
        if (codeBlock is not FencedCodeBlock fencedCodeBlock ||
            codeBlock.Parser is not FencedCodeBlockParser fencedCodeBlockParser ||
            fencedCodeBlock.Info == null ||
            fencedCodeBlockParser.InfoPrefix == null)
        {
            return "";
        }

        return fencedCodeBlock.Info.Replace(fencedCodeBlockParser.InfoPrefix, string.Empty);
    }

    private static string ExtractCode(LeafBlock leafBlock)
    {
        var code = new StringBuilder();

        var lines = leafBlock.Lines.Lines ?? [];
        var totalLines = lines.Length;

        for (var index = 0; index < totalLines; index++)
        {
            var line = lines[index];
            var slice = line.Slice;

            if (slice.Text == null)
            {
                continue;
            }

            var lineText = slice.Text.Substring(slice.Start, slice.Length);

            if (index > 0)
            {
                code.AppendLine();
            }

            code.Append(lineText);
        }

        return code.ToString().Trim();
    }
}