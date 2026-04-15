namespace BeyondLocaleExample;

using Pennington.Localization;

/// <summary>
/// Registers UI string translations for the two locales this example runs.
/// Fenced by the example block in reference/options/translations so docs
/// show a real consumer calling both <c>TranslationOptions.Add</c> overloads.
/// </summary>
public static class TranslationRegistration
{
    /// <summary>
    /// Populates <paramref name="opts"/> with nav strings for the English
    /// default and the Spanish locale. Uses the per-key overload for one
    /// entry and the bulk dictionary overload for the rest so both
    /// signatures appear in the fenced body.
    /// </summary>
    public static void Register(TranslationOptions opts)
    {
        opts.Add("en", "nav.home", "Home");
        opts.Add("es", "nav.home", "Inicio");
        opts.Add("es", new Dictionary<string, string>
        {
            ["nav.docs"] = "Documentación",
            ["nav.blog"] = "Blog",
        });
    }
}