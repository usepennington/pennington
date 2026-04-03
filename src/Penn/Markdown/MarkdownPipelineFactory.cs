namespace Penn.Markdown;

using Markdig;

/// <summary>
/// Creates a configured Markdig MarkdownPipeline.
/// </summary>
public static class MarkdownPipelineFactory
{
    public static MarkdownPipeline CreateDefault()
    {
        return new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .Build();
    }
}
