namespace ExtensibilityLabExample;

using MonorailCss.Theme;
using Pennington.MonorailCss;

/// <summary>
/// Factorizes the <see cref="MonorailCssOptions"/> for the Lab into a single
/// helper so the how-to <c>/how-to/configuration/monorail-css</c> can fence
/// the <c>CustomCssFrameworkSettings</c> shape from a bare
/// <c>AddPennington</c> host.
/// </summary>
public static class MonorailCssCustomization
{
    /// <summary>
    /// Build the options passed to <c>services.AddMonorailCss(...)</c>.
    /// Pairs a <see cref="NamedColorScheme"/> with a
    /// <see cref="MonorailCssOptions.CustomCssFrameworkSettings"/> delegate
    /// that tweaks the framework defaults after the color palette is applied.
    /// </summary>
    public static MonorailCssOptions BuildOptions() => new()
    {
        ColorScheme = new NamedColorScheme
        {
            PrimaryColorName = ColorNames.Sky,
            AccentColorName = ColorNames.Emerald,
            TertiaryOneColorName = ColorNames.Amber,
            TertiaryTwoColorName = ColorNames.Violet,
            BaseColorName = ColorNames.Slate,
        },
        CustomCssFrameworkSettings = settings => settings with
        {
            Applies = settings.Applies
                .SetItem(".lab-tabs", "flex flex-col bg-base-50 border border-base-300 rounded-lg")
                .SetItem(".lab-tabs-list", "flex flex-row gap-2 border-b border-base-200 px-3 pt-2")
                .SetItem(".lab-tabs-button", "py-1.5 text-sm text-base-700 data-[selected=true]:text-primary-700 data-[selected=true]:border-b data-[selected=true]:border-primary-600")
                .SetItem(".lab-tabs-panel", "hidden data-[selected=true]:block px-3 py-3"),
        },
    };
}