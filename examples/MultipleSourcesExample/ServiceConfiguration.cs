namespace MultipleSourcesExample;

using Pennington.Infrastructure;

/// <summary>
/// Static helpers backing <c>how-to/configuration/multiple-sources</c>.
/// Each method is a fence target for the matching step in that how-to.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Points the first markdown source at <c>Content/docs</c> and prefixes
    /// its pages under <c>/docs</c>. Paired with
    /// <see cref="RegisterBlogSource"/> inside a single
    /// <c>AddPennington</c> lambda.
    /// </summary>
    public static void RegisterDocSource(MarkdownContentOptions md)
    {
        md.ContentPath = "Content/docs";
        md.BasePageUrl = "/docs";
        md.SectionLabel = "Documentation";
    }

    /// <summary>
    /// Points the second markdown source at <c>Content/blog</c> under
    /// <c>/blog</c>. The front-matter type on the registration differs
    /// from <see cref="RegisterDocSource"/> so each source parses its own
    /// shape.
    /// </summary>
    public static void RegisterBlogSource(MarkdownContentOptions md)
    {
        md.ContentPath = "Content/blog";
        md.BasePageUrl = "/blog";
        md.SectionLabel = "Blog";
    }

    /// <summary>
    /// Registers a broad catch-all source at <c>Content/</c> that carves out
    /// the <c>blog/</c> subtree via <see cref="MarkdownContentOptions.ExcludePaths"/>
    /// so the blog-specific source below owns it without overlap warnings.
    /// </summary>
    public static void RegisterOverlappingDocSource(MarkdownContentOptions md)
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "/";
        md.ExcludePaths = ["blog"];
    }
}