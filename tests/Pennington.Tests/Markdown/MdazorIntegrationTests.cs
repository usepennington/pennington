using Markdig.Renderers;
using Mdazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Highlighting;
using Pennington.Markdown;
using Pennington.Markdown.Extensions;

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
            new CodeBlockRenderingService(new HighlightingService([])));

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
    public void Pipeline_RendersComponent_WhenComponentRegisteredAfterPipelineFactoryRuns()
    {
        // Mirrors AddDocSite / AddBlogSite: AddMdazor is called first, then
        // AddMdazorComponent<T>() entries follow. The singleton MarkdownPipeline
        // factory must observe the full registry when it runs lazily on first use.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMdazor();
        services.AddSingleton(new HighlightingService([]));
        services.AddSingleton(sp => new CodeBlockRenderingService(sp.GetRequiredService<HighlightingService>()));
        services.AddSingleton(sp =>
            MarkdownPipelineFactory.CreateWithExtensions(sp, sp.GetRequiredService<CodeBlockRenderingService>()));
        services.AddMdazorComponent<MdazorTestGreeting>();

        using var sp = services.BuildServiceProvider();
        var pipeline = sp.GetRequiredService<Markdig.MarkdownPipeline>();

        const string markdown = "<MdazorTestGreeting Name=\"late-bound\" />";
        using var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        pipeline.Setup(renderer);
        var document = Markdig.Markdown.Parse(markdown, pipeline);
        renderer.Render(document);
        writer.Flush();

        var html = writer.ToString();
        html.ShouldContain("hello-world-from-mdazor");
        html.ShouldContain("late-bound");
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
            new CodeBlockRenderingService(new HighlightingService([])));

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