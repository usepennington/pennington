namespace Pennington.Roslyn.Workspace;

using System.Collections.Concurrent;
using System.Text;
using Infrastructure;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using Symbols;

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

    internal ISymbolExtractionService? SymbolExtractionService { get; set; }

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
            $"Pennington_Build_{Guid.NewGuid():N}");
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

        // Redirect intermediates to a temp folder so MSBuildWorkspace's evaluation
        // doesn't fight dotnet watch over real obj/ files. Leave OutputPath alone:
        // ResolveProjectReferences needs each referenced project's compiled assembly
        // at the natural bin path, and dotnet watch (or a prior dotnet build) puts it
        // there. Redirecting BaseOutputPath to a temp folder breaks that lookup with
        // "Found project reference without a matching metadata reference" warnings.
        // OutputPath isn't overridden, so the SDK appends $(TargetFramework) per
        // inner build and multi-target projects no longer collide.
        var properties = new Dictionary<string, string>
        {
            ["BaseIntermediateOutputPath"] = Path.Combine(_tempBuildPath, "obj") + Path.DirectorySeparatorChar,
        };

        var workspace = MSBuildWorkspace.Create(properties);
        workspace.RegisterWorkspaceFailedHandler(args =>
        {
            var diagnostic = args.Diagnostic;
            var message = diagnostic.Message;

            // [Warning] kind from MSBuildWorkspace is its own "informational" tier —
            // typically "found project reference without a matching metadata reference"
            // emitted while parallel multi-target evaluation is in flight. Roslyn falls
            // back to source compilation when a metadata reference is missing, so the
            // resulting symbols are still correct. Real config mistakes (a project
            // reference pointing at a missing csproj, etc.) would have already failed
            // `dotnet build` before we got here.
            if (diagnostic.Kind == WorkspaceDiagnosticKind.Warning)
            {
                _logger.LogDebug("Workspace warning (suppressed): {Diagnostic}", diagnostic);
                return;
            }

            // [Failure] kind: MSBuildWorkspace evaluates project references in parallel;
            // multi-targeted inner builds race on the same per-TFM cache files under
            // our redirected _tempBuildPath ("AssemblyReference.cache",
            // "GlobalUsings.g.cs", "AssemblyInfoInputs.cache", "AssemblyAttributes.cs").
            // Roslyn builds compilations from in-memory state — these on-disk caches
            // don't affect the symbols we extract — so demote the race to Debug rather
            // than surface a confusing temp-path warning.
            if (message.Contains(_tempBuildPath, StringComparison.OrdinalIgnoreCase) &&
                (message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
                 message.Contains("being used by another process", StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogTrace("Workspace cache race in temp build dir (suppressed): {Diagnostic}", diagnostic);
                return;
            }

            _logger.LogWarning("Workspace failed: {Diagnostic}", diagnostic);
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

        projects = SelectMostRecentTargetFramework(projects);

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

        SymbolExtractionService?.ClearCache();
    }

    public void UpdateDocument(string filePath)
    {
        _logger.LogTrace("UpdateDocument called for {FilePath}", filePath);

        _pendingUpdates.Enqueue(filePath);

        _logger.LogTrace("Enqueued document update for {FilePath} (queue depth: {Count})",
            filePath, _pendingUpdates.Count);

        // Symbol cache holds SymbolInfo with snapshot Document references. Those
        // snapshots are frozen to the pre-edit solution; clear so the next
        // extraction re-queries from the patched solution.
        SymbolExtractionService?.ClearCache();
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

            if (IsGeneratedOrBuildOutput(path))
            {
                _logger.LogTrace("Ignoring generated or build-output file: {Path}", path);
                return;
            }

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

    /// <summary>
    /// Returns true for `.cs` paths that should be ignored by the source watcher —
    /// MSBuild output directories (`obj/`, `bin/`) and typical generated files.
    /// Prevents rebuild bursts from thrashing the symbol cache.
    /// </summary>
    private static bool IsGeneratedOrBuildOutput(string path)
    {
        var normalized = path.Replace('\\', '/');

        foreach (var segment in normalized.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
                segment.Equals("bin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        var fileName = Path.GetFileName(normalized);
        return fileName.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".AssemblyAttributes.cs", StringComparison.OrdinalIgnoreCase);
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

    // MSBuildWorkspace yields one Project per TFM for multi-targeted csproj files
    // (same FilePath, Name suffixed as "Foo(net11.0)"). Picking all of them double-
    // extracts symbols and can drag in down-level TFMs like net45 whose APIs diverge
    // from the modern build. Collapse each group to the most recent TFM.
    private IEnumerable<Project> SelectMostRecentTargetFramework(IEnumerable<Project> projects)
    {
        var grouped = projects
            .GroupBy(p => p.FilePath ?? p.Id.ToString(), StringComparer.OrdinalIgnoreCase);

        foreach (var group in grouped)
        {
            var candidates = group.ToList();
            if (candidates.Count == 1)
            {
                yield return candidates[0];
                continue;
            }

            var ranked = candidates
                .Select(p => (Project: p, Tfm: ExtractTargetFramework(p)))
                .OrderByDescending(x => TargetFrameworkRank(x.Tfm))
                .ToList();

            var winner = ranked[0];
            var skipped = string.Join(", ", ranked.Skip(1).Select(x => x.Tfm ?? "?"));
            _logger.LogDebug(
                "Multi-target project {ProjectFile}: selected {SelectedTfm}, skipped {SkippedTfms}",
                group.Key,
                winner.Tfm ?? "?",
                skipped);

            yield return winner.Project;
        }
    }

    // Roslyn names a multi-target project's Project instance as "{Name}({tfm})".
    // Single-target projects have no suffix, in which case the TFM is unknown here.
    private static string? ExtractTargetFramework(Project project)
    {
        var name = project.Name;
        var open = name.LastIndexOf('(');
        if (open < 0 || !name.EndsWith(')'))
        {
            return null;
        }

        return name[(open + 1)..^1];
    }

    // Ranks TFMs so the newest modern .NET wins, and legacy .NET Framework
    // (net45/net48/etc.) can never beat a modern TFM even when it sorts later
    // alphabetically.
    private static long TargetFrameworkRank(string? tfm)
    {
        if (string.IsNullOrEmpty(tfm))
        {
            return 0;
        }

        var dash = tfm.IndexOf('-');
        if (dash >= 0)
        {
            tfm = tfm[..dash];
        }

        if (!tfm.StartsWith("net", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        var rest = tfm[3..];

        if (rest.StartsWith("standard", StringComparison.OrdinalIgnoreCase))
        {
            return 20_000 + ParseDottedVersion(rest["standard".Length..]);
        }

        if (rest.StartsWith("coreapp", StringComparison.OrdinalIgnoreCase))
        {
            return 30_000 + ParseDottedVersion(rest["coreapp".Length..]);
        }

        // "netX.Y" form (.NET 5+) has a dot; .NET Framework uses "net45"/"net472".
        if (rest.Contains('.'))
        {
            return 40_000 + ParseDottedVersion(rest);
        }

        if (int.TryParse(rest, out var frameworkVersion))
        {
            return 10_000 + frameworkVersion;
        }

        return 0;
    }

    private static int ParseDottedVersion(string value)
    {
        return Version.TryParse(value, out var version)
            ? version.Major * 1000 + version.Minor * 10 + Math.Max(version.Build, 0)
            : 0;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        lock (_lock)
        {
            // MSBuildWorkspace.Dispose() can throw on Windows when assembly handles
            // are still mapped. Swallow so the temp-folder cleanup below still runs.
            try
            {
                _workspace?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing MSBuild workspace");
            }
            _compilationCache.Clear();
            _isDisposed = true;
        }

        DeleteTempBuildFolderWithRetry();
    }

    private void DeleteTempBuildFolderWithRetry()
    {
        if (!Directory.Exists(_tempBuildPath))
        {
            return;
        }

        // On Windows, MSBuild can hold transient file locks during shutdown.
        // Brief retries cover that without making the happy path slower.
        int[] delaysMs = [100, 200, 400];
        for (var attempt = 0; attempt <= delaysMs.Length; attempt++)
        {
            try
            {
                Directory.Delete(_tempBuildPath, recursive: true);
                _logger.LogDebug("Cleaned up temp build folder: {Path}", _tempBuildPath);
                return;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                if (attempt == delaysMs.Length)
                {
                    _logger.LogWarning(ex, "Failed to clean up temp build folder: {Path}", _tempBuildPath);
                    return;
                }
                Thread.Sleep(delaysMs[attempt]);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean up temp build folder: {Path}", _tempBuildPath);
                return;
            }
        }
    }
}