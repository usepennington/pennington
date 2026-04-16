---
title: "Hot reload and file watching"
description: "Why Pennington ships its own file watcher and WebSocket reload channel, and how the dev-only script is kept out of published builds."
sectionLabel: "Developer Experience"
order: 306010
tags: [live-reload, file-watching, dev-loop, websockets]
uid: explanation.dev-experience.hot-reload
---

Content files — `.md` sources, front matter, images, assets tracked under a source's `ContentPath` — are not part of the .NET compilation. Restarting the host for every markdown typo would tear down Kestrel and throw away the expensive in-memory caches that make the second request fast. Pennington's answer is narrower: an in-process `FileWatcher` that listens to content directories (and, when Roslyn is wired, to `.cs`/`.csproj`/`.sln` files), a `FileWatchDependencyFactory<T>` that structurally invalidates derived caches on change, and a debounced WebSocket channel through which the browser is told to reload.

## How it works

The mechanism is a single chain: files change, cached services drop their state, a debounce window elapses, and the browser reloads.

### `FileWatcher` wraps the FS watcher

`FileWatcher` is a thin layer over `System.IO.Abstractions.IFileSystemWatcher`. Callers — primarily `MarkdownContentService<T>` when it registers its source directory — subscribe by calling `AddPathWatch(path, pattern, onFileChanged)`. Each subscription is keyed by a `path|pattern` string so the same directory cannot be registered twice under identical conditions. Internally the watcher hooks `Changed`, `Created`, `Deleted`, and `Renamed` and fires a single `NotifySubscribers()` pass after each event. Any service or custom `IContentService` that registers via `AddPathWatch` automatically participates in both cache invalidation and browser refresh.

The reason for the abstraction sits at two levels. In tests, a `MockFileSystem` drives the same interface without touching the real filesystem. In production, the seam also contains the behaviour differences between filesystem event delivery on WSL and on Windows — quirks that would otherwise leak through every consumer.

### `FileWatchDependencyFactory` reconstructs services on change

Several services build expensive lookup tables from disk on startup: link resolvers, cross-reference uid maps, search indexes, sitemaps, and blog content resolvers. Rather than giving each service its own cache-bust logic, Pennington registers them through `AddFileWatched<T>`. That extension wires a singleton `FileWatchDependencyFactory<T>` that subscribes to `IFileWatcher.SubscribeToChanges`, alongside a transient front that resolves to whatever instance the factory currently holds.

When a change notification arrives, the factory drops its cached instance — disposing it if it implements `IDisposable` — and lets it be rebuilt on the next resolution via `ActivatorUtilities.CreateInstance<T>`. The mental model here is structural invalidation rather than explicit cache-busting: no service needs to know when to flush itself, because the factory discards and reconstructs the whole instance when the underlying content moves.

### `LiveReloadServer` debounces and broadcasts over WebSocket

`LiveReloadServer` is a singleton that subscribes to the same `IFileWatcher.SubscribeToChanges` hook in its constructor and maintains a `ConcurrentDictionary<string, WebSocket>` of connected browser sessions. Rather than forwarding every filesystem event immediately, it resets a 300ms debounce timer on each notification — only after 300ms of quiet does it walk the dictionary, send the string `"reload"` to every open socket, and prune closed ones. This coalesces rapid saves (editor auto-save, multi-file renames) into a single browser reload. The endpoint is mapped at `/__pennington/reload` as a WebSocket path; when a browser upgrades the connection, `HandleAsync` parks it until the client disconnects.

### Script injection and reconnection

`LiveReloadScriptProcessor` is an `IResponseProcessor` at `Order = 20`, positioned between the HTML rewriting pipeline at `Order = 10` and the diagnostic overlay at `Order = 30`. When active it finds the last `</body>` tag and inserts an inline script that opens a WebSocket to `/__pennington/reload`. The script includes three refinements over a naive `location.reload()` approach: a `beforeunload` guard that suppresses reconnect attempts during normal page navigation, a 150ms delay before reload so the response pipeline has time to settle, and immediate reload on reconnect so that `dotnet watch` server restarts refresh the browser without waiting for a file-change message.

### Build-mode gating

Both `LiveReloadScriptProcessor` and `UsePenningtonLiveReload` check whether the first command-line argument is `build`. When it is — i.e. during `dotnet run -- build` — the processor's `ShouldProcess` returns `false` and the middleware skips endpoint registration entirely. This means the `OutputGenerationService` crawler sees clean HTML with no script and no WebSocket endpoint: no publish-time stripping step, no build configuration to set, and no dev-only flag to forget.

## Trade-offs

- **Cost — every content edit triggers a full page reload, not a patch.** The broadcast message is a single string (`"reload"`); the browser responds with `location.reload()`. There is no HMR-style diff, no scroll preservation, no island re-render. For a docs engine this is the right shape — the alternative (partial DOM swaps) would need diffing infrastructure that neither Markdig nor the response processors expose today.
- **Alternative considered — restart the host on every file change.** `dotnet watch` could in theory be configured to restart on `.md` changes. It was rejected because restart cost grows with the app (Kestrel bind, DI graph, Razor compile), and the cached derived state (`MarkdownLinkResolver`, `XrefResolver`, search index) would be thrown away on every typo. Instance invalidation via `FileWatchDependencyFactory<T>` is cheap and keeps the host warm.
- **Consequence — file watchers are per-path, not recursive-from-root.** Each source directory is registered explicitly (primarily by `MarkdownContentService<T>` at construction). A change outside any watched path — say, editing a file in `bin/` — does not fire. This is deliberate: watching from the solution root would generate noise from IDE writes, NuGet caches, and build output. The cost is that brand-new content roots only become live after the host restarts.

## Further reading

- Reference: [DI and middleware extension methods](xref:reference.host.extensions)
- Reference: [Response processing interfaces](xref:reference.extension-points.response-processing)
- Explanation: [The response-processing pipeline](xref:explanation.core.response-processing)
- Explanation: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build)
