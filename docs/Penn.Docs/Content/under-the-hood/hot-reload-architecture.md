---
title: "Hot Reload Architecture"
description: "How Penn's file watching and cached-until-invalidated services provide instant content updates during development"
uid: "penn.under-the-hood.hot-reload-architecture"
order: 3001
---

When you are writing docs, the feedback loop matters. Edit a markdown file, save, and see the result. Penn's hot reload system makes this work through three collaborating pieces: `IFileWatcher` for monitoring the file system, `FileWatchDependencyFactory<T>` for managing cached service instances that auto-invalidate on changes, and `SolutionWorkspaceService` for keeping the Roslyn workspace in sync without reloading the entire solution every time someone edits a `.cs` file.

It is not glamorous infrastructure. But when it works well, you do not notice it, and that is the point.

## Architecture Overview

```
File System
    |
    | (FileSystemWatcher events)
    v
FileWatcher (IFileWatcher)
    |
    +-- SubscribeToChanges() --> FileWatchDependencyFactory<T>.InvalidateInstance()
    |                                |
    |                                +-- Disposes cached T instance
    |                                +-- Next GetInstance() creates fresh T
    |
    +-- AddPathWatch(*.cs) --> SolutionWorkspaceService
                                    |
                                    +-- Content change: UpdateDocument() (deferred)
                                    +-- Structural change: InvalidateSolution() (full reload)
```

The left branch handles general-purpose services -- content pipelines, navigation trees, search indexes. Any service registered through `FileWatchDependencyFactory<T>` gets its cached instance thrown away when files change, then lazily rebuilt on the next request.

The right branch handles the Roslyn workspace specifically. Because reloading a full .NET solution is expensive, `SolutionWorkspaceService` distinguishes between content changes (cheap incremental update) and structural changes (full reload required). Both branches originate from the same `FileWatcher`, but they respond to changes differently.

## IFileWatcher and FileWatcher

`IFileWatcher` is the low-level file monitoring interface:

```csharp:xmldocid
T:Penn.Infrastructure.IFileWatcher
```

Two methods, two jobs:

- **`AddPathWatch`** registers a directory watch for files matching a glob pattern. You provide a callback that receives the changed file path and the change type (`Changed`, `Created`, `Deleted`, `Renamed`). Optionally disable subdirectory recursion with `includeSubdirectories: false`.
- **`SubscribeToChanges`** registers a callback that fires whenever *any* watched file changes, regardless of path or pattern. This is the integration point for `FileWatchDependencyFactory`.

The implementation, `FileWatcher`, wraps .NET's `FileSystemWatcher`. It handles the usual `FileSystemWatcher` bookkeeping: deduplication of watch registrations by path-plus-pattern key, subdirectory support, and the four event types. The `NotifyFilter` is configured for `LastWrite | FileName | DirectoryName | CreationTime`, which covers edits, renames, additions, and deletions.

When any watched file changes, `FileWatcher` calls both the path-specific callback *and* all general subscribers. The path-specific callback gets the file path and change type. The general subscribers get a bare "something changed" signal. This two-tier notification is what lets `FileWatchDependencyFactory` remain generic -- it does not need to know which file changed, only that *something* did.

`FileWatcher` is registered as a singleton. It implements `IDisposable` and tears down all `FileSystemWatcher` instances on disposal, disabling events before releasing handles.

## FileWatchDependencyFactory: Cached-Until-Invalidated Services

This is the clever bit. `FileWatchDependencyFactory<T>` manages a single cached instance of a service `T` that gets thrown away when files change:

```csharp:path
src/Penn/Infrastructure/FileWatchDependencyFactory.cs
```

### Lifecycle

1. **First call to `GetInstance()`**: Creates `T` using `ActivatorUtilities.CreateInstance<T>()` from the DI container. This is potentially expensive -- think "scan every markdown file and parse all front matter" or "load an MSBuild workspace."
2. **Subsequent calls**: Returns the cached instance. Fast.
3. **File change detected**: `InvalidateInstance()` fires. Disposes the old instance (if it implements `IDisposable`), sets the field to `null`.
4. **Next call to `GetInstance()`**: Creates a fresh instance. Back to step 1.

The pattern is lazy evaluation with file-system-triggered cache invalidation. The key insight is that invalidation is *immediate* but re-creation is *deferred*. When you save a file, the old cache is thrown away instantly. But we do not eagerly rebuild everything -- we wait until someone actually requests the data. This avoids wasted work when multiple files change in rapid succession (like a git checkout that touches dozens of files).

### Thread Safety

All access to the cached instance goes through a `Lock`. This means:

- Only one thread creates the instance (no duplicate initialization).
- `InvalidateInstance()` and `GetInstance()` cannot race each other.
- Disposal of the old instance happens under the lock, preventing use-after-dispose.

### The AddFileWatched Extension Method

Wiring up the factory by hand would be tedious. The `AddFileWatched` extension methods on `IServiceCollection` handle the two-registration pattern:

```csharp
// Service interface + implementation
services.AddFileWatched<IContentPipeline, ContentPipeline>();

// Concrete type only
services.AddFileWatched<NavigationBuilder>();
```

Each overload does two things:

1. **Registers the factory as a singleton.** `FileWatchDependencyFactory<TImplementation>` is constructed once and subscribes to `IFileWatcher` changes via its constructor.
2. **Registers the service as transient.** Each resolution calls `factory.GetInstance()`, which returns the cached instance or creates a new one.

The transient registration is important. It means every request gets whatever instance the factory currently holds. If a file change invalidated the cache between two requests, the second request gets a fresh instance. If no files changed, both requests share the same instance. The transient lifetime does not mean "create a new instance every time" -- it means "ask the factory every time," and the factory decides.

This pattern is used throughout Penn's DI wiring for services like `MarkdownContentService`, `SearchIndexService`, `SitemapService`, and `BlogContentResolver`. Any service that caches data derived from files on disk is a candidate.

## SolutionWorkspaceService: Smart Roslyn Updates

The Roslyn workspace is the most expensive thing Penn manages. Loading a full .NET solution -- resolving project references, parsing every `.cs` file, building compilation objects -- can take several seconds. You *really* do not want to do that every time someone saves a file.

`SolutionWorkspaceService` uses a two-tier invalidation strategy.

### Content Changes (Cheap)

When a `.cs` file is *modified* (content changed, not added or deleted), the service uses **deferred document updates**:

```csharp
case WatcherChangeTypes.Changed:
    UpdateDocument(path);
    break;
```

`UpdateDocument()` enqueues the file path in a `ConcurrentQueue`. The actual update is applied lazily -- the next time someone calls `LoadSolutionAsync`, the `ApplyPendingUpdates()` method reads the new file content and calls `Solution.WithDocumentText()` to create an updated solution snapshot. Only the affected project's compilation cache entry is removed.

This is dramatically cheaper than reloading the entire solution. The Roslyn `Solution` type is immutable, so `WithDocumentText()` returns a new solution that shares most of its state with the old one. Only the changed document and its containing project need to be re-analyzed. Pending updates are deduplicated by file path, so saving the same file three times in quick succession results in a single read-and-update at request time.

### Structural Changes (Expensive but Necessary)

When a `.cs` file is *added*, *deleted*, or *renamed*, or when a `.csproj` or `.sln`/`.slnx` file changes, the service does a full invalidation:

```csharp
case WatcherChangeTypes.Created:
case WatcherChangeTypes.Deleted:
case WatcherChangeTypes.Renamed:
    InvalidateSolution();
    break;
```

`InvalidateSolution()` throws away the entire workspace, all cached compilations, and all pending updates. The next access reloads the solution from scratch. This is slow but correct -- structural changes (new files, removed projects, changed references) cannot be handled incrementally by Roslyn's immutable `Solution` model.

### File Watch Registration Table

`SolutionWorkspaceService` registers watchers for three file patterns, all rooted at the solution directory:

| Pattern | Change Type | Action |
|---|---|---|
| `*.cs` | Changed | `UpdateDocument()` (deferred) |
| `*.cs` | Created/Deleted/Renamed | `InvalidateSolution()` |
| `*.csproj` | Any | `InvalidateSolution()` |
| `*.sln` / `*.slnx` | Any (matching configured path) | `InvalidateSolution()` |

The solution file watcher is selective -- it only invalidates when the changed `.sln` or `.slnx` matches the configured solution path. Changing an unrelated solution file in a subdirectory does not trigger a reload.

### Compilation Caching

On top of the solution-level caching, `SolutionWorkspaceService` maintains a per-project compilation cache (`ConcurrentDictionary<ProjectId, Compilation>`). Getting a `Compilation` from Roslyn is expensive -- it involves full semantic analysis. The cache means that requesting the compilation for a project twice only compiles it once.

When a deferred document update invalidates a project, only that project's compilation cache entry is removed. Other projects keep their cached compilations. This makes incremental re-analysis fast: edit a file in one project, and only that project recompiles on the next request.

## How It All Fits Together

Here is a typical development session:

1. **App starts.** `FileWatcher` is created as a singleton. `SolutionWorkspaceService` registers its file watches for `*.cs`, `*.csproj`, and `*.sln`/`*.slnx`. Nothing is loaded yet -- no content has been scanned, no solution has been opened.

2. **First page request.** The content pipeline resolves through `FileWatchDependencyFactory`, which calls `GetInstance()` for the first time. The content service discovers markdown files, parses front matter, builds navigation. If the page includes a `csharp:xmldocid` code block, `SolutionWorkspaceService` loads the solution and compiles the relevant projects. All of this is cached.

3. **You edit `getting-started.md`.** `FileWatcher` detects the change. The path-specific callback fires (if one is registered for `*.md`). All general subscribers fire too, including `FileWatchDependencyFactory`, which invalidates the content cache. The Roslyn workspace is unaffected -- it only watches `.cs`, `.csproj`, and solution files.

4. **You refresh the page.** `GetInstance()` finds the cache empty, creates a fresh content pipeline instance. Only the content is reprocessed. Roslyn compilations are still cached from step 2.

5. **You edit `MyComponent.cs`.** `FileWatcher` detects the change. `SolutionWorkspaceService.UpdateDocument()` enqueues the file path. The content cache is *also* invalidated (because `FileWatchDependencyFactory` subscribes to all file changes via `SubscribeToChanges`).

6. **You refresh the page.** The content pipeline rebuilds. When it encounters a `csharp:xmldocid` code block, it calls into the Roslyn workspace. `LoadSolutionAsync` applies the pending document update, invalidates just that project's compilation, and re-analyzes. Other projects keep their cached compilations.

7. **You add a new `.cs` file.** `SolutionWorkspaceService.InvalidateSolution()` fires. The entire workspace is thrown away -- solution, compilations, pending updates. Next access reloads from scratch. Slow, but correct and rare during normal content authoring.

The overall effect: editing markdown files is near-instant. Editing existing C# files is fast (incremental Roslyn update). Adding or removing files triggers a full reload but this is uncommon during the typical write-save-preview cycle of documentation work.

## Why Not Debounce?

You might notice there is no debounce logic in `FileWatchDependencyFactory`. Many file-watching systems add a delay to coalesce rapid changes. Penn does not, for a simple reason: invalidation is cheap (set a field to `null` and optionally call `Dispose`), and re-creation only happens on the *next request*. If ten files change in quick succession, the cache gets invalidated ten times -- but the expensive rebuild only happens once, when the next page request comes in.

The debounce is implicit in the request cycle. Save five files, switch to your browser, refresh -- by the time the HTTP request arrives, all the invalidations have already happened and the single rebuild produces results that reflect every change.

The same logic applies to `SolutionWorkspaceService`. Multiple rapid file changes may call `InvalidateSolution()` repeatedly, but the solution only reloads once on the next `LoadSolutionAsync` call. Deferred document updates go further: they are deduplicated by file path before being applied, so even rapid saves to the same file produce a single Roslyn update.

This architecture pairs with `dotnet watch`, which provides its own browser refresh signal. The full development loop is: save a file, `dotnet watch` triggers a browser reload, the reload request finds invalidated caches, fresh instances are created with updated content, the page renders. For more on how this fits into the broader development and deployment picture, see <xref:penn.under-the-hood.dev-vs-deployment-architecture>. For a practical walkthrough of the edit-save-preview workflow, see <xref:penn.guides.edit-content-existing-site>.
