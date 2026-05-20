namespace Pennington.TranslationAudit;

using LibGit2Sharp;
using Microsoft.Extensions.Logging;

/// <summary>
/// <see cref="IGitHistoryReader"/> backed by LibGit2Sharp. Holds the repository open for the
/// process lifetime and serializes access through a single instance because <see cref="Repository"/>
/// is not thread-safe.
/// </summary>
public sealed class LibGit2GitHistoryReader : IGitHistoryReader, IDisposable
{
    private readonly Repository? _repo;
    private readonly string? _workingDirectory;
    private readonly ILogger<LibGit2GitHistoryReader> _logger;
    private readonly Lock _lock = new();

    /// <summary>Opens the repository at <paramref name="repositoryRoot"/>, falling back to a no-op reader when no repo is found.</summary>
    public LibGit2GitHistoryReader(string? repositoryRoot, ILogger<LibGit2GitHistoryReader> logger)
    {
        _logger = logger;
        var discovered = DiscoverRoot(repositoryRoot);
        if (discovered is null)
        {
            _logger.LogWarning(
                "TranslationAudit: no git repository found at or above '{Path}'. Translation status will treat every file as untracked.",
                repositoryRoot ?? Directory.GetCurrentDirectory());
            return;
        }

        try
        {
            _repo = new Repository(discovered);
            _workingDirectory = _repo.Info.WorkingDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "TranslationAudit: failed to open git repository at '{Path}'.", discovered);
            _repo = null;
        }
    }

    /// <inheritdoc/>
    public CommitInfo? GetLatestCommit(string absoluteFilePath)
    {
        if (_repo is null || _workingDirectory is null)
        {
            return null;
        }

        var relative = Path.GetRelativePath(_workingDirectory, absoluteFilePath).Replace('\\', '/');
        if (relative.StartsWith("..", StringComparison.Ordinal))
        {
            return null;
        }

        lock (_lock)
        {
            var filter = new CommitFilter { SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time };
            var commits = _repo.Commits.QueryBy(relative, filter);
            var commit = commits.FirstOrDefault()?.Commit;
            return commit is null
                ? null
                : new CommitInfo(commit.Sha[..Math.Min(7, commit.Sha.Length)], commit.Author.When);
        }
    }

    /// <inheritdoc/>
    public void Dispose() => _repo?.Dispose();

    private static string? DiscoverRoot(string? hint)
    {
        var start = string.IsNullOrEmpty(hint) ? Directory.GetCurrentDirectory() : hint;
        return Repository.Discover(start);
    }
}