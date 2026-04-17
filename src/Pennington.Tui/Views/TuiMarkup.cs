namespace Pennington.Tui.Views;

/// <summary>
/// Markup helpers for XenoAtom terminal rendering. <c>Markup</c> and <c>LogControl.AppendMarkupLine</c>
/// interpret <c>[tag]...[/]</c>, so any user-controlled text (URLs with brackets, log
/// messages with "[id]") has to be escaped or the parser will either mangle the row or throw.
/// </summary>
internal static class TuiMarkup
{
    /// <summary>Double-escape <c>[</c> and <c>]</c> so markup renders the literal characters.</summary>
    public static string Escape(string? value) =>
        string.IsNullOrEmpty(value) ? "" : value.Replace("[", "[[").Replace("]", "]]");
}
