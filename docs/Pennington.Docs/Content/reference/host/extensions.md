---
title: "DI and middleware extension methods"
description: "Index of every AddPennington/UsePennington/Run* extension method across the Pennington, DocSite, BlogSite, MonorailCSS, Islands, and Roslyn packages."
sectionLabel: "Host Integration"
order: 10
tags: [host, di, middleware, extensions]
uid: reference.host.extensions
---

> **In this page.** The at-a-glance summary of every `IServiceCollection` and `WebApplication` extension — `AddPennington`, `AddDocSite`, `AddBlogSite`, `AddMonorailCss`, `AddPenningtonRoslyn`, `AddSpaNavigation`, `AddFileWatched`, `UsePennington`, `UseDocSite`, `UseBlogSite`, `UseMonorailCss`, `UsePenningtonLocaleRouting`, `UsePenningtonLiveReload`, `UseSpaNavigation`, `RunOrBuildAsync`, `RunDocSiteAsync`, `RunBlogSiteAsync`. Each extension's configuration surface lives on the matching options page; this page is the index, not a duplicate catalog.
>
> **Not in this page.** The underlying services each extension wires up (see the pipeline, islands, and infrastructure reference pages). Parameter-level option catalogs live on the `/reference/options/*` pages.

## Summary

_One sentence: what this page is._ The complete roster of public extension methods Pennington exposes for wiring the library into an ASP.NET Core host — `Add*` (DI registration), `Use*` (middleware and endpoints), and `Run*` (host entry points).
_One sentence: where the methods live._ Declared across `Pennington.Infrastructure.PenningtonExtensions`, `Pennington.Infrastructure.LiveReloadExtensions`, `Pennington.Islands.SpaNavigationExtensions`, `Pennington.Infrastructure.FileWatchedServiceExtensions`, `Pennington.DocSite.DocSiteServiceExtensions`, `Pennington.BlogSite.BlogSiteServiceExtensions`, `Pennington.MonorailCss.MonorailServiceExtensions`, and `Pennington.Roslyn.RoslynExtensions`.

## `IServiceCollection` extensions

_Register services into the DI container. Each row points to the options page that documents the configuration delegate._

| Method | Signature | Package | Options surface | Notes |
|---|---|---|---|---|
| `AddPennington` | `IServiceCollection AddPennington(this IServiceCollection, Action<PenningtonOptions>)` | `Pennington` | [`PenningtonOptions`](xref:reference.options.pennington-options) | Core registration: content sources, pipeline, rewriters, feeds, search, llms.txt, diagnostics. |
| `AddDocSite` | `IServiceCollection AddDocSite(this IServiceCollection, Func<DocSiteOptions>)` | `Pennington.DocSite` | [`DocSiteOptions`](xref:reference.options.docsite-options) | Composes `AddPennington` + `AddMonorailCss` + `AddSpaNavigation`; wires `ContentResolver` and the DocSite article slot renderer. |
| `AddBlogSite` | `IServiceCollection AddBlogSite(this IServiceCollection, Func<BlogSiteOptions>)` | `Pennington.BlogSite` | [`BlogSiteOptions`](xref:reference.options.blogsite-options) | Composes `AddPennington` + `AddMonorailCss`; wires file-watched `BlogContentResolver` and `BlogSiteContentService`. |
| `AddMonorailCss` | `IServiceCollection AddMonorailCss(this IServiceCollection, Func<IServiceProvider, MonorailCssOptions>? = null)` | `Pennington.MonorailCss` | [`MonorailCssOptions`](xref:reference.options.monorail-css-options) | Registers `CssClassCollector`, the stylesheet service, and the `CssClassCollectorProcessor` as `IResponseProcessor`. |
| `AddPenningtonRoslyn` | `IServiceCollection AddPenningtonRoslyn(this IServiceCollection, Action<RoslynOptions>? = null)` | `Pennington.Roslyn` | [`RoslynOptions`](xref:reference.options.roslyn-options) | Always registers `RoslynHighlighter`; when `SolutionPath` is set, adds workspace + symbol services + the xmldocid code-block preprocessor. |
| `AddSpaNavigation` | `IServiceCollection AddSpaNavigation(this IServiceCollection, Action<SpaNavigationOptions>? = null)` | `Pennington` | [`SpaNavigationOptions`](xref:reference.options.auxiliary-options) | Registers the SPA envelope services backing the `_spa-data` JSON endpoint; `UseSpaNavigation` maps the endpoint. |
| `AddFileWatched<T>` | `IServiceCollection AddFileWatched<T>(this IServiceCollection) where T : class` | `Pennington` | _none_ | Registers `T` as a singleton behind `FileWatchDependencyFactory<T>` that recreates on watched-file changes. A two-parameter overload `AddFileWatched<TService, TImplementation>` exists for interface/implementation pairs. |

### `AddPennington`

```csharp:xmldocid
M:Pennington.Infrastructure.PenningtonExtensions.AddPennington(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.Infrastructure.PenningtonOptions})
```

_Core entry point; callers configure a `PenningtonOptions` via the delegate. See [`PenningtonOptions`](xref:reference.options.pennington-options) for the full property catalog._

### `AddDocSite`

```csharp:xmldocid
M:Pennington.DocSite.DocSiteServiceExtensions.AddDocSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.DocSite.DocSiteOptions})
```

_Template-style composition over `AddPennington`. Takes a `Func<DocSiteOptions>` (not `Action<T>`) because the options instance is constructed by the caller. See [`DocSiteOptions`](xref:reference.options.docsite-options)._

### `AddBlogSite`

```csharp:xmldocid
M:Pennington.BlogSite.BlogSiteServiceExtensions.AddBlogSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.BlogSite.BlogSiteOptions})
```

_Template-style composition over `AddPennington` tuned for blogs. See [`BlogSiteOptions`](xref:reference.options.blogsite-options)._

### `AddMonorailCss`

```csharp:xmldocid
M:Pennington.MonorailCss.MonorailServiceExtensions.AddMonorailCss(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{System.IServiceProvider,Pennington.MonorailCss.MonorailCssOptions})
```

_Paired with `UseMonorailCss`. The options factory receives the resolved `IServiceProvider`. See [`MonorailCssOptions`](xref:reference.options.monorail-css-options)._

### `AddPenningtonRoslyn`

```csharp:xmldocid
M:Pennington.Roslyn.RoslynExtensions.AddPenningtonRoslyn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.Roslyn.RoslynOptions})
```

_The Roslyn-backed highlighter is registered unconditionally; symbol extraction and xmldocid preprocessing activate only when `SolutionPath` is configured. See [`RoslynOptions`](xref:reference.options.roslyn-options)._

### `AddSpaNavigation`

```csharp:xmldocid
M:Pennington.Islands.SpaNavigationExtensions.AddSpaNavigation(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.Islands.SpaNavigationOptions})
```

_Paired with `UseSpaNavigation` (an `IEndpointRouteBuilder` extension). Already called by `AddDocSite`; bare `AddPennington` hosts call it explicitly when they need the SPA envelope. See [`SpaNavigationOptions`](xref:reference.options.auxiliary-options)._

### `AddFileWatched<T>`

```csharp:xmldocid
M:Pennington.Infrastructure.FileWatchedServiceExtensions.AddFileWatched``1(Microsoft.Extensions.DependencyInjection.IServiceCollection)
```

_Registers a singleton wrapped by `FileWatchDependencyFactory<T>` so the instance is reconstructed on file-system change. No options delegate; the registered type is responsible for declaring which paths it watches._

## `WebApplication` and endpoint extensions

_Mount middleware, endpoints, and static-file roots. Ordering matters within a single `Use*` call chain — see the individual method pages._

| Method | Signature | Package | Call site | Notes |
|---|---|---|---|---|
| `UsePennington` | `WebApplication UsePennington(this WebApplication)` | `Pennington` | After routing, before endpoints | Mounts per-source/per-locale static files, locale routing, live reload, `ResponseProcessingMiddleware`, and maps `/search-index-{code}.json`, `/sitemap.xml`, optional `/llms.txt`. |
| `UseDocSite` | `WebApplication UseDocSite(this WebApplication)` | `Pennington.DocSite` | Single top-level call | Wires locale routing → antiforgery → static files → `MapRazorComponents<App>` → MonorailCSS → SPA nav → `UsePennington`. |
| `UseBlogSite` | `WebApplication UseBlogSite(this WebApplication)` | `Pennington.BlogSite` | Single top-level call | Wires antiforgery → static files → `MapRazorComponents<App>` → MonorailCSS → `UsePennington`; maps `/rss.xml` when `EnableRss`. |
| `UseMonorailCss` | `WebApplication UseMonorailCss(this WebApplication, string path = "/styles.css")` | `Pennington.MonorailCss` | After routing, before endpoints | Scans `MonorailCssOptions.ContentPaths`, maps the stylesheet endpoint at `path`. |
| `UsePenningtonLocaleRouting` | `WebApplication UsePenningtonLocaleRouting(this WebApplication)` | `Pennington` | Before endpoint mapping | Registers `RequestLocalizationOptions`, `PenningtonUrlRequestCultureProvider`, and `LocaleDetectionMiddleware`, then calls `UseRouting()`; idempotent via the `Pennington.LocaleRoutingAdded` key. |
| `UsePenningtonLiveReload` | `WebApplication UsePenningtonLiveReload(this WebApplication)` | `Pennington` | After routing | Gated on `DOTNET_WATCH`; maps the `/__pennington/reload` WebSocket to `LiveReloadServer`. |
| `UseSpaNavigation` | `IEndpointRouteBuilder UseSpaNavigation(this IEndpointRouteBuilder)` | `Pennington` | Inside endpoint configuration | Maps the `SpaNavigationOptions.DataPath` JSON endpoint (default `/_spa-data`). Note the `IEndpointRouteBuilder` receiver. |

### `UsePennington`

```csharp:xmldocid
M:Pennington.Infrastructure.PenningtonExtensions.UsePennington(Microsoft.AspNetCore.Builder.WebApplication)
```

_The single mandatory middleware call for a bare `AddPennington` host. Order inside is load-bearing: static files → locale routing → live reload → `ResponseProcessingMiddleware` → mapped feed/search endpoints._

### `UseDocSite`

```csharp:xmldocid
M:Pennington.DocSite.DocSiteServiceExtensions.UseDocSite(Microsoft.AspNetCore.Builder.WebApplication)
```

_Composes the full DocSite middleware chain including a call to `UsePennington`; callers do not invoke `UsePennington` separately._

### `UseBlogSite`

```csharp:xmldocid
M:Pennington.BlogSite.BlogSiteServiceExtensions.UseBlogSite(Microsoft.AspNetCore.Builder.WebApplication)
```

_Composes the full BlogSite middleware chain including a call to `UsePennington`. Maps `/rss.xml` when `BlogSiteOptions.EnableRss` is true._

### `UseMonorailCss`

```csharp:xmldocid
M:Pennington.MonorailCss.MonorailServiceExtensions.UseMonorailCss(Microsoft.AspNetCore.Builder.WebApplication,System.String)
```

_Optional second positional argument overrides the default `/styles.css` path. Already called by `UseDocSite`/`UseBlogSite`._

### `UsePenningtonLocaleRouting`

```csharp:xmldocid
M:Pennington.Infrastructure.PenningtonExtensions.UsePenningtonLocaleRouting(Microsoft.AspNetCore.Builder.WebApplication)
```

_Idempotent: subsequent calls are no-ops. Called implicitly by `UsePennington`; invoke explicitly when you need locale routing before other middleware that depends on a stripped path._

### `UsePenningtonLiveReload`

```csharp:xmldocid
M:Pennington.Infrastructure.LiveReloadExtensions.UsePenningtonLiveReload(Microsoft.AspNetCore.Builder.WebApplication)
```

_No-op outside `dotnet watch` (gated on `DOTNET_WATCH` environment variable). Pairs with `LiveReloadScriptProcessor`, which injects the reconnection script on the response side._

### `UseSpaNavigation`

```csharp:xmldocid
M:Pennington.Islands.SpaNavigationExtensions.UseSpaNavigation(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder)
```

_Receiver is `IEndpointRouteBuilder`, not `WebApplication`; call inside `app.UseEndpoints(...)` or on a route group. Already invoked by `UseDocSite`._

## Host runtime helpers

_Entry points that decide between dev-serve and static-build based on `args[0]`._

| Method | Signature | Package | Dispatches to | Notes |
|---|---|---|---|---|
| `RunOrBuildAsync` | `Task RunOrBuildAsync(this WebApplication, string[] args)` | `Pennington` | `app.RunAsync()` or `OutputGenerationService.GenerateAsync` | Dev-serve on plain `dotnet run`; on `build [baseUrl] [output]` starts the host, crawls it, writes output, sets the exit code on diagnostics. See [CLI and build arguments](xref:reference.host.cli). |
| `RunDocSiteAsync` | `Task RunDocSiteAsync(this WebApplication, string[] args)` | `Pennington.DocSite` | `RunOrBuildAsync` | Thin delegate; exists so DocSite hosts do not need to import `Pennington.Infrastructure` just to reach `RunOrBuildAsync`. |
| `RunBlogSiteAsync` | `Task RunBlogSiteAsync(this WebApplication, string[] args)` | `Pennington.BlogSite` | `RunOrBuildAsync` | Thin delegate for the blog template; same behavior as `RunOrBuildAsync`. |

### `RunOrBuildAsync`

```csharp:xmldocid
M:Pennington.Infrastructure.PenningtonExtensions.RunOrBuildAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])
```

_Unified code path — dev and build share one rendering pipeline. The `build` branch calls `app.StartAsync()`, resolves `OutputGenerationService`, crawls the running host via HTTP, and writes to `OutputOptions.OutputDirectory`. Non-`build` invocations fall through to `app.RunAsync()`._

### `RunDocSiteAsync`

```csharp:xmldocid
M:Pennington.DocSite.DocSiteServiceExtensions.RunDocSiteAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])
```

_One-line delegate to `RunOrBuildAsync`. Callers who already reference `Pennington.Infrastructure` can invoke `RunOrBuildAsync` directly._

### `RunBlogSiteAsync`

```csharp:xmldocid
M:Pennington.BlogSite.BlogSiteServiceExtensions.RunBlogSiteAsync(Microsoft.AspNetCore.Builder.WebApplication,System.String[])
```

_One-line delegate to `RunOrBuildAsync`. Mirrors `RunDocSiteAsync`._

## Example

_A complete DocSite host wiring all three layers — `AddDocSite`, `UseDocSite`, `RunDocSiteAsync` — in their canonical call order._

```csharp:path
examples/DocSiteScaffoldExample/Program.cs
```

The same three-call shape holds for every template: `Add*` builds the service graph, `Use*` mounts the middleware and endpoints, `Run*Async` reads `args` and either serves or builds.

## See also

- Reference: [CLI and build arguments](xref:reference.host.cli)
- Reference: [`PenningtonOptions`](xref:reference.options.pennington-options)
- Reference: [`DocSiteOptions`](xref:reference.options.docsite-options)
- Background: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build)
