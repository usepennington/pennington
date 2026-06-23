namespace Pennington.LlmsTxt;

/// <summary>Configuration for llms.txt generation.</summary>
public sealed class LlmsTxtOptions
{
    /// <summary>Whether to also generate llms-full.txt with all content concatenated.</summary>
    public bool GenerateFullFile { get; set; }
}