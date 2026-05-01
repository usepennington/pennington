namespace Pennington.TranslationAudit;

using Pennington.Diagnostics;

/// <summary>Configuration for <see cref="TranslationAuditor"/>.</summary>
public sealed class TranslationAuditOptions
{
    /// <summary>
    /// Absolute path to the git repository root. When null, the repo is auto-discovered
    /// by walking up from the current directory until a <c>.git</c> folder is found.
    /// </summary>
    public string? RepositoryPath { get; set; }

    /// <summary>
    /// Locale codes to include. When null, every non-default locale configured in
    /// <see cref="Pennington.Infrastructure.LocalizationOptions"/> is reported.
    /// </summary>
    public HashSet<string>? IncludedLocales { get; set; }

    /// <summary>Severity for "translation file does not exist" diagnostics. Default <see cref="DiagnosticSeverity.Warning"/>.</summary>
    public DiagnosticSeverity MissingSeverity { get; set; } = DiagnosticSeverity.Warning;

    /// <summary>Severity for "translation predates source's last commit" diagnostics. Default <see cref="DiagnosticSeverity.Warning"/>.</summary>
    public DiagnosticSeverity OutdatedSeverity { get; set; } = DiagnosticSeverity.Warning;

    /// <summary>When true (default), missing translations are reported. Set false to defer that signal to a different mechanism.</summary>
    public bool ReportMissing { get; set; } = true;
}
