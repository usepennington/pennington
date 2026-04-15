---
title: "Hot reload and file watching"
description: "Why Pennington layers its own content watcher and WebSocket reload on top of dotnet watch, and how the dev-only script is kept out of published builds."
sectionLabel: "Developer Experience"
order: 306010
tags: [live-reload, file-watching, dev-loop, websockets]
uid: explanation.dev-experience.hot-reload
---

`dotnet watch` already restarts the app when C# or Razor files change — why does Pennington ship its own file watcher and WebSocket reload channel?

## Context

`dotnet watch` is the upstream tool: it rebuilds and restarts the host when compiled code changes, and Pennington relies on it for exactly that half of the loop. The gap is that content files — `.md` sources, front matter, images, assets tracked under a source's `ContentPath` — are not part of the compilation, so `dotnet watch` ignores them entirely. The naive alternative, restarting the host whenever any file changes, would tear down Kestrel for a markdown typo and throw away all the expensive in-memory caches that make the second request fast. Pennington's answer is narrower: an in-process `FileWatcher` that listens to content directories, a `FileWatchDependencyFactory<T>` that structurally invalidates derived caches on change, and a WebSocket channel through which the browser is told to reload. The remaining constraint — that none of this should survive into a published build — shapes the last piece of the design.

## How it works

The mechanism is a single chain: content files change, cached services drop their state, and the browser reloads. Each subsection below examines one link in that chain.

### `FileWatcher` wraps the FS watcher

`FileWatcher` is a thin layer over `System.IO.Abstractions.IFileSystemWatcher`. Callers — primarily `MarkdownContentService<T>` when it registers its source directory — subscribe by calling `AddPathWatch(path, pattern, onFileChanged)`. Each subscription is keyed by a `path|pattern` string so the same directory cannot be registered twice under identical conditions. Internally the watcher hooks `Changed`, `Created`, `Deleted`, and `Renamed` and fires a single `NotifySubscribers()` pass after each event.

The reason for the abstraction sits at two levels. In tests, a `MockFileSystem` drives the same interface without touching the real filesystem. In production, the seam also contains the behaviour differences between filesystem event delivery on WSL and on Windows — quirks that would otherwise leak through every consumer.

```csharp:xmldocid
T:Pennington.Infrastructure.FileWatcher
```

### `FileWatchDependencyFactory` reconstructs services on change

Several services build expensive lookup tables from disk on startup: link resolvers, cross-reference uid maps, search indexes, sitemaps, and blog content resolvers. Rather than giving each service its own cache-bust logic, Pennington registers them through `AddFileWatched<T>`. That extension wires a singleton `FileWatchDependencyFactory<T>` that subscribes to `IFileWatcher.SubscribeToChanges`, alongside a transient front that resolves to whatever instance the factory currently holds.

When a change notification arrives, the factory drops its cached instance — disposing it if it implements `IDisposable` — and lets it be rebuilt on the next resolution via `ActivatorUtilities.CreateInstance<T>`. The mental model here is structural invalidation rather than explicit cache-busting: no service needs to know when to flush itself, because the factory discards and reconstructs the whole instance when the underlying content moves.

```csharp:xmldocid
T:Pennington.Infrastructure.FileWatchDependencyFactory`1
```

### `LiveReloadServer` broadcasts over WebSocket

`LiveReloadServer` is a singleton that subscribes to the same `IFileWatcher.SubscribeToChanges` hook in its constructor and maintains a `ConcurrentDictionary<string, WebSocket>` of connected browser sessions. On any watcher notification it walks the dictionary, sends the string `"reload"` to every open socket, and prunes closed ones. The endpoint is mapped at `/__pennington/reload` as a WebSocket path; when a browser upgrades the connection, `HandleAsync` parks it until the client disconnects.

The browser-side script wires `ws.onmessage` to `location.reload()` and `ws.onclose` to a one-second reconnect loop. This is what makes the experience feel seamless after `dotnet watch` restarts Kestrel: the browser reconnects automatically on the next tick, receives the next `"reload"` from the refreshed server, and no manual F5 is needed.

```csharp:xmldocid
T:Pennington.Infrastructure.LiveReloadServer
```

### Script injection is `DOTNET_WATCH`-gated

`LiveReloadScriptProcessor` is an `IResponseProcessor` at `Order = 20`, positioned between the HTML rewriting pipeline at `Order = 10` and the diagnostic overlay at `Order = 30`. Its dev-mode flag is evaluated once at field initialisation — `!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_WATCH"))` — and `ShouldProcess` returns `false` whenever it is false. When the flag is true, the processor finds the last `</body>` tag and inserts a short inline script that opens a WebSocket to `/__pennington/reload`.

The same `DOTNET_WATCH` check gates `UsePenningtonLiveReload` itself. If the environment variable is absent the middleware never maps the endpoint, so the socket path does not exist and could not be reached even if a script tried. This means there is no publish-time stripping step, no build configuration to set, and no dev-only flag to forget. Running `dotnet run` without `watch` and publishing via `dotnet build` both evaluate the gate the same way — `false` — and the result is identical: no script, no endpoint, no trace of the dev loop in the output.

```csharp:xmldocid
T:Pennington.Infrastructure.LiveReloadScriptProcessor
```

## Trade-offs

- **Cost — every content edit triggers a full page reload, not a patch.** The broadcast message is a single string (`"reload"`); the browser responds with `location.reload()`. There is no HMR-style diff, no scroll preservation, no island re-render. For a docs engine this is the right shape — the alternative (partial DOM swaps) would need diffing infrastructure that neither Markdig nor the response processors expose today.
- **Alternative considered — restart the host on every file change.** `dotnet watch` could in theory be configured to restart on `.md` changes. It was rejected because restart cost grows with the app (Kestrel bind, DI graph, Razor compile), and the cached derived state (`MarkdownLinkResolver`, `XrefResolver`, search index) would be thrown away on every typo. Instance invalidation via `FileWatchDependencyFactory<T>` is cheap and keeps the host warm.
- **Consequence — `DOTNET_WATCH` is the single source of dev-mode truth.** Injection, endpoint mapping, and client connection all read the same env var. Running `dotnet run` (without `watch`) and publishing via `build` both yield the same gate value — `false` — so nothing dev-only ever ships. To test a production-shaped response locally, run `dotnet run`; to bring the reload loop back, run `dotnet watch run` instead.
- **Consequence — file watchers are per-path, not recursive-from-root.** Each source directory is registered explicitly (primarily by `MarkdownContentService<T>` at construction). A change outside any watched path — say, editing a file in `bin/` — does not fire. This is deliberate: watching from the solution root would generate noise from IDE writes, NuGet caches, and build output. The cost is that brand-new content roots only become live after the host restarts.

## Further reading

- Reference: [DI and middleware extension methods](xref:reference.host.extensions)
- Reference: [Response processing interfaces](xref:reference.extension-points.response-processing)
- Explanation: [The response-processing pipeline](xref:explanation.core.response-processing)
- Explanation: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build)
