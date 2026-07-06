---
title: "Hot reload and file watching"
description: "Why Pennington ships its own file watcher and WebSocket reload channel, and how the dev-only script is kept out of published builds."
sectionLabel: "Developer Experience"
order: 1
tags: [live-reload, file-watching, dev-loop, websockets]
uid: explanation.dev-experience.hot-reload
---

Content files — `.md` sources, front matter, images, assets tracked under a source's `ContentPath` — are not part of the .NET compilation. Restarting the host for every markdown typo would tear down Kestrel and throw away the expensive in-memory caches that make the second request fast. Pennington instead watches content in process and reloads only the browser: an in-process file watcher, services that discard and rebuild their derived caches on change, and a debounced WebSocket channel through which the browser is told to reload. WebSocket also makes server restarts easy to detect on the client — the socket closes and reconnects, and the browser reloads — without the careful `onerror` plumbing SSE would need for the same signal. Polling, the third alternative, adds latency under load and noise under idle.

## How it works

The mechanism is a single chain: files change, cached services drop their state, a debounce window elapses, and the browser reloads.

### Watching content directories

A service tells the engine which directories to watch by declaring `WatchScopes` — the public contract through which every file-reactive service registers its content roots. Creates, deletes, and renames count as changes, not just edits in place, so a new markdown file or a deleted asset reloads the same way a save does. The watcher behaves consistently across Windows and WSL, the two platforms most contributors run, so the dev loop feels the same on either.

### The `IFileWatchAware` contract

Several services build expensive lookup tables from disk on startup: link resolvers, cross-reference uid maps, search indexes, sitemaps, and blog content resolvers. They register through `AddFileWatched<T>`, which is constrained to types implementing `IFileWatchAware` — the single contract every file-reactive service shares. A service declares the directories it needs watched and an `OnFileChanged` method whose return value says how the change should be handled: `Ignore` it, report it `Refreshed` its own state, or ask to be `Recreate`d.

The services above return `Recreate`, so on the next request the stale instance is dropped — disposed if it implements `IDisposable` — and a fresh one is rebuilt through normal constructor injection. The approach is to discard and rebuild the whole instance rather than bust individual caches: no service needs to know when to flush itself, because the engine replaces the entire instance when the underlying content moves.

This is the extension point you reach for when your own service caches something derived from content files. The how-tos that lean on it: [write a custom content service](xref:how-to.content-services.custom-content-service), [publish a custom feed from a content service](xref:how-to.feeds.custom-feed), and [use a YAML or JSON data file in pages](xref:how-to.content-services.data-files).

### Debouncing and broadcasting over WebSocket

`LiveReloadServer` resets a 300ms debounce timer on every change notification, so it broadcasts a single reload only after 300ms of quiet — coalescing rapid saves (editor auto-save, multi-file renames) into one browser reload. It listens on the WebSocket endpoint `/__pennington/reload`, which is worth knowing if a reverse proxy or a Content-Security-Policy sits in front of the dev server and needs to allow the upgrade.

### Script injection and reconnection

`LiveReloadScriptProcessor` injects an inline script before the closing `</body>` that opens a WebSocket to `/__pennington/reload`, reloads when a reload message arrives, and reloads on reconnect so a host restart refreshes the browser without waiting for a file change. Guards that keep it from firing mid-navigation or before the response pipeline settles are tuning details, not knobs a consumer sets.

### Build-mode gating

Both `LiveReloadScriptProcessor` and `UseLiveReload` check `PenningtonCli.Current.IsHeadlessOneShot` — true for any headless one-shot run, which covers `build` and `diag` alike, not just the `build` verb. When it is true, the processor's `ShouldProcess` returns `false` and the middleware skips endpoint registration entirely. This means the `OutputGenerationService` crawler sees clean HTML with no script and no WebSocket endpoint: no publish-time stripping step, no build configuration to set, and no dev-only flag to forget.

## Relation to `dotnet watch` and .NET Hot Reload

This is a different layer from .NET Hot Reload and `dotnet watch`, and the two are complementary. .NET Hot Reload patches running CLR code — your `.cs` and `.razor` source — and `dotnet watch` restarts or re-applies edits when compiled code changes. Pennington's watcher covers the half they don't: content files (`.md`, front matter, images, `_meta.yml`, data files) that never enter the compilation. Run the host under `dotnet watch` and you get both — code edits patched by the runtime, content edits reloaded by Pennington — without either stepping on the other.

## Disabling reload or tuning the debounce

In serve mode live reload is always on, and the 300ms debounce and reconnection behavior are fixed; there is no option to tune them or switch reload off while still serving. The single off-switch is build mode: a headless one-shot run (`build` or `diag`) gates the whole subsystem out, which is exactly what you want for published output. If you need a dev server with no reload at all, host the engine without `UsePennington`'s dev path rather than reaching for a flag.

## Further reading

- Reference: [DI and middleware extension methods](xref:reference.host.extensions)
- Reference: [Response processing interfaces](xref:reference.api.i-response-processor)
- Explanation: [The response-processing pipeline](xref:explanation.core.response-processing)
- Explanation: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build)
