namespace Pennington.LlmsTxt;

/// <summary>Configuration for llms.txt generation.</summary>
public sealed class LlmsTxtOptions
{
    /// <summary>Output directory for raw markdown files (relative to site root). Default: "_llms".</summary>
    public string OutputDirectory { get; set; } = "_llms";

    /// <summary>Whether to also generate llms-full.txt with all content concatenated.</summary>
    public bool GenerateFullFile { get; set; }
}
