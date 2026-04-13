namespace Pennington.Markdown.Extensions;

using System.Text;

/// <summary>
/// Options for customizing the CSS classes used in the code highlight renderer.
/// </summary>
public record CodeHighlightRenderOptions
{
    public static readonly CodeHighlightRenderOptions Default = new()
    {
        OuterWrapperCss = "code-highlight-wrapper not-prose",
        StandaloneContainerCss = "standalone-code-container",
        PreBaseCss = "",
        PreStandaloneCss = "standalone-code-highlight",
    };

    /// <summary>CSS class for the outer wrapper element.</summary>
    public required string OuterWrapperCss { get; init; }

    /// <summary>CSS classes for the container when not in a tabbed code block.</summary>
    public required string StandaloneContainerCss { get; init; }

    /// <summary>CSS classes for the Pre element.</summary>
    public required string PreBaseCss { get; init; }

    /// <summary>Additional CSS classes for the Pre element when not in a tabbed code block.</summary>
    public required string PreStandaloneCss { get; init; }
}

/// <summary>
/// Helper class for building HTML wrappers around highlighted code blocks.
/// Encapsulates the logic for generating consistent HTML structure with appropriate CSS classes.
/// </summary>
internal static class CodeBlockHtmlBuilder
{
    /// <summary>
    /// Builds the complete HTML structure for a code block.
    /// </summary>
    public static string BuildHtml(
        string highlightedHtml,
        CodeHighlightRenderOptions options,
        bool isInTabGroup = false)
    {
        var (containerCss, preCss) = GetCssClasses(options, isInTabGroup);

        var html = new StringBuilder();
        html.AppendLine($"<div class=\"{options.OuterWrapperCss}\">");

        if (!string.IsNullOrEmpty(containerCss))
        {
            html.AppendLine($"<div class=\"{containerCss}\">");
        }

        html.AppendLine($"<div class=\"{preCss}\">");
        html.AppendLine(highlightedHtml);
        html.AppendLine("</div>");

        if (!string.IsNullOrEmpty(containerCss))
        {
            html.AppendLine("</div>");
        }

        html.AppendLine("</div>");

        return html.ToString();
    }

    private static (string containerCss, string preCss) GetCssClasses(
        CodeHighlightRenderOptions options,
        bool isInTabGroup)
    {
        var preCss = options.PreBaseCss;
        var containerCss = "";

        if (!isInTabGroup)
        {
            containerCss = options.StandaloneContainerCss;
            preCss = $"{preCss} {options.PreStandaloneCss}".Trim();
        }

        return (containerCss, preCss);
    }
}