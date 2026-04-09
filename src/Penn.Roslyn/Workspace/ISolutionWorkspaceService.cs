namespace Pennington.Roslyn.Workspace;

using Microsoft.CodeAnalysis;

/// <summary>
/// Manages a Roslyn Solution loaded from an MSBuild workspace.
/// Supports incremental document updates and full solution reloads.
/// </summary>
public interface ISolutionWorkspaceService : IDisposable
{
    /// <summary>
    /// Loads a solution from the specified path. If already loaded,
    /// applies any pending document updates and returns the cached solution.
    /// </summary>
    Task<Solution> LoadSolutionAsync(string solutionPath);

    /// <summary>
    /// Gets projects from the loaded solution with optional filtering.
    /// </summary>
    Task<IEnumerable<Project>> GetProjectsAsync(Func<Project, bool>? filter = null);

    /// <summary>
    /// Gets the compilation for a specific project, with caching.
    /// </summary>
    Task<Compilation?> GetCompilationAsync(Project project);

    /// <summary>
    /// Invalidates the cached solution, forcing a full reload on next access.
    /// Clears all caches and pending updates.
    /// </summary>
    void InvalidateSolution();

    /// <summary>
    /// Queues a document update for deferred processing.
    /// The update is applied on the next <see cref="LoadSolutionAsync"/> call.
    /// </summary>
    void UpdateDocument(string filePath);
}
