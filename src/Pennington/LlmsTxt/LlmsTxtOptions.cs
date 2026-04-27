namespace Pennington.LlmsTxt;

/// <summary>Configuration for llms.txt generation.</summary>
public sealed class LlmsTxtOptions
{
    /// <summary>Output directory for raw markdown files (relative to site root). Default: "_llms".</summary>
    public string OutputDirectory { get; set; } = "_llms";

    /// <summary>Whether to also generate llms-full.txt with all content concatenated.</summary>
    public bool GenerateFullFile { get; set; }

    /// <summary>
    /// CSS selector used to scope the HTML-to-markdown conversion when a page is fetched
    /// over HTTP for the LLM channel. Markdown-source pages render via the rendition
    /// channel and ignore this setting — this only applies to Razor pages and other
    /// non-markdown content where the LlmsTxtService falls back to fetching the live
    /// rendered HTML and stripping the layout chrome.
    /// <para>
    /// Default <see langword="null"/> means the whole <c>&lt;body&gt;</c> is used.
    /// Hosts with a layout shell (e.g. DocSite's <c>#main-content</c>) should set this
    /// so navigation, footers, and other chrome don't bleed into the LLM sidecars.
    /// </para>
    /// </summary>
    public string? ContentSelector { get; set; }
}