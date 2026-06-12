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
        // gap-8 matches the pre-slot rendering, where a gap-4 list and an mt-4 on each
        // top-level item summed to 2rem between entries; the list gap is now the one owner
        // of top-level rhythm, with toc.section left as an empty hook.
        [StyleKeys.TocList] = "flex flex-col gap-8",
        [StyleKeys.TocSection] = "",
        [StyleKeys.TocSectionTitle] = "font-display font-medium text-base-900 dark:text-base-50",
        [StyleKeys.TocSectionList] = "mt-4",
        [StyleKeys.TocLink] = "block text-sm w-full border-l pl-3.5 py-1.5 transition-colors transition-300 border-base-300 dark:border-base-800 data-[current=true]:border-primary-400 text-base-500 dark:text-base-400 data-[current=true]:text-primary-800 dark:data-[current=true]:text-primary-500 hover:text-accent-400 dark:hover:text-base-50",
        [StyleKeys.TocTopLink] = "block w-full py-1 transition-colors transition-300 text-base-700 dark:text-base-400 data-[current=true]:text-primary-800 dark:data-[current=true]:text-primary-500 hover:text-accent-400 dark:hover:text-base-50",
        [StyleKeys.OutlineTitle] = "font-display text-[13px] font-semibold mb-3 text-base-600 dark:text-base-300",
        [StyleKeys.OutlineContainer] = "border-l border-base-200 dark:border-base-800",
        [StyleKeys.OutlineMarker] = "left-[-1px] w-[2px] rounded-sm bg-primary-600 dark:bg-primary-300 transition-all duration-500",
        [StyleKeys.OutlineList] = "list-none pl-4 text-base-500 dark:text-base-400",
        [StyleKeys.OutlineLink] = "block py-1 ml-[calc(-1*(4em-1px))] pl-[calc(4em+1px)] transition-colors duration-150 hover:text-base-900 dark:hover:text-base-50 data-[selected=true]:text-primary-700 dark:data-[selected=true]:text-primary-300 data-[selected=true]:font-medium",
        [StyleKeys.OutlineNestedLink] = "pl-4",
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>Component default for <paramref name="key"/> — the no-registry fallback used when a host never calls <c>AddPenningtonStyles</c>.</summary>
    public static string Get(string key) => Defaults[key];
}
