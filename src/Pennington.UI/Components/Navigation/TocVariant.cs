namespace Pennington.UI.Components.Navigation;

/// <summary>
/// Visual archetype for <see cref="TableOfContentsNavigation"/>. A variant names a cohesive
/// look; per-instance <c>*Class</c> parameters Tailwind-merge on top of it for one-off tweaks.
/// The class strings live in <see cref="TocVariantStyles.For"/> — inline literals in a method
/// body, so an edit hot-reloads under <c>dotnet watch</c> (unlike a static dictionary, whose
/// initializer only runs once).
/// </summary>
public enum TocVariant
{
    /// <summary>Bordered rail: a left border with the active child marked by a colored edge.
    /// The bare Pennington.UI default.</summary>
    Rail,

    /// <summary>Rounded pill buttons with a tinted active background — the DocSite sidebar look.</summary>
    Pill,
}

/// <summary>
/// Per-variant base class strings for each slot <see cref="TableOfContentsNavigation"/> renders.
/// One <see cref="For"/> call returns every slot for a variant; the component merges any
/// per-instance <c>*Class</c> parameter over these.
/// </summary>
public static class TocVariantStyles
{
    /// <summary>Base classes per rendered slot, before any per-instance merge.</summary>
    public readonly record struct Slots(
        string List,
        string Section,
        string SectionTitle,
        string SectionList,
        string Link,
        string TopLink);

    /// <summary>Resolves the slot bases for <paramref name="variant"/>.</summary>
    public static Slots For(TocVariant variant)
    {
        switch (variant)
        {
            case TocVariant.Pill:
            {
                // Child links and top-level leaf links render the same pill; the active state is
                // a tinted background, matching the area-pill / outline-rail family.
                const string link =
                    "flex items-center gap-1.5 px-2 py-2 rounded-md text-sm leading-snug " +
                    "transition-colors duration-150 " +
                    "text-base-500 dark:text-base-400 " +
                    "hover:text-base-900 hover:bg-base-100 dark:hover:text-base-50 dark:hover:bg-base-900 " +
                    "data-[current=true]:text-primary-700 data-[current=true]:bg-primary-500/8 " +
                    "dark:data-[current=true]:text-primary-300 dark:data-[current=true]:bg-primary-300/8 " +
                    "data-[current=true]:font-medium";

                return new Slots(
                    List: "flex flex-col ",
                    Section: "",
                    SectionTitle: "font-display text-[12.5px] font-semibold mb-2 mt-8 [li:first-child>&]:mt-2 px-2.5 text-base-700 dark:text-base-200 uppercase",
                    SectionList: "mt-1 flex flex-col gap-0.5",
                    Link: link,
                    TopLink: link);
            }

            default:
                return new Slots(
                    List: "flex flex-col gap-8",
                    Section: "",
                    SectionTitle: "font-display font-medium text-base-900 dark:text-base-50",
                    SectionList: "mt-4",
                    Link: "block text-sm w-full border-l pl-3.5 py-1.5 transition-colors transition-300 border-base-300 dark:border-base-800 data-[current=true]:border-primary-400 text-base-500 dark:text-base-400 data-[current=true]:text-primary-800 dark:data-[current=true]:text-primary-500 hover:text-accent-400 dark:hover:text-base-50",
                    TopLink: "block w-full py-1 transition-colors transition-300 text-base-700 dark:text-base-400 data-[current=true]:text-primary-800 dark:data-[current=true]:text-primary-500 hover:text-accent-400 dark:hover:text-base-50");
        }
    }
}
