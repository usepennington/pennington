namespace Pennington.DocSite;

using System.Collections.Frozen;
using Pennington.UI.Styling;

/// <summary>
/// The DocSite template's style-registry skin — per-slot replacements applied wholesale over the
/// Pennington.UI component defaults, with consumer values from <see cref="DocSiteOptions.Styles"/>
/// merged on top. No <c>outline.*</c> entries: the component defaults are the DocSite outline
/// look. <c>toc.list-gap</c> stays a per-instance parameter in the DocSite layout.
/// </summary>
public static class DocSiteStyleSkin
{
    // Shared TOC styling — the area-aware and single-tree sidebar branches render the same
    // skin, and root-level leaf links share the child-link slot values. The pill-style nav
    // links replaced the previous border-l indicator; the active state is a tinted background,
    // matching the area-pill / outline-rail family.
    private const string PillLinkStructure =
        "flex items-center gap-1.5 px-2.5 py-1.5 rounded-md text-sm leading-snug";

    private const string PillLinkColor =
        "transition-colors duration-150 " +
        "text-base-500 dark:text-base-400 " +
        "hover:text-base-900 hover:bg-base-100 dark:hover:text-base-50 dark:hover:bg-base-900 " +
        "data-[current=true]:text-primary-700 data-[current=true]:bg-primary-500/8 " +
        "dark:data-[current=true]:text-primary-300 dark:data-[current=true]:bg-primary-300/8 " +
        "data-[current=true]:font-medium";

    /// <summary>Slot-key → class string replacements for the DocSite chrome.</summary>
    public static FrozenDictionary<string, string> Styles { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [StyleKeys.TocChildList] = "mt-1 flex flex-col gap-0.5",
        [StyleKeys.TocSectionHeaderStructure] = "font-display text-[12.5px] font-semibold mt-6 first:mt-0 mb-2 px-2.5",
        [StyleKeys.TocSectionHeaderColor] = "text-base-700 dark:text-base-200 uppercase",
        [StyleKeys.TocLinkStructure] = PillLinkStructure,
        [StyleKeys.TocLinkColor] = PillLinkColor,
        [StyleKeys.TocRootLinkStructure] = PillLinkStructure,
        [StyleKeys.TocRootLinkColor] = PillLinkColor,
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}
