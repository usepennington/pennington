namespace Pennington.UI.Styling;

/// <summary>
/// Catalog of style-slot keys accepted by <see cref="StyleRegistry"/>. Each constant names one
/// overridable CSS class string on a Pennington.UI component; pass them as dictionary keys in
/// <c>DocSiteOptions.Styles</c> / <c>BlogSiteOptions.Styles</c>. Keys are case-insensitive.
/// </summary>
public static class StyleKeys
{
    /// <summary>Gap classes on the outer <c>&lt;ul&gt;</c> of <c>TableOfContentsNavigation</c>.</summary>
    public const string TocListGap = "toc.list-gap";

    /// <summary>Classes on the nested <c>&lt;ul&gt;</c> that holds a TOC section's child entries.</summary>
    public const string TocChildList = "toc.child-list";

    /// <summary>Layout and typography classes on TOC section-header elements.</summary>
    public const string TocSectionHeaderStructure = "toc.section-header-structure";

    /// <summary>Color classes on TOC section-header text.</summary>
    public const string TocSectionHeaderColor = "toc.section-header-color";

    /// <summary>Layout classes on each child-level TOC <c>&lt;a&gt;</c>.</summary>
    public const string TocLinkStructure = "toc.link-structure";

    /// <summary>Color and <c>data-current=true</c> state classes on each child-level TOC <c>&lt;a&gt;</c>.</summary>
    public const string TocLinkColor = "toc.link-color";

    /// <summary>Layout classes on a leaf root-level TOC <c>&lt;a&gt;</c> (a top-level entry with no children).</summary>
    public const string TocRootLinkStructure = "toc.root-link-structure";

    /// <summary>Color and <c>data-current=true</c> state classes on a leaf root-level TOC <c>&lt;a&gt;</c>.</summary>
    public const string TocRootLinkColor = "toc.root-link-color";

    /// <summary>Layout and typography classes on the eyebrow above the <c>OutlineNavigation</c> list.</summary>
    public const string OutlineTitleStructure = "outline.title-structure";

    /// <summary>Color classes on the <c>OutlineNavigation</c> eyebrow text.</summary>
    public const string OutlineTitleColor = "outline.title-color";

    /// <summary>Layout and border classes on the outline's outer <c>data-role="page-outline"</c> container.</summary>
    public const string OutlineContainerStructure = "outline.container-structure";

    /// <summary>Color classes on the outline's outer container, composed after <see cref="OutlineContainerStructure"/>.</summary>
    public const string OutlineContainerColor = "outline.container-color";

    /// <summary>Layout classes on the outline <c>&lt;ul&gt;</c>.</summary>
    public const string OutlineListStructure = "outline.list-structure";

    /// <summary>Color classes on the outline <c>&lt;ul&gt;</c>, composed after <see cref="OutlineListStructure"/>.</summary>
    public const string OutlineListColor = "outline.list-color";

    /// <summary>Layout classes the client-side outline script applies to each generated outline <c>&lt;a&gt;</c>.</summary>
    public const string OutlineLinkStructure = "outline.link-structure";

    /// <summary>Color and <c>data-selected=true</c> state classes the client-side outline script applies to each generated outline <c>&lt;a&gt;</c>.</summary>
    public const string OutlineLinkColor = "outline.link-color";
}
