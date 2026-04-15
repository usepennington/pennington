---
title: "DI and middleware extension methods"
description: "Index of every AddPennington/UsePennington/Run* extension method across the Pennington, DocSite, BlogSite, MonorailCSS, Islands, and Roslyn packages."
sectionLabel: "Host Integration"
order: 406010
tags: [host, di, middleware, extensions]
uid: reference.host.extensions
---

The complete roster of public extension methods Pennington exposes for wiring the library into an ASP.NET Core host — `Add*` (DI registration), `Use*` (middleware and endpoints), and `Run*` (host entry points). Declared across `Pennington.Infrastructure.PenningtonExtensions`, `Pennington.Infrastructure.LiveReloadExtensions`, `Pennington.Islands.SpaNavigationExtensions`, `Pennington.Infrastructure.FileWatchedServiceExtensions`, `Pennington.DocSite.DocSiteServiceExtensions`, `Pennington.BlogSite.BlogSiteServiceExtensions`, `Pennington.MonorailCss.MonorailServiceExtensions`, and `Pennington.Roslyn.RoslynExtensions`.

## `IServiceCollection` extensions

Each row points to the options page that documents the configuration delegate.

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

Callers configure a `PenningtonOptions` via the delegate. See [`PenningtonOptions`](xref:reference.options.pennington-options) for the full property catalog.

### `AddDocSite`

Composes over `AddPennington`; takes a `Func<DocSiteOptions>` (not `Action<T>`) because the options instance is constructed by the caller. See [`DocSiteOptions`](xref:reference.options.docsite-options).

### `AddBlogSite`

Composes over `AddPennington` tuned for blogs. See [`BlogSiteOptions`](xref:reference.options.blogsite-options).

### `AddMonorailCss`

The options factory receives the resolved `IServiceProvider`; paired with `UseMonorailCss`. See [`MonorailCssOptions`](xref:reference.options.monorail-css-options).

### `AddPenningtonRoslyn`

The Roslyn-backed highlighter is registered unconditionally; symbol extraction and xmldocid preprocessing activate only when `SolutionPath` is configured. See [`RoslynOptions`](xref:reference.options.roslyn-options).

### `AddSpaNavigation`

Paired with `UseSpaNavigation` (an `IEndpointRouteBuilder` extension); already called by `AddDocSite`. See [`SpaNavigationOptions`](xref:reference.options.auxiliary-options).

### `AddFileWatched<T>`

Registers a singleton wrapped by `FileWatchDependencyFactory<T>` that reconstructs the instance on file-system change; no options delegate is taken.

## `WebApplication` and endpoint extensions

Ordering within a `Use*` call chain is load-bearing; see each method's detail below.

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

The mandatory middleware call for a bare `AddPennington` host; internal order is static files → locale routing → live reload → `ResponseProcessingMiddleware` → mapped feed/search endpoints.

### `UseDocSite`

Composes the full DocSite middleware chain including a call to `UsePennington`; callers do not invoke `UsePennington` separately.

### `UseBlogSite`

Composes the full BlogSite middleware chain including a call to `UsePennington`; maps `/rss.xml` when `BlogSiteOptions.EnableRss` is `true`.

### `UseMonorailCss`

The optional `path` argument overrides the default `/styles.css` endpoint; already called by `UseDocSite` and `UseBlogSite`.

### `UsePenningtonLocaleRouting`

Idempotent — subsequent calls are no-ops; called implicitly by `UsePennington`.

### `UsePenningtonLiveReload`

No-op outside `dotnet watch` (gated on the `DOTNET_WATCH` environment variable); pairs with `LiveReloadScriptProcessor`, which injects the reconnection script on the response side.

### `UseSpaNavigation`

Receiver is `IEndpointRouteBuilder`, not `WebApplication`; call inside `app.UseEndpoints(...)` or on a route group. Already invoked by `UseDocSite`.

## Host runtime helpers

Each method dispatches between dev-serve and static-build based on `args[0]`.

| Method | Signature | Package | Dispatches to | Notes |
|---|---|---|---|---|
| `RunOrBuildAsync` | `Task RunOrBuildAsync(this WebApplication, string[] args)` | `Pennington` | `app.RunAsync()` or `OutputGenerationService.GenerateAsync` | Dev-serve on plain `dotnet run`; on `build [baseUrl] [output]` starts the host, crawls it, writes output, sets the exit code on diagnostics. See [CLI and build arguments](xref:reference.host.cli). |
| `RunDocSiteAsync` | `Task RunDocSiteAsync(this WebApplication, string[] args)` | `Pennington.DocSite` | `RunOrBuildAsync` | Thin delegate for DocSite hosts that do not otherwise reference `Pennington.Infrastructure`. |
| `RunBlogSiteAsync` | `Task RunBlogSiteAsync(this WebApplication, string[] args)` | `Pennington.BlogSite` | `RunOrBuildAsync` | Thin delegate for the blog template; same behavior as `RunOrBuildAsync`. |

### `RunOrBuildAsync`

Dev and build share one rendering pipeline; the `build` branch calls `app.StartAsync()`, resolves `OutputGenerationService`, crawls the running host via HTTP, and writes to `OutputOptions.OutputDirectory`. All other invocations fall through to `app.RunAsync()`.

### `RunDocSiteAsync`

One-line delegate to `RunOrBuildAsync`; callers who already reference `Pennington.Infrastructure` may invoke `RunOrBuildAsync` directly.

### `RunBlogSiteAsync`

One-line delegate to `RunOrBuildAsync`; mirrors `RunDocSiteAsync`.

## Example

A complete DocSite host wiring all three layers — `AddDocSite`, `UseDocSite`, `RunDocSiteAsync` — in their canonical call order.

```csharp:path
examples/DocSiteScaffoldExample/Program.cs
```

The same three-call shape holds for every template: `Add*` builds the service graph, `Use*` mounts the middleware and endpoints, `Run*Async` reads `args` and either serves or builds.

## See also

- Reference: [CLI and build arguments](xref:reference.host.cli)
- Reference: [`PenningtonOptions`](xref:reference.options.pennington-options)
- Reference: [`DocSiteOptions`](xref:reference.options.docsite-options)
- Background: [Dev mode and build mode share one code path](xref:explanation.core.dev-vs-build)
