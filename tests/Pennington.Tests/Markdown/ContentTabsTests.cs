using Markdig;
using Pennington.Markdown;

namespace Pennington.Tests.Markdown;

public class ContentTabsTests
{
    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().UseContentTabs().Build();

    private static string Render(string markdown) => Markdig.Markdown.ToHtml(markdown, Pipeline);

    private static int Count(string haystack, string needle)
    {
        var count = 0;
        var index = 0;
        while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }
        return count;
    }

    [Fact]
    public void Render_TwoTabHeadings_ProducesTabsetStructure()
    {
        var html = Render(
            "# [Bash](#tab/bash)\n\nUse bash here.\n\n" +
            "# [PowerShell](#tab/pwsh)\n\nUse pwsh here.\n\n---\n");

        html.ShouldContain("data-content-tabs");
        html.ShouldContain("class=\"ctabs-bar not-prose\"");
        html.ShouldContain("data-tab=\"bash\"");
        html.ShouldContain("data-tab=\"pwsh\"");
        Count(html, "ctab-panel").ShouldBe(2);
        html.ShouldContain("Use bash here.");
        html.ShouldContain("Use pwsh here.");
    }

    [Fact]
    public void Render_TabHeadings_NotEmittedAsHeadings()
    {
        var html = Render(
            "# [Bash](#tab/bash)\n\nbody\n\n# [Zsh](#tab/zsh)\n\nbody\n\n---\n");

        html.ShouldNotContain("<h1");
    }

    [Fact]
    public void Render_FirstTab_IsActiveByDefault()
    {
        var html = Render(
            "# [One](#tab/one)\n\nfirst\n\n# [Two](#tab/two)\n\nsecond\n\n---\n");

        html.ShouldContain("data-tab=\"one\" data-active=\"true\"");
        html.ShouldContain("data-tab=\"two\" data-active=\"false\"");
    }

    [Fact]
    public void Render_PanelContent_RendersBlockMarkdown()
    {
        var html = Render(
            "# [List](#tab/list)\n\n- alpha\n- beta\n\n# [Code](#tab/code)\n\n```bash\nls\n```\n\n---\n");

        html.ShouldContain("<li>alpha</li>");
        html.ShouldContain("<code");
    }

    [Fact]
    public void Render_ThematicBreak_TerminatesGroup()
    {
        var html = Render(
            "# [A](#tab/a)\n\nin first group\n\n---\n\n" +
            "# [B](#tab/b)\n\nin second group\n\n---\n");

        Count(html, "data-content-tabs").ShouldBe(2);
    }

    [Fact]
    public void Render_DependentTabs_CollapseButtonsAndCarryCondition()
    {
        var html = Render(
            "# [.NET](#tab/dotnet/linux)\n\ndotnet linux\n\n" +
            "# [.NET](#tab/dotnet/windows)\n\ndotnet windows\n\n" +
            "# [Node](#tab/node/linux)\n\nnode linux\n\n" +
            "# [Node](#tab/node/windows)\n\nnode windows\n\n---\n");

        // Two distinct ids collapse to two buttons; four panels carry conditions.
        Count(html, "role=\"tab\"").ShouldBe(2);
        Count(html, "ctab-panel").ShouldBe(4);
        html.ShouldContain("data-condition=\"linux\"");
        html.ShouldContain("data-condition=\"windows\"");
    }

    [Fact]
    public void Render_NonTabHeading_LeftUntouched()
    {
        var html = Render("# Regular heading\n\nbody text\n");

        html.ShouldContain("<h1");
        html.ShouldNotContain("data-content-tabs");
    }
}