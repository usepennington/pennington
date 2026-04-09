namespace Pennington.DocSite;

/// <summary>
/// Defines a content area within a documentation site.
/// Each area maps to a top-level directory and URL prefix, and gets its own TOC section.
/// </summary>
/// <param name="Title">Display name shown in the area selector (e.g., "Getting Started").</param>
/// <param name="Slug">URL path prefix / top-level directory name (e.g., "getting-started").</param>
/// <param name="Icon">Optional SVG or markup for an icon beside the title.</param>
public record ContentArea(string Title, string Slug, string? Icon = null);
