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
        if (!TryExtractFencedCodeBlock(codeBlock, out var languageId, out var code))
        {
            return;
        }

        var isInTabGroup = codeBlock.Parent is TabbedCodeBlock;
        var html = _renderingService.Render(code, languageId, _optionsFactory(), isInTabGroup);
        renderer.Write(html);
    }

    private static bool TryExtractFencedCodeBlock(CodeBlock codeBlock, out string languageId, out string code)
    {
        languageId = "";
        code = "";

        if (codeBlock is not FencedCodeBlock fencedCodeBlock ||
            codeBlock.Parser is not FencedCodeBlockParser fencedCodeBlockParser ||
            fencedCodeBlock.Info == null ||
            fencedCodeBlockParser.InfoPrefix == null)
        {
            return false;
        }

        languageId = fencedCodeBlock.Info.Replace(fencedCodeBlockParser.InfoPrefix, string.Empty);
        code = ExtractCode(codeBlock);
        return true;
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
