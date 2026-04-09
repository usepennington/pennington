---
title: "File Watching and DI Wiring"
description: "Reference for IFileWatcher, FileWatchDependencyFactory<T> (invalidation pattern), AddFileWatched<T>, AddPennington/UsePennington/RunOrBuildAsync registration details, service lifetimes (singleton vs scoped vs transient), LiveReloadServer, and the WebSocket reload protocol"
uid: "penn.reference.file-watching-and-di-wiring"
order: 10
---

Document the infrastructure types that most users won't touch but extension authors need. `IFileWatcher`: `AddPathWatch(path, pattern, callback)`. `FileWatcher` implementation using `FileSystemWatcher`. `FileWatchDependencyFactory<T>`: the invalidation-on-change pattern — holds a current instance, recreates it when watched files change, thread-safe via `AsyncLazy<T>`. `AddFileWatched<T>()` and `AddFileWatched<TService, TImpl>()` extension methods. List which built-in services use file watching: XrefResolver, SearchIndexService, SitemapService, LlmsTxtService, BlogContentResolver. Document the DI registration methods: `AddPennington` (what it registers and in what lifetime), `UsePennington` (middleware it adds), `UsePenningtonLocaleRouting` (when to call it relative to MapRazorComponents), `RunOrBuildAsync` (mode detection from args). Document service lifetimes: singletons (options, parsers, factories), scoped (DiagnosticContext, LocaleContext), transient (content services, renderers, pipelines). Document live reload: `LiveReloadServer` WebSocket at `/__penn/reload`, triggered when `DOTNET_WATCH` env var is set.
