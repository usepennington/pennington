---
title: "Hot reload and file watching"
description: "Why Pennington layers its own content watcher and WebSocket reload on top of dotnet watch, and how the dev-only script is kept out of published builds."
sectionLabel: "Developer Experience"
order: 10
tags: [live-reload, file-watching, dev-loop, websockets]
uid: explanation.dev-experience.hot-reload
---

> **In this page.** How the file watcher observes content and asset directories, how change events reach the browser over WebSocket, and how the live-reload script is kept out of production builds.
>
> **Not in this page.** ASP.NET's own `dotnet watch` hot reload — Pennington relies on it for C# and Razor changes and does not re-implement it.

## The question

_One sentence framed as the reader's question. Something like: "`dotnet watch` already restarts the app when I change code — why does Pennington ship its own file watcher and WebSocket reload channel?" Keep it to one sentence; the Context section earns the answer._

## Context

_Three to five sentences. Start by noting that `dotnet watch` is the upstream tool that rebuilds and restarts when C# / Razor files change — Pennington relies on it for the code-change half of the dev loop and does not try to replace it. Then name the gap: content files (`.md`, front matter, images, assets under a source's `ContentPath`) don't trigger `dotnet watch`, because they're not part of the compilation. A naive fix — restart on every file change — would tear down Kestrel for a markdown typo. The chosen shape instead: an in-process `FileWatcher` that listens for content-directory changes, a `FileWatchDependencyFactory<T>` that invalidates derived caches, and a WebSocket the browser polls to reload the page. Close by flagging the invariant that follows — this whole apparatus must vanish in published builds, which shapes the last subsection._

## How it works

_The mechanism reads as one sentence: content files change, cached services drop their state, the browser reloads. Four subsections expand each link in that chain; stay narrative, resist listing API members._

### `FileWatcher` wraps the FS watcher

_Two or three paragraphs. `FileWatcher : IFileWatcher` is a thin wrapper over `System.IO.Abstractions.IFileSystemWatcher`. Callers — primarily `MarkdownContentService<T>` when it registers its source directory, and the content pipeline's auxiliary services — invoke `AddPathWatch(path, pattern, onFileChanged)` to subscribe a specific directory, keyed by `path|pattern` so the same path isn't double-watched. The watcher hooks `Changed`, `Created`, `Deleted`, `Renamed` and fires a single `NotifySubscribers()` after invoking the per-path callback. Note the reason for the abstraction: tests drive a `MockFileSystem` through the same interface, and WSL vs Windows filesystem-event quirks stay behind one seam._

```csharp:xmldocid
T:Pennington.Infrastructure.FileWatcher
```

_Optional — keep this fence if the wrapper's shape (abstraction over `IFileSystemWatcher`, `path|pattern` dedup dictionary) is clearer as code than as prose. Drop it if the narrative stands._

### `FileWatchDependencyFactory` reconstructs services on change

_Two or three paragraphs. Describe the cache-invalidation pattern: services that build expensive lookup tables from disk (`MarkdownLinkResolver`'s source → URL index, `XrefResolver`'s uid map, `SearchIndexService`, `SitemapService`, `LlmsTxtService`, the blog's `BlogContentResolver` and `BlogSiteContentService`) are registered through `AddFileWatched<T>`. That extension wires a singleton `FileWatchDependencyFactory<T>` that subscribes to `IFileWatcher.SubscribeToChanges`, plus a transient front that resolves to whatever instance the factory currently holds. On a change notification the factory drops its cached instance and disposes it if it implements `IDisposable`; the next resolution calls `ActivatorUtilities.CreateInstance<T>` to build a fresh one from the current filesystem state. The reader's mental model: caches don't need explicit bust calls — they're structurally invalidated whenever content moves._

```csharp:xmldocid
T:Pennington.Infrastructure.FileWatchDependencyFactory`1
```

_Optional — the class is small and its invalidate-on-notify loop reads clearly in source. Include if the prose alone leaves the reader guessing at the locking or lifecycle; drop otherwise._

### `LiveReloadServer` broadcasts over WebSocket

_Two or three paragraphs. Explain that `LiveReloadServer` is a singleton that subscribes to the same `IFileWatcher.SubscribeToChanges` hook in its constructor and holds a `ConcurrentDictionary<string, WebSocket>` of connected browsers. On any watcher notification it walks the dictionary and sends the literal bytes `"reload"` to every `Open` socket, pruning closed ones. `LiveReloadExtensions.UsePenningtonLiveReload` maps the internal constant `ReloadPath = "/__pennington/reload"` as a WebSocket endpoint; on upgrade it hands the socket to `LiveReloadServer.HandleAsync`, which parks the connection until the client closes. The browser-side script (injected by the next subsection's processor) wires `ws.onmessage` to `location.reload()` and `ws.onclose` to a one-second reconnect loop — so after `dotnet watch` restarts Kestrel, the browser reconnects on its own and picks up the next change without a manual F5._

```csharp:xmldocid
T:Pennington.Infrastructure.LiveReloadServer
```

_Optional — the class is short and the subscribe-broadcast shape is load-bearing to the explanation. Keep the fence unless the prose has already shown the pattern clearly._

### Script injection is `DOTNET_WATCH`-gated

_Two or three paragraphs. The client-side half is `LiveReloadScriptProcessor`, an `IResponseProcessor` with `Order => 20` that sits between the HTML rewriting pipeline (`Order = 10`) and the diagnostic overlay (`Order = 30`). Its dev-mode flag is set once in a field initializer: `!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_WATCH"))`. `ShouldProcess` returns `false` whenever that flag is false, so the processor is effectively inert outside `dotnet watch`. When it does run, it finds the last `</body>` and inserts a short script that opens a WebSocket to `/__pennington/reload`. Critically, the same `DOTNET_WATCH` check gates `UsePenningtonLiveReload` itself — if the env var is missing, the middleware never wires the `/__pennington/reload` endpoint into the pipeline. So in a published build the processor emits nothing and the endpoint does not exist; the script literally cannot be injected and the socket path literally 404s. There is no "strip dev-only markers at publish" step because there is nothing to strip._

```csharp:xmldocid
T:Pennington.Infrastructure.LiveReloadScriptProcessor
```

_Keep the fence — the `DOTNET_WATCH` check in the field initializer and the `Order = 20` slot are the two facts the subsection exists to make explicit._

## Trade-offs

- **Cost — every content edit triggers a full page reload, not a patch.** The broadcast message is a single string (`"reload"`); the browser responds with `location.reload()`. There is no HMR-style diff, no scroll preservation, no island re-render. For a docs engine this is the right shape — the alternative (partial DOM swaps) would need diffing infrastructure that neither Markdig nor the response processors expose today.
- **Alternative considered — restart the host on every file change.** `dotnet watch` could in theory be configured to restart on `.md` changes. It was rejected because restart cost grows with the app (Kestrel bind, DI graph, Razor compile), and the cached derived state (`MarkdownLinkResolver`, `XrefResolver`, search index) would be thrown away on every typo. Instance invalidation via `FileWatchDependencyFactory<T>` is cheap and keeps the host warm.
- **Consequence — `DOTNET_WATCH` is the single source of dev-mode truth.** Injection, endpoint mapping, and client connection all read the same env var. Running `dotnet run` (without `watch`) and publishing via `build` both yield the same gate value — `false` — so nothing dev-only ever ships. If you need to test a production-shaped response locally, just run `dotnet run`; if you want the reload loop back, `dotnet watch run`.
- **Consequence — file watchers are per-path, not recursive-from-root.** Each source directory is registered explicitly (primarily by `MarkdownContentService<T>` at construction). A change outside any watched path — say, editing a file in `bin/` — does not fire. This is deliberate: watching from the solution root would generate noise from IDE writes, NuGet caches, and build output. The cost is that brand-new content roots only become live after the host restarts.

## Further reading

- Reference: [DI and middleware extension methods](xref:reference.host.extensions)
- Reference: [Response processing interfaces](xref:reference.extension-points.response-processing)
- Explanation: [The response-processing pipeline](xref:explanation.core.response-processing)
- Explanation: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build)
