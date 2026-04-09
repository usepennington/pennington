namespace Pennington.Infrastructure;

/// <summary>
/// Represents a font file to preload via a link rel="preload" hint in the HTML head.
/// </summary>
/// <param name="Href">The URL path to the font file (e.g., "fonts/lexend.woff2").</param>
/// <param name="Type">The MIME type of the font file. Defaults to "font/woff2".</param>
public record FontPreload(string Href, string Type = "font/woff2");
