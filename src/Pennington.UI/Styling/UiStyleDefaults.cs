namespace Pennington.UI.Styling;

using System.Collections.Frozen;

/// <summary>
/// Built-in class strings for every <see cref="StyleKeys"/> slot — the values the components
/// render when neither a template skin nor a consumer override is in play. Every value is a
/// compile-time literal so MonorailCss.Discovery's IL scanner registers it.
/// </summary>
internal static class UiStyleDefaults
{
    /// <summary>Component default per slot key; the registry's bottom layer and its valid-key set.</summary>
    public static FrozenDictionary<string, string> Defaults { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [StyleKeys.TocListGap] = "gap-4",
        [StyleKeys.TocChildList] = "mt-4",
        [StyleKeys.TocSectionHeaderStructure] = "font-display font-medium first:pt-0",
        [StyleKeys.TocSectionHeaderColor] = "text-base-900 dark:text-base-50",
        [StyleKeys.TocLinkStructure] = "block text-sm w-full border-l pl-3.5 py-1.5",
        [StyleKeys.TocLinkColor] = "transition-colors transition-300 border-base-300 dark:border-base-800 data-[current=true]:border-primary-400 text-base-500 dark:text-base-400 data-[current=true]:text-primary-800 dark:data-[current=true]:text-primary-500 hover:text-accent-400 dark:hover:text-base-50",
        [StyleKeys.TocRootLinkStructure] = "block w-full py-1",
        [StyleKeys.TocRootLinkColor] = "transition-colors transition-300 text-base-700 dark:text-base-400 data-[current=true]:text-primary-800 dark:data-[current=true]:text-primary-500 hover:text-accent-400 dark:hover:text-base-50",
        [StyleKeys.OutlineTitleStructure] = "font-display text-[13px] font-semibold mb-3",
        [StyleKeys.OutlineTitleColor] = "text-base-600 dark:text-base-300",
        [StyleKeys.OutlineContainerStructure] = "border-l border-base-200 dark:border-base-800",
        [StyleKeys.OutlineContainerColor] = "",
        [StyleKeys.OutlineListStructure] = "list-none pl-4",
        [StyleKeys.OutlineListColor] = "text-base-500 dark:text-base-400",
        [StyleKeys.OutlineLinkStructure] = "block py-1 ml-[calc(-1*(4em-1px))] pl-[calc(4em+1px)]",
        [StyleKeys.OutlineLinkColor] = "transition-colors duration-150 hover:text-base-900 dark:hover:text-base-50 data-[selected=true]:text-primary-700 dark:data-[selected=true]:text-primary-300 data-[selected=true]:font-medium",
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>Component default for <paramref name="key"/> — the no-registry fallback used when a host never calls <c>AddPenningtonStyles</c>.</summary>
    public static string Get(string key) => Defaults[key];
}
