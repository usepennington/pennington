---
title: "Hot reload and file watching"
description: "How FileWatcher observes content and asset directories, how LiveReloadServer pushes change events over WebSocket, and how LiveReloadScriptProcessor keeps the script out of production builds."
section: "dev-experience"
order: 10
tags: []
uid: explanation.dev-experience.hot-reload
isDraft: true
search: false
llms: false
---

> **In this page.** How `FileWatcher` observes content and asset directories, how `LiveReloadServer` pushes change events over WebSocket, and how `LiveReloadScriptProcessor` keeps the script out of production builds.
>
> **Not in this page.** ASP.NET's own `dotnet watch` hot reload is an upstream feature with its own documentation; this page only covers Pennington's browser refresh loop layered on top of it.

## The question

Why does Pennington ship a second reload mechanism on top of `dotnet watch`, and how do its three moving pieces stay invisible to a published site?

## Context

- `dotnet watch` already rebuilds and restarts the ASP.NET host when C# or Razor files change, but it does not know about the Markdown, YAML front matter, or asset files that make up most of a Pennington site.
- Editing a `.md` file should refresh the browser without a manual reload; editing a CSS file in `wwwroot/` should do the same without a rebuild.
- The reload plumbing must not leak into the static output produced by `dotnet run -- build` — a published site is a folder of HTML that a CDN serves, with no dev host behind it.
- Alternatives considered: polling the server from the browser (wasted cycles, lag), SSE (one-way but heavier), a separate tool watching the output folder (breaks the "one code path for dev and build" invariant described in the architecture explanation).

## How it works

Three collaborators form a single loop: a filesystem watcher, a WebSocket broadcaster, and a response rewriter that injects a tiny client script. Each is gated so nothing touches a published build.

### Filesystem observation — `FileWatcher`

- Lives in `src/Pennington/Infrastructure/FileWatcher.cs`; implements `IFileWatcher`.
- Wraps `System.IO.Abstractions.IFileSystem.FileSystemWatcher.New`, one watcher per `path|pattern` key, deduped so repeated `AddPathWatch` calls are harmless.
- `NotifyFilter` covers `LastWrite | FileName | DirectoryName | CreationTime`; `IncludeSubdirectories` defaults to true.
- Each `Changed`/`Created`/`Deleted`/`Renamed` event invokes the caller's `onFileChanged(path, WatcherChangeTypes)` callback **and** calls `NotifySubscribers()`, which fans out to every `Action` registered via `SubscribeToChanges`.
- Two distinct audiences consume the same events: file-watched services (`MarkdownLinkResolver`, `XrefResolver`, `SearchIndexService`, `SitemapService`, `LlmsTxtService`, `BlogContentResolver`, `BlogSiteContentService`) rebuild their caches via `FileWatchDependencyFactory<T>`; `LiveReloadServer` forwards the signal to browsers.

```csharp:xmldocid
T:Pennington.Infrastructure.FileWatcher
```

### Broadcasting — `LiveReloadServer`

- Lives in `src/Pennington/Infrastructure/LiveReloadServer.cs`.
- At construction, subscribes to `IFileWatcher.SubscribeToChanges(NotifyClients)`; it owns no watchers of its own — the same signal that invalidates caches also reaches browsers.
- Holds a `ConcurrentDictionary<string, WebSocket>` of open clients keyed by `Guid.NewGuid()`.
- `NotifyClients` sends the literal UTF-8 bytes `"reload"` to every socket in `Open` state, pruning closed ones as it iterates.
- `HandleAsync` is invoked by the middleware mapped at `/__pennington/reload` (constant `ReloadPath`); it blocks on `ReceiveAsync` only so the socket stays open — the server sends, the client only listens.
- The payload deliberately carries no diff, no path, no event kind: the client's only response is `location.reload()`, so nothing richer is needed.

### Dev-only injection — `LiveReloadScriptProcessor`

- Lives in `src/Pennington/Infrastructure/LiveReloadScriptProcessor.cs`; `IResponseProcessor` with `Order => 20`, slotted between HTML rewriting (10) and the diagnostic overlay (30).
- Gate: `_isDevMode = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_WATCH"))`, evaluated once at construction. `dotnet watch` sets this variable; `dotnet run` and `dotnet run -- build` do not.
- `ShouldProcess` returns false outside dev mode, so `ProcessAsync` never runs during a build — the injected `<script>` cannot reach the emitted HTML.
- When dev mode is on, `ProcessAsync` inserts a ~10-line script immediately before the last `</body>`. The script opens a WebSocket to `/__pennington/reload`, calls `location.reload()` on any message, and retries with a 1-second backoff on close.
- The same `DOTNET_WATCH` gate guards `LiveReloadExtensions.UsePenningtonLiveReload` — if the variable is absent, the endpoint is never mapped at all, so even a forged script could not connect.

### The loop end-to-end

1. Author saves `hot-reload.md`. The OS fires a `Changed` event.
2. `FileWatcher` invokes the Markdown source's `onFileChanged` (invalidating any relevant caches) and then `NotifySubscribers()`.
3. `LiveReloadServer.NotifyClients` sends `"reload"` to every open WebSocket.
4. The injected script's `onmessage` handler fires `location.reload()`.
5. The browser re-requests the page. The static-build code path is untouched — dev-serve and build share the same HTTP pipeline, but the reload processor and endpoint are compiled out by the env-var gate during `build`.

## Trade-offs

- **Cost:** a full page reload, not a partial DOM patch. The simpler protocol means authors who edit a single paragraph still rebuild the whole client state. For a docs/blog workload that is a better trade than the complexity of computed diffs.
- **Alternative considered:** teaching `dotnet watch` to also track content files. Rejected because the Markdown pipeline already has to invalidate `MarkdownLinkResolver`, `XrefResolver`, and friends on change — `IFileWatcher` is the single notification primitive those services already depend on. Reusing it for browser reload keeps the contract minimal.
- **Consequence:** the `DOTNET_WATCH` gate is load-bearing in two places. If you introduce a new dev-only processor, put the gate in its `ShouldProcess` and, if it maps an endpoint, also inside its `Use…` extension — otherwise a published site will ship dev scaffolding.
- **Consequence:** the reload payload is intentionally opaque ("reload"), so you cannot drive smarter behavior (e.g., CSS-only swap) from the server without expanding the protocol. That's a deliberate floor, not an oversight.

## Further reading

- Reference: [Infrastructure API catalog](/reference/api/infrastructure)
- Explanation: [Response processing pipeline](/explanation/rendering/response-processing)
- Explanation: [Unified dev and build code path](/explanation/core/dev-and-build)
- External: [dotnet watch documentation](https://learn.microsoft.com/dotnet/core/tools/dotnet-watch)
