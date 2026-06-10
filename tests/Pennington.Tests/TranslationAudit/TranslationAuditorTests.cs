using System.Collections.Immutable;
using Pennington.Content;
using Pennington.Generation;
using Pennington.Infrastructure;
using Pennington.Localization;
using Pennington.Routing;
using Pennington.TranslationAudit;

namespace Pennington.Tests.TranslationAudit;

public class TranslationAuditorTests
{
    [Fact]
    public async Task AuditAsync_WalksSourceHistoryOncePerFile_NotOncePerLocale()
    {
        var localization = Localization("en", "fr", "de");

        // One source page paired with a translation in each of two non-default locales.
        var pages = ImmutableList.Create(
            Page("/page/", "/repo/page.md", "en"),
            Page("/fr/page/", "/repo/fr/page.md", "fr"),
            Page("/de/page/", "/repo/de/page.md", "de"));

        var when = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var git = new CountingGitHistoryReader(new(StringComparer.Ordinal)
        {
            ["/repo/page.md"] = new CommitInfo("aaaaaaa", when),
            ["/repo/fr/page.md"] = new CommitInfo("bbbbbbb", when),
            ["/repo/de/page.md"] = new CommitInfo("ccccccc", when),
        });

        var auditor = new TranslationAuditor(new TranslationAuditOptions(), git);

        await auditor.AuditAsync(
            new BuildAuditContext(pages, localization),
            TestContext.Current.CancellationToken);

        // The source file is shared by both the fr and de buckets. Before memoization its
        // history was walked once per non-default locale (2x); it must now be walked once.
        git.Calls["/repo/page.md"].ShouldBe(1);
        git.Calls["/repo/fr/page.md"].ShouldBe(1);
        git.Calls["/repo/de/page.md"].ShouldBe(1);
    }

    [Fact]
    public async Task AuditAsync_TranslationOlderThanSource_ReportsOutdated()
    {
        var localization = Localization("en", "fr");
        var pages = ImmutableList.Create(
            Page("/page/", "/repo/page.md", "en"),
            Page("/fr/page/", "/repo/fr/page.md", "fr"));

        var git = new CountingGitHistoryReader(new(StringComparer.Ordinal)
        {
            ["/repo/page.md"] = new CommitInfo("src1234", new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero)),
            ["/repo/fr/page.md"] = new CommitInfo("tr12345", new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)),
        });

        var auditor = new TranslationAuditor(new TranslationAuditOptions(), git);

        var diagnostics = await auditor.AuditAsync(
            new BuildAuditContext(pages, localization),
            TestContext.Current.CancellationToken);

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Message.ShouldContain("outdated");
        diagnostics[0].SourceFile.ShouldBe("translation.audit/outdated/fr");
    }

    [Fact]
    public async Task AuditAsync_TranslationNewerThanSource_NoDiagnostic()
    {
        var localization = Localization("en", "fr");
        var pages = ImmutableList.Create(
            Page("/page/", "/repo/page.md", "en"),
            Page("/fr/page/", "/repo/fr/page.md", "fr"));

        var git = new CountingGitHistoryReader(new(StringComparer.Ordinal)
        {
            ["/repo/page.md"] = new CommitInfo("src1234", new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            ["/repo/fr/page.md"] = new CommitInfo("tr12345", new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero)),
        });

        var auditor = new TranslationAuditor(new TranslationAuditOptions(), git);

        var diagnostics = await auditor.AuditAsync(
            new BuildAuditContext(pages, localization),
            TestContext.Current.CancellationToken);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task AuditAsync_NoTranslationFile_ReportsMissing()
    {
        var localization = Localization("en", "fr");
        var pages = ImmutableList.Create(Page("/page/", "/repo/page.md", "en"));

        var git = new CountingGitHistoryReader(new(StringComparer.Ordinal)
        {
            ["/repo/page.md"] = new CommitInfo("src1234", DateTimeOffset.UnixEpoch),
        });

        var auditor = new TranslationAuditor(new TranslationAuditOptions(), git);

        var diagnostics = await auditor.AuditAsync(
            new BuildAuditContext(pages, localization),
            TestContext.Current.CancellationToken);

        diagnostics.Count.ShouldBe(1);
        diagnostics[0].Message.ShouldContain("Missing");
        diagnostics[0].SourceFile.ShouldBe("translation.audit/missing/fr");
    }

    private static LocalizationOptions Localization(params string[] locales)
    {
        var options = new LocalizationOptions { DefaultLocale = locales[0] };
        foreach (var code in locales)
        {
            options.AddLocale(code, new LocaleInfo(code.ToUpperInvariant()));
        }

        return options;
    }

    private static ContentTocItem Page(string canonicalPath, string sourceFile, string locale) =>
        new(
            Title: "Page",
            Route: new ContentRoute
            {
                CanonicalPath = new UrlPath(canonicalPath),
                OutputFile = new FilePath(canonicalPath.Trim('/') + "/index.html"),
                SourceFile = new FilePath(sourceFile),
                Locale = locale,
            },
            Order: 0,
            HierarchyParts: [],
            SectionLabel: null,
            Locale: locale);

    private sealed class CountingGitHistoryReader(Dictionary<string, CommitInfo?> commits) : IGitHistoryReader
    {
        public Dictionary<string, int> Calls { get; } = new(StringComparer.Ordinal);

        public CommitInfo? GetLatestCommit(string absoluteFilePath)
        {
            Calls[absoluteFilePath] = Calls.GetValueOrDefault(absoluteFilePath) + 1;
            return commits.GetValueOrDefault(absoluteFilePath);
        }
    }
}
