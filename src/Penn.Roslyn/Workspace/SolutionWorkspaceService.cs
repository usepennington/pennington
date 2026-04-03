namespace Penn.Roslyn.Workspace;

using System.Collections.Concurrent;
using System.Text;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using Penn.Infrastructure;

/// <summary>
/// Implementation of <see cref="ISolutionWorkspaceService"/> that manages an MSBuild workspace,
/// supports deferred document updates, and integrates with file watching.
/// </summary>
internal sealed class SolutionWorkspaceService : ISolutionWorkspaceService
{
    private readonly ILogger<SolutionWorkspaceService> _logger;
    private readonly RoslynOptions _options;
    private readonly Lock _lock = new();
    private readonly string _tempBuildPath;

    private MSBuildWorkspace? _workspace;
    private Solution? _solution;
    private readonly ConcurrentDictionary<ProjectId, Compilation> _compilationCache = new();
    private readonly ConcurrentQueue<string> _pendingUpdates = new();
    private bool _isDisposed;

    static SolutionWorkspaceService()
    {
        if (!MSBuildLocator.IsRegistered)
        {
            var instance = MSBuildLocator.QueryVisualStudioInstances()
                .OrderByDescending(x => x.Version)
                .FirstOrDefault();

            if (instance is not null)
            {
                MSBuildLocator.RegisterInstance(instance);
            }
            else
            {
                MSBuildLocator.RegisterDefaults();
            }
        }
    }

    public SolutionWorkspaceService(
        RoslynOptions options,
        IFileWatcher fileWatcher,
        ILogger<SolutionWorkspaceService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(fileWatcher);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.SolutionPath))
        {
            throw new ArgumentException("Solution path must be specified in options", nameof(options));
        }

        // Create temp folder for build artifacts to avoid polluting real output
        _tempBuildPath = Path.Combine(
            Path.GetTempPath(),
            $"Penn_Build_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempBuildPath);
        _logger.LogDebug("Created temp build folder: {Path}", _tempBuildPath);

        RegisterFileWatching(fileWatcher);
    }

    public async Task<Solution> LoadSolutionAsync(string solutionPath)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        lock (_lock)
        {
            if (_solution is not null && _workspace is not null)
            {
                ApplyPendingUpdates();
                return _solution;
            }
        }

        _logger.LogDebug("Loading solution from {SolutionPath}", solutionPath);

        // Configure MSBuild properties to redirect build artifacts to temp folder
        var properties = new Dictionary<string, string>
        {
            ["BaseIntermediateOutputPath"] = Path.Combine(_tempBuildPath, "obj") + Path.DirectorySeparatorChar,
            ["IntermediateOutputPath"] = Path.Combine(_tempBuildPath, "obj", "$(Configuration)") + Path.DirectorySeparatorChar,
            ["OutputPath"] = Path.Combine(_tempBuildPath, "bin", "$(Configuration)") + Path.DirectorySeparatorChar,
        };

        var workspace = MSBuildWorkspace.Create(properties);
        workspace.RegisterWorkspaceFailedHandler(args =>
        {
            _logger.LogWarning("Workspace failed: {Diagnostic}", args.Diagnostic);
        });

        try
        {
            var solution = await workspace.OpenSolutionAsync(solutionPath);

            lock (_lock)
            {
                _workspace?.Dispose();
                _workspace = workspace;
                _solution = solution;
                _compilationCache.Clear();
            }

            _logger.LogDebug("Successfully loaded solution with {ProjectCount} projects",
                solution.Projects.Count());
            return solution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load solution from {SolutionPath}", solutionPath);
            workspace.Dispose();
            throw;
        }
    }

    public async Task<IEnumerable<Project>> GetProjectsAsync(Func<Project, bool>? filter = null)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        var solution = await LoadSolutionAsync(_options.SolutionPath!);
        var projects = solution.Projects;

        if (filter is not null)
        {
            projects = projects.Where(filter);
        }

        if (_options.ProjectFilter is not null)
        {
            projects = ApplyProjectFilter(projects, _options.ProjectFilter);
        }

        return projects.ToList();
    }

    public async Task<Compilation?> GetCompilationAsync(Project project)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (_compilationCache.TryGetValue(project.Id, out var cachedCompilation))
        {
            _logger.LogTrace("Compilation cache HIT for project {ProjectName} ({ProjectId})",
                project.Name, project.Id);
            return cachedCompilation;
        }

        _logger.LogTrace("Compilation cache MISS for project {ProjectName} ({ProjectId}) - compiling",
            project.Name, project.Id);

        try
        {
            _logger.LogDebug("Compiling project {ProjectName}", project.Name);
            var compilation = await project.GetCompilationAsync();

            if (compilation is not null)
            {
                _compilationCache.TryAdd(project.Id, compilation);
                _logger.LogTrace("Compilation cached for project {ProjectName} ({ProjectId})",
                    project.Name, project.Id);
            }

            return compilation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compile project {ProjectName}", project.Name);
            return null;
        }
    }

    public void InvalidateSolution()
    {
        _logger.LogWarning("InvalidateSolution called - clearing {QueuedCount} pending updates",
            _pendingUpdates.Count);

        lock (_lock)
        {
            var cachedProjectCount = _compilationCache.Count;
            var queuedCount = _pendingUpdates.Count;

            _logger.LogTrace("Invalidating solution cache (clearing {Count} cached compilations, {QueuedCount} queued updates)",
                cachedProjectCount, queuedCount);

            _solution = null;
            _workspace?.Dispose();
            _workspace = null;
            _compilationCache.Clear();
            _pendingUpdates.Clear();

            _logger.LogTrace("Solution cache invalidated, workspace disposed");
        }
    }

    public void UpdateDocument(string filePath)
    {
        _logger.LogTrace("UpdateDocument called for {FilePath}", filePath);

        _pendingUpdates.Enqueue(filePath);

        _logger.LogTrace("Enqueued document update for {FilePath} (queue depth: {Count})",
            filePath, _pendingUpdates.Count);
    }

    private void ApplyPendingUpdates()
    {
        // Must be called within _lock

        if (_solution is null)
        {
            _logger.LogTrace("No solution loaded, clearing pending updates queue");
            _pendingUpdates.Clear();
            return;
        }

        if (_pendingUpdates.IsEmpty)
        {
            return;
        }

        _logger.LogTrace("Applying {Count} pending document updates", _pendingUpdates.Count);

        // Dequeue all pending updates and deduplicate by file path
        var updatesByPath = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        while (_pendingUpdates.TryDequeue(out var filePath))
        {
            updatesByPath[filePath] = true;
        }

        _logger.LogTrace("Deduplicated to {Count} unique file(s)", updatesByPath.Count);

        // Apply updates to solution
        var updatedSolution = _solution;
        var invalidatedProjects = new HashSet<ProjectId>();
        var successCount = 0;

        foreach (var filePath in updatesByPath.Keys)
        {
            try
            {
                var documentIds = _solution.GetDocumentIdsWithFilePath(filePath);

                if (documentIds.IsEmpty)
                {
                    _logger.LogTrace("File {FilePath} not found in solution during deferred update", filePath);
                    continue;
                }

                // Read file content with sharing enabled (file may be locked by editor)
                using var fileStream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);
                using var reader = new StreamReader(fileStream, Encoding.UTF8);
                var fileContent = reader.ReadToEnd();
                var newText = SourceText.From(fileContent, Encoding.UTF8);

                foreach (var docId in documentIds)
                {
                    var document = _solution.GetDocument(docId);
                    _logger.LogTrace("Applying deferred update to document in project {ProjectName} for {FilePath}",
                        document?.Project.Name ?? "Unknown", filePath);

                    updatedSolution = updatedSolution.WithDocumentText(docId, newText);
                    invalidatedProjects.Add(docId.ProjectId);
                }

                successCount++;
            }
            catch (FileNotFoundException)
            {
                _logger.LogTrace("File {FilePath} not found during update (may have been deleted)", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to apply deferred update for {FilePath}, invalidating solution", filePath);

                // On unexpected failure, invalidate entire solution for safety
                _solution = null;
                _workspace?.Dispose();
                _workspace = null;
                _compilationCache.Clear();
                return;
            }
        }

        // Commit the batched update
        if (successCount > 0)
        {
            _solution = updatedSolution;

            foreach (var projectId in invalidatedProjects)
            {
                _compilationCache.TryRemove(projectId, out _);
            }

            _logger.LogTrace("Successfully applied {Count} deferred document update(s), invalidated {ProjectCount} project compilation(s)",
                successCount, invalidatedProjects.Count);
        }
        else
        {
            _logger.LogTrace("No updates were successfully applied");
        }
    }

    private void RegisterFileWatching(IFileWatcher fileWatcher)
    {
        var solutionDir = Path.GetDirectoryName(Path.GetFullPath(_options.SolutionPath!));
        if (string.IsNullOrEmpty(solutionDir))
        {
            return;
        }

        // Watch for project file changes - always invalidate
        fileWatcher.AddPathWatch(solutionDir, "*.csproj", (path, changeType) =>
        {
            _logger.LogTrace("Project file watcher triggered: {ChangeType} for {Path}", changeType, path);
            _logger.LogDebug("Project file changed: {Path}", path);
            InvalidateSolution();
        });

        // Watch for solution file changes - invalidate only if it matches our configured path
        fileWatcher.AddPathWatch(solutionDir, "*.sln", (path, changeType) =>
        {
            _logger.LogTrace("Solution file watcher triggered: {ChangeType} for {Path}", changeType, path);

            if (path.Equals(Path.GetFullPath(_options.SolutionPath!), StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Solution file changed: {Path}", path);
                InvalidateSolution();
            }
            else
            {
                _logger.LogTrace("Solution file changed but not the configured solution, ignoring: {Path}", path);
            }
        });

        fileWatcher.AddPathWatch(solutionDir, "*.slnx", (path, changeType) =>
        {
            _logger.LogTrace("Solution file watcher triggered: {ChangeType} for {Path}", changeType, path);

            if (path.Equals(Path.GetFullPath(_options.SolutionPath!), StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Solution file changed: {Path}", path);
                InvalidateSolution();
            }
            else
            {
                _logger.LogTrace("Solution file changed but not the configured solution, ignoring: {Path}", path);
            }
        });

        // Watch for C# source file changes - smart handling based on change type
        fileWatcher.AddPathWatch(solutionDir, "*.cs", (path, changeType) =>
        {
            _logger.LogTrace("C# source file watcher triggered: {ChangeType} for {Path}", changeType, path);

            switch (changeType)
            {
                case WatcherChangeTypes.Changed:
                    _logger.LogTrace("File content changed - calling UpdateDocument");
                    UpdateDocument(path);
                    break;

                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Deleted:
                case WatcherChangeTypes.Renamed:
                    _logger.LogTrace("Structural change detected - calling InvalidateSolution");
                    InvalidateSolution();
                    break;
            }
        });
    }

    private static IEnumerable<Project> ApplyProjectFilter(IEnumerable<Project> projects, ProjectFilter filter)
    {
        if (filter.IncludedProjects is { Count: > 0 })
        {
            projects = projects.Where(p => filter.IncludedProjects.Contains(p.Name));
        }

        if (filter.ExcludedProjects is { Count: > 0 })
        {
            projects = projects.Where(p => !filter.ExcludedProjects.Contains(p.Name));
        }

        return projects;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        lock (_lock)
        {
            _workspace?.Dispose();
            _compilationCache.Clear();
            _isDisposed = true;
        }

        // Clean up temp folder
        if (Directory.Exists(_tempBuildPath))
        {
            try
            {
                Directory.Delete(_tempBuildPath, recursive: true);
                _logger.LogDebug("Cleaned up temp build folder: {Path}", _tempBuildPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean up temp build folder: {Path}", _tempBuildPath);
            }
        }
    }
}
