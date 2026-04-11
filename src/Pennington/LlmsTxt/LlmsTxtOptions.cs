namespace Pennington.LlmsTxt;

/// <summary>Configuration for llms.txt generation.</summary>
public sealed class LlmsTxtOptions
{
    /// <summary>Output directory for raw markdown files (relative to site root). Default: "_llms".</summary>
    public string OutputDirectory { get; set; } = "_llms";

    /// <summary>Whether to also generate llms-full.txt with all content concatenated.</summary>
    public bool GenerateFullFile { get; set; }

    /// <summary>
    /// CSS selector identifying the main content element inside the rendered page HTML
    /// (e.g. "#main-content", "article", "main"). The matched element is converted to
    /// markdown for the llms.txt output. When null, the entire &lt;body&gt; is used.
    /// </summary>
    public string? ContentSelector { get; set; }
}
