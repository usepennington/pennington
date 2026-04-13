using Markdig;
using Markdig.Renderers;
using Mdazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Highlighting;
using Pennington.Markdown;

namespace Pennington.Tests.Markdown;

public class MdazorIntegrationTests
{
    [Fact]
    public void Pipeline_RendersMdazorComponentFromMarkdown()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMdazor();
        services.AddMdazorComponent<MdazorTestGreeting>();
        using var sp = services.BuildServiceProvider();

        var pipeline = MarkdownPipelineFactory.CreateWithExtensions(
            sp,
            new HighlightingService([]));

        const string markdown = "Before\n\n<MdazorTestGreeting Name=\"world\" />\n\nAfter";

        using var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        pipeline.Setup(renderer);
        var document = Markdig.Markdown.Parse(markdown, pipeline);
        renderer.Render(document);
        writer.Flush();

        var html = writer.ToString();
        html.ShouldContain("hello-world-from-mdazor");
        html.ShouldContain("world");
    }

    [Fact]
    public void Pipeline_PassesThroughMarkdown_WhenNoMdazorComponentsRegistered()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMdazor();
        using var sp = services.BuildServiceProvider();

        var pipeline = MarkdownPipelineFactory.CreateWithExtensions(
            sp,
            new HighlightingService([]));

        const string markdown = "# Heading\n\nPlain paragraph.";

        using var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        pipeline.Setup(renderer);
        var document = Markdig.Markdown.Parse(markdown, pipeline);
        renderer.Render(document);
        writer.Flush();

        var html = writer.ToString();
        html.ShouldContain("<h1");
        html.ShouldContain("Heading");
        html.ShouldContain("Plain paragraph.");
    }
}

internal sealed class MdazorTestGreeting : ComponentBase
{
    [Parameter] public string Name { get; set; } = "";

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", "hello-world-from-mdazor");
        builder.AddContent(2, Name);
        builder.CloseElement();
    }
}
