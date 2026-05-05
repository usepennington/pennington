namespace Pennington.UI.Components;

/// <summary>
/// Returns Tailwind utility class strings for Card and LinkCard, keyed off the component's
/// <c>Color</c> parameter. Static utilities (layout, typography, border-width) live in single
/// const fields; only the color-specific utilities are enumerated per color. Each switch arm
/// is a literal string so MonorailCss.Discovery's IL scanner can extract every permutation —
/// Razor's <c>@Color</c>-style interpolation in inline class attributes is invisible to static
/// scanning.
/// </summary>
internal static class CardColorClasses
{
    private const string WrapperStatic = "group h-full relative rounded-xl border";
    private const string IconStatic = "pt-1";
    private const string TitleStatic = "font-display font-bold pb-2";

    public static string Wrapper(string color) => $"{WrapperStatic} {WrapperColors(color)}";

    public static string LinkWrapper(string color) => $"{WrapperStatic} {LinkWrapperColors(color)}";

    public static string Icon(string color) => $"{IconStatic} {IconColors(color)}";

    public static string Title(string color) => $"{TitleStatic} {TitleColors(color)}";

    private static string WrapperColors(string color) => color switch
    {
        "slate" => "text-slate-900 dark:text-slate-50 border-slate-200 dark:border-slate-900 dark:border-slate-800 bg-slate-200/50 dark:bg-slate-500/10",
        "gray" => "text-gray-900 dark:text-gray-50 border-gray-200 dark:border-gray-900 dark:border-gray-800 bg-gray-200/50 dark:bg-gray-500/10",
        "zinc" => "text-zinc-900 dark:text-zinc-50 border-zinc-200 dark:border-zinc-900 dark:border-zinc-800 bg-zinc-200/50 dark:bg-zinc-500/10",
        "neutral" => "text-neutral-900 dark:text-neutral-50 border-neutral-200 dark:border-neutral-900 dark:border-neutral-800 bg-neutral-200/50 dark:bg-neutral-500/10",
        "stone" => "text-stone-900 dark:text-stone-50 border-stone-200 dark:border-stone-900 dark:border-stone-800 bg-stone-200/50 dark:bg-stone-500/10",
        "red" => "text-red-900 dark:text-red-50 border-red-200 dark:border-red-900 dark:border-red-800 bg-red-200/50 dark:bg-red-500/10",
        "orange" => "text-orange-900 dark:text-orange-50 border-orange-200 dark:border-orange-900 dark:border-orange-800 bg-orange-200/50 dark:bg-orange-500/10",
        "amber" => "text-amber-900 dark:text-amber-50 border-amber-200 dark:border-amber-900 dark:border-amber-800 bg-amber-200/50 dark:bg-amber-500/10",
        "yellow" => "text-yellow-900 dark:text-yellow-50 border-yellow-200 dark:border-yellow-900 dark:border-yellow-800 bg-yellow-200/50 dark:bg-yellow-500/10",
        "lime" => "text-lime-900 dark:text-lime-50 border-lime-200 dark:border-lime-900 dark:border-lime-800 bg-lime-200/50 dark:bg-lime-500/10",
        "green" => "text-green-900 dark:text-green-50 border-green-200 dark:border-green-900 dark:border-green-800 bg-green-200/50 dark:bg-green-500/10",
        "emerald" => "text-emerald-900 dark:text-emerald-50 border-emerald-200 dark:border-emerald-900 dark:border-emerald-800 bg-emerald-200/50 dark:bg-emerald-500/10",
        "teal" => "text-teal-900 dark:text-teal-50 border-teal-200 dark:border-teal-900 dark:border-teal-800 bg-teal-200/50 dark:bg-teal-500/10",
        "cyan" => "text-cyan-900 dark:text-cyan-50 border-cyan-200 dark:border-cyan-900 dark:border-cyan-800 bg-cyan-200/50 dark:bg-cyan-500/10",
        "sky" => "text-sky-900 dark:text-sky-50 border-sky-200 dark:border-sky-900 dark:border-sky-800 bg-sky-200/50 dark:bg-sky-500/10",
        "blue" => "text-blue-900 dark:text-blue-50 border-blue-200 dark:border-blue-900 dark:border-blue-800 bg-blue-200/50 dark:bg-blue-500/10",
        "indigo" => "text-indigo-900 dark:text-indigo-50 border-indigo-200 dark:border-indigo-900 dark:border-indigo-800 bg-indigo-200/50 dark:bg-indigo-500/10",
        "violet" => "text-violet-900 dark:text-violet-50 border-violet-200 dark:border-violet-900 dark:border-violet-800 bg-violet-200/50 dark:bg-violet-500/10",
        "purple" => "text-purple-900 dark:text-purple-50 border-purple-200 dark:border-purple-900 dark:border-purple-800 bg-purple-200/50 dark:bg-purple-500/10",
        "fuchsia" => "text-fuchsia-900 dark:text-fuchsia-50 border-fuchsia-200 dark:border-fuchsia-900 dark:border-fuchsia-800 bg-fuchsia-200/50 dark:bg-fuchsia-500/10",
        "pink" => "text-pink-900 dark:text-pink-50 border-pink-200 dark:border-pink-900 dark:border-pink-800 bg-pink-200/50 dark:bg-pink-500/10",
        "rose" => "text-rose-900 dark:text-rose-50 border-rose-200 dark:border-rose-900 dark:border-rose-800 bg-rose-200/50 dark:bg-rose-500/10",
        "primary" => "text-primary-900 dark:text-primary-50 border-primary-200 dark:border-primary-900 dark:border-primary-800 bg-primary-200/50 dark:bg-primary-500/10",
        "accent" => "text-accent-900 dark:text-accent-50 border-accent-200 dark:border-accent-900 dark:border-accent-800 bg-accent-200/50 dark:bg-accent-500/10",
        "base" => "text-base-900 dark:text-base-50 border-base-200 dark:border-base-900 dark:border-base-800 bg-base-200/50 dark:bg-base-500/10",
        "secondary" => "text-secondary-900 dark:text-secondary-50 border-secondary-200 dark:border-secondary-900 dark:border-secondary-800 bg-secondary-200/50 dark:bg-secondary-500/10",
        "tertiary" => "text-tertiary-900 dark:text-tertiary-50 border-tertiary-200 dark:border-tertiary-900 dark:border-tertiary-800 bg-tertiary-200/50 dark:bg-tertiary-500/10",
        _ => WrapperColors("primary"),
    };

    private static string LinkWrapperColors(string color) => color switch
    {
        "slate" => "border-slate-200 dark:border-slate-900 bg-slate-200/50 hover:bg-slate-500/20 dark:bg-slate-500/10 dark:hover:bg-slate-600/20",
        "gray" => "border-gray-200 dark:border-gray-900 bg-gray-200/50 hover:bg-gray-500/20 dark:bg-gray-500/10 dark:hover:bg-gray-600/20",
        "zinc" => "border-zinc-200 dark:border-zinc-900 bg-zinc-200/50 hover:bg-zinc-500/20 dark:bg-zinc-500/10 dark:hover:bg-zinc-600/20",
        "neutral" => "border-neutral-200 dark:border-neutral-900 bg-neutral-200/50 hover:bg-neutral-500/20 dark:bg-neutral-500/10 dark:hover:bg-neutral-600/20",
        "stone" => "border-stone-200 dark:border-stone-900 bg-stone-200/50 hover:bg-stone-500/20 dark:bg-stone-500/10 dark:hover:bg-stone-600/20",
        "red" => "border-red-200 dark:border-red-900 bg-red-200/50 hover:bg-red-500/20 dark:bg-red-500/10 dark:hover:bg-red-600/20",
        "orange" => "border-orange-200 dark:border-orange-900 bg-orange-200/50 hover:bg-orange-500/20 dark:bg-orange-500/10 dark:hover:bg-orange-600/20",
        "amber" => "border-amber-200 dark:border-amber-900 bg-amber-200/50 hover:bg-amber-500/20 dark:bg-amber-500/10 dark:hover:bg-amber-600/20",
        "yellow" => "border-yellow-200 dark:border-yellow-900 bg-yellow-200/50 hover:bg-yellow-500/20 dark:bg-yellow-500/10 dark:hover:bg-yellow-600/20",
        "lime" => "border-lime-200 dark:border-lime-900 bg-lime-200/50 hover:bg-lime-500/20 dark:bg-lime-500/10 dark:hover:bg-lime-600/20",
        "green" => "border-green-200 dark:border-green-900 bg-green-200/50 hover:bg-green-500/20 dark:bg-green-500/10 dark:hover:bg-green-600/20",
        "emerald" => "border-emerald-200 dark:border-emerald-900 bg-emerald-200/50 hover:bg-emerald-500/20 dark:bg-emerald-500/10 dark:hover:bg-emerald-600/20",
        "teal" => "border-teal-200 dark:border-teal-900 bg-teal-200/50 hover:bg-teal-500/20 dark:bg-teal-500/10 dark:hover:bg-teal-600/20",
        "cyan" => "border-cyan-200 dark:border-cyan-900 bg-cyan-200/50 hover:bg-cyan-500/20 dark:bg-cyan-500/10 dark:hover:bg-cyan-600/20",
        "sky" => "border-sky-200 dark:border-sky-900 bg-sky-200/50 hover:bg-sky-500/20 dark:bg-sky-500/10 dark:hover:bg-sky-600/20",
        "blue" => "border-blue-200 dark:border-blue-900 bg-blue-200/50 hover:bg-blue-500/20 dark:bg-blue-500/10 dark:hover:bg-blue-600/20",
        "indigo" => "border-indigo-200 dark:border-indigo-900 bg-indigo-200/50 hover:bg-indigo-500/20 dark:bg-indigo-500/10 dark:hover:bg-indigo-600/20",
        "violet" => "border-violet-200 dark:border-violet-900 bg-violet-200/50 hover:bg-violet-500/20 dark:bg-violet-500/10 dark:hover:bg-violet-600/20",
        "purple" => "border-purple-200 dark:border-purple-900 bg-purple-200/50 hover:bg-purple-500/20 dark:bg-purple-500/10 dark:hover:bg-purple-600/20",
        "fuchsia" => "border-fuchsia-200 dark:border-fuchsia-900 bg-fuchsia-200/50 hover:bg-fuchsia-500/20 dark:bg-fuchsia-500/10 dark:hover:bg-fuchsia-600/20",
        "pink" => "border-pink-200 dark:border-pink-900 bg-pink-200/50 hover:bg-pink-500/20 dark:bg-pink-500/10 dark:hover:bg-pink-600/20",
        "rose" => "border-rose-200 dark:border-rose-900 bg-rose-200/50 hover:bg-rose-500/20 dark:bg-rose-500/10 dark:hover:bg-rose-600/20",
        "primary" => "border-primary-200 dark:border-primary-900 bg-primary-200/50 hover:bg-primary-500/20 dark:bg-primary-500/10 dark:hover:bg-primary-600/20",
        "accent" => "border-accent-200 dark:border-accent-900 bg-accent-200/50 hover:bg-accent-500/20 dark:bg-accent-500/10 dark:hover:bg-accent-600/20",
        "base" => "border-base-200 dark:border-base-900 bg-base-200/50 hover:bg-base-500/20 dark:bg-base-500/10 dark:hover:bg-base-600/20",
        "secondary" => "border-secondary-200 dark:border-secondary-900 bg-secondary-200/50 hover:bg-secondary-500/20 dark:bg-secondary-500/10 dark:hover:bg-secondary-600/20",
        "tertiary" => "border-tertiary-200 dark:border-tertiary-900 bg-tertiary-200/50 hover:bg-tertiary-500/20 dark:bg-tertiary-500/10 dark:hover:bg-tertiary-600/20",
        _ => LinkWrapperColors("primary"),
    };

    private static string IconColors(string color) => color switch
    {
        "slate" => "text-slate-700 dark:text-slate-500 fill-slate-200 dark:fill-slate-950",
        "gray" => "text-gray-700 dark:text-gray-500 fill-gray-200 dark:fill-gray-950",
        "zinc" => "text-zinc-700 dark:text-zinc-500 fill-zinc-200 dark:fill-zinc-950",
        "neutral" => "text-neutral-700 dark:text-neutral-500 fill-neutral-200 dark:fill-neutral-950",
        "stone" => "text-stone-700 dark:text-stone-500 fill-stone-200 dark:fill-stone-950",
        "red" => "text-red-700 dark:text-red-500 fill-red-200 dark:fill-red-950",
        "orange" => "text-orange-700 dark:text-orange-500 fill-orange-200 dark:fill-orange-950",
        "amber" => "text-amber-700 dark:text-amber-500 fill-amber-200 dark:fill-amber-950",
        "yellow" => "text-yellow-700 dark:text-yellow-500 fill-yellow-200 dark:fill-yellow-950",
        "lime" => "text-lime-700 dark:text-lime-500 fill-lime-200 dark:fill-lime-950",
        "green" => "text-green-700 dark:text-green-500 fill-green-200 dark:fill-green-950",
        "emerald" => "text-emerald-700 dark:text-emerald-500 fill-emerald-200 dark:fill-emerald-950",
        "teal" => "text-teal-700 dark:text-teal-500 fill-teal-200 dark:fill-teal-950",
        "cyan" => "text-cyan-700 dark:text-cyan-500 fill-cyan-200 dark:fill-cyan-950",
        "sky" => "text-sky-700 dark:text-sky-500 fill-sky-200 dark:fill-sky-950",
        "blue" => "text-blue-700 dark:text-blue-500 fill-blue-200 dark:fill-blue-950",
        "indigo" => "text-indigo-700 dark:text-indigo-500 fill-indigo-200 dark:fill-indigo-950",
        "violet" => "text-violet-700 dark:text-violet-500 fill-violet-200 dark:fill-violet-950",
        "purple" => "text-purple-700 dark:text-purple-500 fill-purple-200 dark:fill-purple-950",
        "fuchsia" => "text-fuchsia-700 dark:text-fuchsia-500 fill-fuchsia-200 dark:fill-fuchsia-950",
        "pink" => "text-pink-700 dark:text-pink-500 fill-pink-200 dark:fill-pink-950",
        "rose" => "text-rose-700 dark:text-rose-500 fill-rose-200 dark:fill-rose-950",
        "primary" => "text-primary-700 dark:text-primary-500 fill-primary-200 dark:fill-primary-950",
        "accent" => "text-accent-700 dark:text-accent-500 fill-accent-200 dark:fill-accent-950",
        "base" => "text-base-700 dark:text-base-500 fill-base-200 dark:fill-base-950",
        "secondary" => "text-secondary-700 dark:text-secondary-500 fill-secondary-200 dark:fill-secondary-950",
        "tertiary" => "text-tertiary-700 dark:text-tertiary-500 fill-tertiary-200 dark:fill-tertiary-950",
        _ => IconColors("primary"),
    };

    private static string TitleColors(string color) => color switch
    {
        "slate" => "text-slate-900 dark:text-slate-50",
        "gray" => "text-gray-900 dark:text-gray-50",
        "zinc" => "text-zinc-900 dark:text-zinc-50",
        "neutral" => "text-neutral-900 dark:text-neutral-50",
        "stone" => "text-stone-900 dark:text-stone-50",
        "red" => "text-red-900 dark:text-red-50",
        "orange" => "text-orange-900 dark:text-orange-50",
        "amber" => "text-amber-900 dark:text-amber-50",
        "yellow" => "text-yellow-900 dark:text-yellow-50",
        "lime" => "text-lime-900 dark:text-lime-50",
        "green" => "text-green-900 dark:text-green-50",
        "emerald" => "text-emerald-900 dark:text-emerald-50",
        "teal" => "text-teal-900 dark:text-teal-50",
        "cyan" => "text-cyan-900 dark:text-cyan-50",
        "sky" => "text-sky-900 dark:text-sky-50",
        "blue" => "text-blue-900 dark:text-blue-50",
        "indigo" => "text-indigo-900 dark:text-indigo-50",
        "violet" => "text-violet-900 dark:text-violet-50",
        "purple" => "text-purple-900 dark:text-purple-50",
        "fuchsia" => "text-fuchsia-900 dark:text-fuchsia-50",
        "pink" => "text-pink-900 dark:text-pink-50",
        "rose" => "text-rose-900 dark:text-rose-50",
        "primary" => "text-primary-900 dark:text-primary-50",
        "accent" => "text-accent-900 dark:text-accent-50",
        "base" => "text-base-900 dark:text-base-50",
        "secondary" => "text-secondary-900 dark:text-secondary-50",
        "tertiary" => "text-tertiary-900 dark:text-tertiary-50",
        _ => TitleColors("primary"),
    };
}
