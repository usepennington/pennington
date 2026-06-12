namespace Pennington.UI.Styling;

/// <summary>
/// Catalog of style-slot keys accepted by <see cref="StyleRegistry"/>. One slot per rendered
/// element — Tailwind-aware merging lets an override change a single utility without restating
/// the rest, so layout and color share a slot. Pass them as dictionary keys in
/// <c>DocSiteOptions.Styles</c> / <c>BlogSiteOptions.Styles</c>. Keys are case-insensitive.
/// </summary>
public static class StyleKeys
{
    /// <summary>The outer <c>&lt;ul&gt;</c> of <c>TableOfContentsNavigation</c> — list layout and the gap between top-level entries.</summary>
    public const string TocList = "toc.list";

    /// <summary>Each top-level <c>&lt;li&gt;</c> of <c>TableOfContentsNavigation</c>. Empty by default — a hook for per-section margins.</summary>
    public const string TocSection = "toc.section";

    /// <summary>A TOC section's label — the plain <c>&lt;div&gt;</c> for empty-route entries, or the <c>&lt;a&gt;</c> when a top-level entry has children.</summary>
    public const string TocSectionTitle = "toc.section-title";

    /// <summary>The nested <c>&lt;ul&gt;</c> that holds a TOC section's child entries.</summary>
    public const string TocSectionList = "toc.section-list";

    /// <summary>Each child-level TOC <c>&lt;a&gt;</c>, including its <c>data-[current=true]</c> state styling.</summary>
    public const string TocLink = "toc.link";

    /// <summary>A top-level leaf TOC <c>&lt;a&gt;</c> (an entry with no children), including its <c>data-[current=true]</c> state styling.</summary>
    public const string TocTopLink = "toc.top-link";

    /// <summary>The eyebrow above the <c>OutlineNavigation</c> list.</summary>
    public const string OutlineTitle = "outline.title";

    /// <summary>The outline's outer <c>data-role="page-outline"</c> container; <c>relative</c> stays hardcoded for marker positioning.</summary>
    public const string OutlineContainer = "outline.container";

    /// <summary>The moving highlight bar that tracks the active heading; <c>absolute</c> and <c>opacity-0</c> stay hardcoded — the client script positions it and toggles its opacity.</summary>
    public const string OutlineMarker = "outline.marker";

    /// <summary>The outline <c>&lt;ul&gt;</c>.</summary>
    public const string OutlineList = "outline.list";

    /// <summary>Each outline <c>&lt;a&gt;</c> the client script generates, including its <c>data-[selected=true]</c> state styling.</summary>
    public const string OutlineLink = "outline.link";

    /// <summary>Extra classes the client script appends to nested (H3-level) outline links.</summary>
    public const string OutlineNestedLink = "outline.nested-link";
}
