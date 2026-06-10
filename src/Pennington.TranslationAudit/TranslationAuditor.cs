namespace Pennington.TranslationAudit;

using System.Collections.Immutable;
using Pennington.Generation;

/// <summary>
/// <see cref="IBuildAuditor"/> that pairs each default-locale page with its translations
/// in every other configured locale and classifies the pair as Up-to-date, Outdated
/// (translation predates source's last commit) or Missing (no translation file).
/// </summary>
public sealed class TranslationAuditor : IBuildAuditor
{
    private readonly TranslationAuditOptions _options;
    private readonly IGitHistoryReader _git;

    /// <summary>Stable identifier surfaced on every diagnostic this auditor emits.</summary>
    public string Code => "translation.audit";

    /// <summary>Wires the auditor to its options and git history reader.</summary>
    public TranslationAuditor(TranslationAuditOptions options, IGitHistoryReader git)
    {
        _options = options;
        _git = git;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<BuildDiagnostic>> AuditAsync(BuildAuditContext context, CancellationToken cancellationToken)
    {
        var localization = context.Localization;
        var nonDefaultLocales = localization.Locales
            .Where(kv => !string.Equals(kv.Key, localization.DefaultLocale, StringComparison.OrdinalIgnoreCase))
            .Where(kv => _options.IncludedLocales is null || _options.IncludedLocales.Contains(kv.Key))
            .ToList();

        if (nonDefaultLocales.Count == 0)
        {
            return Task.FromResult<IReadOnlyList<BuildDiagnostic>>([]);
        }

        // Bucket every TOC entry by locale-stripped canonical path. The default-locale entry
        // is the source of truth for that bucket; non-default-locale entries are translations.
        // Fallback routes (synthesized when a translation file doesn't exist on disk) are
        // skipped here — the absence of a real entry IS the signal we report below.
        var byCanonicalPath = new Dictionary<string, PageEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in context.Pages)
        {
            var route = item.Route;
            if (route.SourceFile is not { Value: var sourcePath } || !sourcePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (route.IsFallback)
            {
                continue;
            }

            var locale = item.Locale ?? (string.IsNullOrEmpty(route.Locale) ? localization.DefaultLocale : route.Locale);
            var key = localization.StripLocalePrefix(route.CanonicalPath.Value, locale);

            if (!byCanonicalPath.TryGetValue(key, out var entry))
            {
                entry = new PageEntry();
                byCanonicalPath[key] = entry;
            }

            if (string.Equals(locale, localization.DefaultLocale, StringComparison.OrdinalIgnoreCase))
            {
                entry.SourceRoute = route;
                entry.SourceFile = sourcePath;
                entry.SourceTitle = item.Title;
            }
            else
            {
                entry.Translations[locale] = (route, sourcePath);
            }
        }

        // Git state is constant across one audit run, so each distinct file's history is
        // walked at most once. Without this, GetLatestCommit(page.SourceFile) below re-walks
        // the same source file once per non-default locale (the reader has no cache).
        var commitCache = new Dictionary<string, CommitInfo?>(StringComparer.Ordinal);

        var diagnostics = ImmutableList.CreateBuilder<BuildDiagnostic>();
        foreach (var (locale, info) in nonDefaultLocales)
        {
            foreach (var page in byCanonicalPath.Values)
            {
                if (page.SourceRoute is null || page.SourceFile is null)
                {
                    continue;
                }

                if (!page.Translations.TryGetValue(locale, out var translation))
                {
                    if (!_options.ReportMissing)
                    {
                        continue;
                    }

                    diagnostics.Add(new BuildDiagnostic(
                        Severity: _options.MissingSeverity,
                        Route: page.SourceRoute,
                        Message: $"Missing {info.DisplayName} ({locale}) translation for \"{page.SourceTitle}\" ({page.SourceRoute.CanonicalPath.Value}).",
                        SourceFile: $"{Code}/missing/{locale}"));
                    continue;
                }

                var sourceCommit = GetLatestCommit(commitCache, page.SourceFile);
                var translationCommit = GetLatestCommit(commitCache, translation.SourcePath);
                if (sourceCommit is null || translationCommit is null)
                {
                    continue;
                }

                if (translationCommit.When >= sourceCommit.When)
                {
                    continue;
                }

                diagnostics.Add(new BuildDiagnostic(
                    Severity: _options.OutdatedSeverity,
                    Route: translation.Route,
                    Message: $"{info.DisplayName} ({locale}) translation of \"{page.SourceTitle}\" is outdated: source {sourceCommit.Sha} ({sourceCommit.When:yyyy-MM-dd}) is newer than translation {translationCommit.Sha} ({translationCommit.When:yyyy-MM-dd}).",
                    SourceFile: $"{Code}/outdated/{locale}"));
            }
        }

        return Task.FromResult<IReadOnlyList<BuildDiagnostic>>(diagnostics.ToImmutable());
    }

    private CommitInfo? GetLatestCommit(Dictionary<string, CommitInfo?> cache, string path)
    {
        if (cache.TryGetValue(path, out var commit))
        {
            return commit;
        }

        commit = _git.GetLatestCommit(path);
        cache[path] = commit;
        return commit;
    }

    private sealed class PageEntry
    {
        public Routing.ContentRoute? SourceRoute { get; set; }
        public string? SourceFile { get; set; }
        public string? SourceTitle { get; set; }
        public Dictionary<string, (Routing.ContentRoute Route, string SourcePath)> Translations { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}