namespace Pennington.Cli.Diag;

using System.CommandLine;
using Content;
using Generation;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;

/// <summary><c>diag translation</c> (alias <c>i18n</c>) — per-locale page coverage, fallback pages, and untranslated UI strings.</summary>
internal sealed class DiagTranslationCommand : IDiagCommand
{
    /// <inheritdoc/>
    public string Name => "translation";

    /// <inheritdoc/>
    public string Description => "Diagnose localization: per-locale coverage, fallback pages, and untranslated UI strings.";

    /// <inheritdoc/>
    public Command Build(IServiceProvider services, TextWriter output)
    {
        var localeOption = new Option<string>("--locale")
        {
            Description = "Limit the detailed breakdown to one locale.",
        };

        var command = new Command(Name, Description);
        command.Aliases.Add("i18n");
        command.Options.Add(localeOption);
        command.SetAction(async (parseResult, _) =>
        {
            var localeFilter = parseResult.GetValue(localeOption);
            var options = services.GetRequiredService<PenningtonOptions>();
            var localization = options.Localization;
            var defaultLocale = localization.DefaultLocale;

            if (!localization.IsMultiLocale)
            {
                output.WriteLine($"Site is single-locale (default: {defaultLocale}). Nothing to diagnose.");
                return 0;
            }

            var toc = await services.GetServices<IContentService>().CollectTocEntriesAsync();

            // Real (non-fallback) locale-stripped paths per locale; default-locale set is the source of truth.
            var realByLocale = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            var defaultKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in toc)
            {
                var route = item.Route;
                if (route.IsFallback)
                {
                    continue;
                }

                var locale = item.Locale ?? (string.IsNullOrEmpty(route.Locale) ? defaultLocale : route.Locale);
                var key = localization.StripLocalePrefix(route.CanonicalPath.Value, locale);
                if (!realByLocale.TryGetValue(locale, out var set))
                {
                    set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    realByLocale[locale] = set;
                }

                set.Add(key);
                if (string.Equals(locale, defaultLocale, StringComparison.OrdinalIgnoreCase))
                {
                    defaultKeys.Add(key);
                }
            }

            var translations = options.Translations;
            var defaultUiKeys = translations.GetAll(defaultLocale).Keys;

            output.WriteLine($"Translation — default: {defaultLocale}, {localization.Locales.Count} locales");
            output.WriteLine();
            output.WriteLine($"  {"LOCALE",-8}{"PAGES",-7}{"TRANSLATED",-14}{"FALLBACK",-10}{"UI GAPS"}");
            foreach (var (code, _) in localization.Locales)
            {
                var isDefault = string.Equals(code, defaultLocale, StringComparison.OrdinalIgnoreCase);
                var real = realByLocale.GetValueOrDefault(code) ?? [];
                var translated = isDefault ? defaultKeys.Count : defaultKeys.Count(real.Contains);
                var fallback = defaultKeys.Count - translated;
                var percent = defaultKeys.Count == 0 ? 100 : (int)Math.Round(translated * 100.0 / defaultKeys.Count);
                var uiGaps = isDefault
                    ? "—"
                    : defaultUiKeys.Except(translations.GetAll(code).Keys, StringComparer.OrdinalIgnoreCase).Count().ToString();
                output.WriteLine($"  {code,-8}{defaultKeys.Count,-7}{$"{translated} ({percent}%)",-14}{fallback,-10}{uiGaps}");
            }

            output.WriteLine();

            foreach (var (code, _) in localization.Locales)
            {
                if (string.Equals(code, defaultLocale, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(localeFilter) && !string.Equals(code, localeFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var real = realByLocale.GetValueOrDefault(code) ?? [];
                var fallbackPages = defaultKeys.Where(k => !real.Contains(k)).OrderBy(k => k, StringComparer.Ordinal).ToList();
                if (fallbackPages.Count > 0)
                {
                    output.WriteLine($"Fallback pages ({code}):");
                    foreach (var page in fallbackPages)
                    {
                        output.WriteLine($"  {page}");
                    }

                    output.WriteLine();
                }

                var uiGapKeys = defaultUiKeys
                    .Except(translations.GetAll(code).Keys, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(k => k, StringComparer.Ordinal)
                    .ToList();
                if (uiGapKeys.Count > 0)
                {
                    output.WriteLine($"Missing UI strings ({code}): {string.Join(", ", uiGapKeys)}");
                    output.WriteLine();
                }
            }

            // Optional git-based audit (Pennington.TranslationAudit). Diagnostics land in the audit
            // cache keyed by the auditor's "translation.audit/" source prefix; surface them if present.
            await services.GetRequiredService<AuditRunner>().WaitForInitialPassAsync();
            var auditDiagnostics = services.GetRequiredService<IAuditCache>().Diagnostics
                .Where(d => d.SourceFile?.StartsWith("translation.audit/", StringComparison.Ordinal) == true)
                .ToList();
            if (auditDiagnostics.Count > 0)
            {
                output.WriteLine("Translation audit (git):");
                foreach (var diagnostic in auditDiagnostics)
                {
                    output.WriteLine($"  {diagnostic.Message}");
                }

                output.WriteLine();
            }

            return 0;
        });
        return command;
    }
}
