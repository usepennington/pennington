namespace Pennington.ApiMetadata;

/// <summary>Source-link target for a documented type or member, typically used to render "view source" links.</summary>
/// <param name="Path">Relative path to the source file within the repository.</param>
/// <param name="StartLine">1-based line number where the declaration begins.</param>
/// <param name="EndLine">1-based line number where the declaration ends.</param>
/// <param name="RepoUrl">Base URL of the source repository (e.g. GitHub URL), or <see langword="null"/> when unknown.</param>
/// <param name="Branch">Branch or tag the path resolves against, or <see langword="null"/> when unknown.</param>
public sealed record ApiSourceLocation(
    string Path,
    int StartLine,
    int EndLine,
    string? RepoUrl,
    string? Branch);
