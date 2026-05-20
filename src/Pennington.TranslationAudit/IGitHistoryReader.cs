namespace Pennington.TranslationAudit;

/// <summary>Reads the most recent commit affecting a tracked file. Abstracted for testability.</summary>
public interface IGitHistoryReader
{
    /// <summary>Returns the latest commit touching <paramref name="absoluteFilePath"/>, or null when the file is untracked or the repo is missing.</summary>
    CommitInfo? GetLatestCommit(string absoluteFilePath);
}

/// <summary>Minimal commit metadata exposed by <see cref="IGitHistoryReader"/>.</summary>
/// <param name="Sha">Short commit hash (7 chars).</param>
/// <param name="When">Commit author timestamp.</param>
public sealed record CommitInfo(string Sha, DateTimeOffset When);