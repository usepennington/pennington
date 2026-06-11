using Markdig;
using Pennington.Markdown;

namespace Pennington.Tests.Markdown;

public class CustomAlertsTests
{
    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().UseCustomAlerts().Build();

    private static string Render(string markdown) => Markdig.Markdown.ToHtml(markdown, Pipeline);

    [Theory]
    [InlineData("NOTE", "markdown-alert-note")]
    [InlineData("TIP", "markdown-alert-tip")]
    [InlineData("CHECKPOINT", "markdown-alert-checkpoint")]
    public void Recognized_kinds_render_their_flavor_class(string kind, string cssClass)
    {
        var html = Render($"> [!{kind}]\n> body text\n");

        html.ShouldContain(cssClass);
        html.ShouldContain("body text");
        html.ShouldNotContain($"[!{kind}]");
    }

    [Fact]
    public void Unknown_kinds_stay_a_plain_blockquote()
    {
        // Guards the allowlist: an arbitrary kind must not become an alert box.
        var html = Render("> [!BOGUS]\n> body\n");

        html.ShouldNotContain("markdown-alert");
    }
}
