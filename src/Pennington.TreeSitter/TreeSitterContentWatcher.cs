namespace Pennington.TreeSitter;

using Pennington.Infrastructure;

/// <summary>
/// Registers <see cref="TreeSitterOptions.ContentRoot"/> with the file-watch subsystem so editing a source
/// file referenced by a <c>:symbol</c> fence triggers a live-reload. Watches are scoped to the configured
/// source-file globs (<see cref="TreeSitterOptions.WatchFilePatterns"/>), so build output (<c>bin/</c>,
/// <c>obj/</c>) and directory churn never reach the (path-blind) live-reload server. Fragments are read per
/// request, so there is no cache of our own to refresh.
/// </summary>
internal sealed class TreeSitterContentWatcher : IFileWatchAware
{
    private readonly IReadOnlyList<FileWatchScope> _scopes;

    /// <summary>Builds one recursive watch scope per configured file glob under the content root.</summary>
    public TreeSitterContentWatcher(TreeSitterOptions options)
    {
        var root = Path.GetFullPath(options.ContentRoot!);
        _scopes = Directory.Exists(root)
            ? [.. options.WatchFilePatterns.Select(pattern => new FileWatchScope(root, pattern, IncludeSubdirectories: true))]
            : [];
    }

    /// <inheritdoc />
    public IReadOnlyList<FileWatchScope> WatchScopes => _scopes;

    /// <inheritdoc />
    public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Ignore;
}
