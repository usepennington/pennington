---
title: "Hot Reload and File Watching"
description: "The difference between .NET hot reload and Penn's live reload — covering IFileWatcher, FileWatchDependencyFactory<T> invalidate-and-recreate pattern, which services use AddFileWatched, the WebSocket protocol, and AsyncLazy for thread-safe recomputation"
uid: "penn.explanation.hot-reload-and-file-watching"
order: 10
---

Distinguish between two reload mechanisms and explain how they compose. (1) .NET hot reload via `dotnet watch`: monitors C# and Razor files, applies incremental updates to the running app without restart. (2) Penn's live reload: a WebSocket connection between the browser and `LiveReloadServer` that triggers a page refresh when content files change. Explain `FileWatchDependencyFactory<T>` — the "invalidate and recreate" pattern where a service instance is cached until a watched file changes, then lazily recreated on next access via `AsyncLazy<T>`. This avoids both stale caches and expensive eager recomputation. List which services use this pattern: XrefResolver (content change → rebuild UID lookup), SearchIndexService (content change → rebuild index), SitemapService, LlmsTxtService. Discuss why `AsyncLazy<T>` is needed for thread safety — multiple concurrent requests may trigger recreation, but only one should actually do the work. Explain the WebSocket protocol at `/__penn/reload` and how `LiveReloadScriptProcessor` injects the client-side script only when the `DOTNET_WATCH` environment variable is set.
