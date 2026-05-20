namespace Pennington.UI.Components;

/// <summary>
/// Maps a Card / LinkCard <c>Color</c> parameter to its single <c>card-tint-*</c> utility
/// class. Each switch arm is a compile-time literal so MonorailCss.Discovery's IL scanner
/// registers it — Razor's <c>@Color</c>-style interpolation in markup is invisible to the
/// scan, and discovery does not read rendered HTML. The <c>card-tint-*</c> wildcard utility
/// (in <c>Pennington.MonorailCss</c>) resolves the class to <c>--card-tint: var(--color-{c}-500)</c>;
/// the <c>.pn-card</c> component rules derive every surface from that one variable via
/// <c>color-mix()</c>. This is one short class per color — not the styled permutation of
/// background, border, text, and fill the palette was previously enumerated across.
/// </summary>
internal static class CardColorClasses
{
    /// <summary>Returns the <c>card-tint-*</c> class for <paramref name="color"/>, defaulting to <c>primary</c>.</summary>
    public static string Tint(string color) => color switch
    {
        "slate" => "card-tint-slate-500",
        "gray" => "card-tint-gray-500",
        "zinc" => "card-tint-zinc-500",
        "neutral" => "card-tint-neutral-500",
        "stone" => "card-tint-stone-500",
        "red" => "card-tint-red-500",
        "orange" => "card-tint-orange-500",
        "amber" => "card-tint-amber-500",
        "yellow" => "card-tint-yellow-500",
        "lime" => "card-tint-lime-500",
        "green" => "card-tint-green-500",
        "emerald" => "card-tint-emerald-500",
        "teal" => "card-tint-teal-500",
        "cyan" => "card-tint-cyan-500",
        "sky" => "card-tint-sky-500",
        "blue" => "card-tint-blue-500",
        "indigo" => "card-tint-indigo-500",
        "violet" => "card-tint-violet-500",
        "purple" => "card-tint-purple-500",
        "fuchsia" => "card-tint-fuchsia-500",
        "pink" => "card-tint-pink-500",
        "rose" => "card-tint-rose-500",
        "primary" => "card-tint-primary-500",
        "accent" => "card-tint-accent-500",
        "base" => "card-tint-base-500",
        "secondary" => "card-tint-secondary-500",
        "tertiary" => "card-tint-tertiary-500",
        _ => "card-tint-primary-500",
    };
}