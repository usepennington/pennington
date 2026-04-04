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

## IFileWatcher and FileWatcher

`IFileWatcher` is the low-level file monitoring interface:

```csharp:xmldocid
T:Penn.Infrastructure.IFileWatcher
```

Two methods, no nonsense:

- **`AddPathWatch`**: Watch a directory for files matching a pattern. You provide a callback that receives the changed file path and change type.
- **`SubscribeToChanges`**: Register a callback that fires whenever *any* watched file changes. This is the integration point for `FileWatchDependencyFactory`.

The implementation, `FileWatcher`, wraps .NET's `FileSystemWatcher`. It handles the usual `FileSystemWatcher` bookkeeping: deduplication of watch registrations, subdirectory support, and multiple event types (Changed, Created, Deleted, Renamed).

When any watched file changes, `FileWatcher` notifies all path-specific callbacks *and* all general subscribers. The path-specific callbacks get the file path and change type; the general subscribers just get a "something changed" signal.

## FileWatchDependencyFactory: Cached-Until-Invalidated Services

This is the clever bit. `FileWatchDependencyFactory<T>` manages a single cached instance of a service `T` that gets thrown away when files change:

```csharp:path
src/Penn/Infrastructure/FileWatchDependencyFactory.cs
```

The lifecycle:

1. **First call to `GetInstance()`**: Creates `T` using `ActivatorUtilities.CreateInstance<T>()` from the DI container. This is potentially expensive -- think "scan every markdown file and parse all front matter" or "load an MSBuild workspace."
2. **Subsequent calls**: Returns the cached instance. Fast.
3. **File change detected**: `InvalidateInstance()` fires. Disposes the old instance (if it implements `IDisposable`), sets it to `null`.
4. **Next call to `GetInstance()`**: Creates a fresh instance. Back to step 1.

The pattern is "lazy evaluation with file-system-triggered cache invalidation." The key insight is that invalidation is *immediate* but re-creation is *deferred*. When you save a file, the old cache is thrown away instantly. But we do not eagerly rebuild everything -- we wait until someone actually requests the data. This avoids wasted work when multiple files change in rapid succession (like a git checkout that touches dozens of files).

### Thread Safety

All access to the cached instance goes through a `Lock`. This means:

- Only one thread creates the instance (no duplicate initialization).
- `InvalidateInstance()` and `GetInstance()` cannot race each other.
- Disposal of the old instance happens under the lock, preventing use-after-dispose.

### Usage Pattern

Register the factory in DI as a singleton. It subscribes to `IFileWatcher` changes automatically via constructor injection:

```csharp
services.AddSingleton<FileWatchDependencyFactory<ExpensiveService>>();
```

Then inject the factory wherever you need the service:

```csharp
public class MyComponent(FileWatchDependencyFactory<ExpensiveService> factory)
{
    public void DoWork()
    {
        var service = factory.GetInstance(); // cached or freshly created
        service.Process();
    }
}
```

## SolutionWorkspaceService: Smart Roslyn Updates

The Roslyn workspace is the most expensive thing Penn manages. Loading a full .NET solution -- resolving project references, parsing every `.cs` file, building compilation objects -- can take several seconds. You *really* do not want to do that every time someone saves a file.

`SolutionWorkspaceService` uses a two-tier invalidation strategy:

### Content Changes (Cheap)

When a `.cs` file is *modified* (content changed, not added or deleted), the service uses **deferred document updates**:

```csharp
case WatcherChangeTypes.Changed:
    UpdateDocument(path);
    break;
```

`UpdateDocument()` enqueues the file path in a `ConcurrentQueue`. The actual update is applied lazily -- the next time someone asks for the solution, `ApplyPendingUpdates()` reads the new file content and calls `Solution.WithDocumentText()` to create an updated solution snapshot. Only the affected project's compilation cache is invalidated.

This is dramatically cheaper than reloading the entire solution. The Roslyn `Solution` type is immutable, so `WithDocumentText()` returns a new solution that shares most of its state with the old one. Only the changed document and its containing project need to be re-analyzed.

### Structural Changes (Expensive but Necessary)

When a `.cs` file is *added*, *deleted*, or *renamed*, or when a `.csproj` or `.sln`/`.slnx` file changes, the service does a full invalidation:

```csharp
case WatcherChangeTypes.Created:
case WatcherChangeTypes.Deleted:
case WatcherChangeTypes.Renamed:
    InvalidateSolution();
    break;
```

`InvalidateSolution()` throws away the entire workspace, all cached compilations, and all pending updates. The next access reloads the solution from scratch. This is slow but correct -- structural changes (new files, removed projects, changed references) cannot be handled incrementally.

### The File Watching Registration

`SolutionWorkspaceService` registers watchers for three file patterns, all rooted at the solution directory:

| Pattern | Change Type | Action |
|---|---|---|
| `*.cs` | Changed | `UpdateDocument()` (deferred) |
| `*.cs` | Created/Deleted/Renamed | `InvalidateSolution()` |
| `*.csproj` | Any | `InvalidateSolution()` |
| `*.sln` / `*.slnx` | Any (matching configured path) | `InvalidateSolution()` |

The solution file watcher is selective -- it only invalidates when the changed `.sln` or `.slnx` matches the configured solution path. Changing an unrelated solution file in a subdirectory does not trigger a reload.

### Compilation Caching

On top of the solution-level caching, `SolutionWorkspaceService` maintains a per-project compilation cache (`ConcurrentDictionary<ProjectId, Compilation>`). Getting a `Compilation` from Roslyn is expensive -- it involves full semantic analysis. The cache means that requesting the compilation for `Penn.Core` twice only compiles it once.

When a deferred document update invalidates a project, only that project's compilation cache entry is removed. Other projects keep their cached compilations, which makes incremental re-analysis fast.

## How It All Fits Together

Here is a typical development session:

1. **App starts.** `FileWatcher` is created. `SolutionWorkspaceService` registers its file watches. Nothing is loaded yet.

2. **First page request.** `FileWatchDependencyFactory<ContentPipeline>` (or similar) calls `GetInstance()`. The content pipeline discovers markdown files, parses front matter, builds navigation. `SolutionWorkspaceService` loads the solution and compiles projects. All of this is cached.

3. **You edit `getting-started.md`.** `FileWatcher` detects the change. `FileWatchDependencyFactory` invalidates the content cache. The Roslyn workspace is unaffected (it only watches `.cs` files).

4. **You refresh the page.** `GetInstance()` finds the cache empty, creates a fresh content pipeline. Only the content is reprocessed. Roslyn compilations are still cached.

5. **You edit `MyComponent.cs`.** `FileWatcher` detects the change. `SolutionWorkspaceService.UpdateDocument()` enqueues the file. The content cache is *also* invalidated (because `FileWatchDependencyFactory` subscribes to all file changes). 

6. **You refresh the page.** Content pipeline rebuilds. When it gets to a `csharp:xmldocid` code block, Roslyn applies the pending document update, invalidates just that project's compilation, and re-analyzes. Other projects keep their cached compilations.

7. **You add a new `.cs` file.** `SolutionWorkspaceService.InvalidateSolution()` fires. The entire workspace is thrown away. Next access reloads from scratch. Slow, but correct.

The overall effect: editing markdown files is near-instant. Editing existing C# files is fast (incremental update). Adding or removing files is slow but rare during normal content writing.

## Why Not Debounce?

You might notice there is no debounce logic in `FileWatchDependencyFactory`. Many file-watching systems add a delay to coalesce rapid changes. Penn does not, for a simple reason: invalidation is cheap (just set a field to `null`), and re-creation only happens on the *next request*. If ten files change in quick succession, the cache gets invalidated ten times -- but the expensive rebuild only happens once, when the next page request comes in.

The debounce is implicit in the request cycle. Save five files, switch to your browser, refresh -- by the time the request arrives, all the invalidations have already happened.
