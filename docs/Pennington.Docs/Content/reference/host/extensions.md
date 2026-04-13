---
title: "DI and middleware extension methods"
description: "Every IServiceCollection and WebApplication extension method Pennington ships: AddPennington, AddDocSite, AddBlogSite, AddMonorailCss, AddPenningtonRoslyn, AddSpaNavigation, AddFileWatched, UsePennington, UseDocSite, UseBlogSite, UseMonorailCss, UsePenningtonLocaleRouting, UsePenningtonLiveReload, UseSpaNavigation, RunOrBuildAsync, RunDocSiteAsync, RunBlogSiteAsync."
section: "host"
order: 10
tags: []
uid: reference.host.extensions
isDraft: true
search: false
llms: false
---

> **In this page.** Every `IServiceCollection` and `WebApplication` extension method shipped by Pennington — `AddPennington`, `AddDocSite`, `AddMonorailCss`, `UsePennington`, `UseDocSite`, `UsePenningtonLocaleRouting`, `UsePenningtonLiveReload`, `UseMonorailCss`, `RunOrBuildAsync`, `RunDocSiteAsync`, and their siblings.
>
> **Not in this page.** The underlying services these extensions wire up — see the individual reference pages for `PenningtonOptions`, `DocSiteOptions`, `BlogSiteOptions`, `MonorailCssOptions`, `RoslynOptions`, and the content pipeline.

## Summary

The public DI and middleware surface area is composed of static extension methods on `IServiceCollection` and `WebApplication`, grouped by product package.
They live under `Pennington.Infrastructure`, `Pennington.Islands`, `Pennington.DocSite`, `Pennington.BlogSite`, `Pennington.MonorailCss`, and `Pennington.Roslyn`.

## Core — `Pennington.Infrastructure.PenningtonExtensions`

Defined in `src/Pennington/Infrastructure/PenningtonExtensions.cs`.

| Method | Full signature | Purpose |
|---|---|---|
| `AddPennington` | `public static IServiceCollection AddPennington(this IServiceCollection services, Action<PenningtonOptions> configure)` | Registers core Pennington services: options, file-system abstraction, `IFileWatcher`, highlighters (`TextMateHighlighter`, `ShellHighlighter`, user-registered), `MarkdownPipeline`, `MarkdownLinkResolver`, `IContentRenderer`, per-source `MarkdownContentService<T>` and `IContentParser`, optional `RazorPageContentService`, `IContentPipeline`, `XrefResolver`/`XrefResolvingService`, the three built-in `IHtmlResponseRewriter`s, the three built-in `IResponseProcessor`s, `LiveReloadServer`, feed/sitemap/search/llms.txt services, `DiagnosticContext`, `LocaleContext`, `IStringLocalizerFactory`, and `OutputGenerationService`. |
| `UsePennington` | `public static WebApplication UsePennington(this WebApplication app)` | Configures the middleware pipeline: validates content paths, serves static files from the content root, from every registered markdown source, and from each non-default-locale subdirectory; calls `UsePenningtonLocaleRouting` (idempotent), `UsePenningtonLiveReload`, and `UseMiddleware<ResponseProcessingMiddleware>`; maps one `/search-index-{code}.json` endpoint per configured locale, `/sitemap.xml`, and (when `LlmsTxtOptions` is registered) `/llms.txt`. |
| `UsePenningtonLocaleRouting` | `public static WebApplication UsePenningtonLocaleRouting(this WebApplication app)` | No-op unless `PenningtonOptions.Localization.IsMultiLocale` is true. Registers `RequestLocalizationOptions` with `PenningtonUrlRequestCultureProvider`, `CookieRequestCultureProvider`, and `AcceptLanguageHeaderRequestCultureProvider`; adds `LocaleDetectionMiddleware`; then calls `UseRouting()` so endpoint matching sees the locale-stripped path. Guarded by the `Pennington.LocaleRoutingAdded` `IApplicationBuilder` property to prevent double-registration. |
| `RunOrBuildAsync` | `public static Task RunOrBuildAsync(this WebApplication app, string[] args)` | Calls `StaticWebAssetsLoader.UseStaticWebAssets`, then branches on `args[0]`: `"build"` starts the host, resolves `OutputGenerationService`, calls `GenerateAsync(app.Urls.First())`, stops the host, writes the `BuildReport` to stdout, and sets `Environment.ExitCode = 1` on errors. Any other argument shape falls through to `app.RunAsync()`. |

## Live reload — `Pennington.Infrastructure.LiveReloadExtensions`

Defined in `src/Pennington/Infrastructure/LiveReloadServer.cs`.

| Method | Full signature | Purpose |
|---|---|---|
| `UsePenningtonLiveReload` | `public static WebApplication UsePenningtonLiveReload(this WebApplication app)` | Returns `app` unchanged when the `DOTNET_WATCH` environment variable is empty or missing. Otherwise calls `UseWebSockets()` and inserts a terminal-ish `Use` branch that accepts the WebSocket at the internal constant `ReloadPath` (`"/__pennington/reload"`) and hands it to `LiveReloadServer.HandleAsync`. |

## SPA navigation — `Pennington.Islands.SpaNavigationExtensions`

Defined in `src/Pennington/Islands/SpaNavigationExtensions.cs`.

| Method | Full signature | Purpose |
|---|---|---|
| `AddSpaNavigation` | `public static IServiceCollection AddSpaNavigation(this IServiceCollection services, Action<SpaNavigationOptions>? configure = null)` | Registers `SpaNavigationOptions`, transient `SpaPageDataService`, transient `IContentService` → `SpaNavigationContentService`, and a default singleton `RenderContext` derived from `PenningtonOptions.CanonicalBaseUrl` and `SiteTitle`. |
| `UseSpaNavigation` | `public static IEndpointRouteBuilder UseSpaNavigation(this IEndpointRouteBuilder app)` | Maps `GET /{SpaNavigationOptions.DataPath}/{*slug}` (default `/_spa-data/{*slug}`) which strips an optional `.json` suffix, resolves the page title from registered `IContentService`s, calls `SpaPageDataService.GetPageDataAsync`, resolves xrefs in each island's HTML via `XrefResolvingService`, attaches per-request diagnostics, and returns the `SpaEnvelope` as `application/json`. |

## File-watched services — `Pennington.Infrastructure.FileWatchedServiceExtensions`

Defined in `src/Pennington/Infrastructure/FileWatchedServiceExtensions.cs`.

| Method | Full signature | Purpose |
|---|---|---|
| `AddFileWatched<T>` | `public static IServiceCollection AddFileWatched<T>(this IServiceCollection services) where T : class` | Registers a singleton `FileWatchDependencyFactory<T>` and a transient `T` whose instance is pulled from the factory. The factory rebuilds `T` via `ActivatorUtilities.CreateInstance` on every `IFileWatcher` change notification, so callers always resolve a fresh instance. |
| `AddFileWatched<TService, TImplementation>` | `public static IServiceCollection AddFileWatched<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService` | Same as above but separates the abstraction from the implementation: the singleton factory is keyed on `TImplementation`, and the transient `TService` returns the current `TImplementation` instance. |

## Doc site — `Pennington.DocSite.DocSiteServiceExtensions`

Defined in `src/Pennington.DocSite/DocSiteServiceExtensions.cs`.

| Method | Full signature | Purpose |
|---|---|---|
| `AddDocSite` | `public static IServiceCollection AddDocSite(this IServiceCollection services, Func<DocSiteOptions> configureOptions)` | Registers `DocSiteOptions`, calls `AddRazorComponents`, composes `AddPennington` (wiring `DocSiteFrontMatter` markdown content, llms.txt with `#main-content` selector, default search selector, localization, and routing assemblies), composes `AddMonorailCss` with `DocSiteOptions.ColorScheme`/`ExtraStyles`, calls `AddSpaNavigation`, registers scoped `ComponentRenderer`, transient `DocSiteArticleSlotRenderer : IIslandRenderer`, and transient `ContentResolver`. |
| `UseDocSite` | `public static WebApplication UseDocSite(this WebApplication app)` | Calls `UsePenningtonLocaleRouting`, `UseAntiforgery`, `UseStaticFiles`, `MapRazorComponents<App>().AddAdditionalAssemblies(DocSiteOptions.AdditionalRoutingAssemblies)`, `UseMonorailCss`, `UseSpaNavigation`, and `UsePennington`. |
| `RunDocSiteAsync` | `public static Task RunDocSiteAsync(this WebApplication app, string[] args)` | Thin delegate to `RunOrBuildAsync(args)`. |

## Blog site — `Pennington.BlogSite.BlogSiteServiceExtensions`

Defined in `src/Pennington.BlogSite/BlogSiteServiceExtensions.cs`.

| Method | Full signature | Purpose |
|---|---|---|
| `AddBlogSite` | `public static IServiceCollection AddBlogSite(this IServiceCollection services, Func<BlogSiteOptions> configureOptions)` | Registers `BlogSiteOptions`, calls `AddRazorComponents`, composes `AddPennington` (wiring `BlogSiteFrontMatter` markdown content from `ContentRootPath/BlogContentPath` at `BlogBaseUrl`, plus the `Pennington.BlogSite` assembly and any `AdditionalRoutingAssemblies` for Razor page discovery), composes `AddMonorailCss` with `BlogSiteOptions.ColorScheme`/`ExtraStyles`, and registers `BlogContentResolver` and `BlogSiteContentService` as file-watched services (the latter also as an `IContentService`). |
| `UseBlogSite` | `public static WebApplication UseBlogSite(this WebApplication app)` | Calls `UseAntiforgery`, `UseStaticFiles`, `MapRazorComponents<App>().AddAdditionalAssemblies(BlogSiteOptions.AdditionalRoutingAssemblies)`, `UseMonorailCss`, `UsePennington`, and — when `BlogSiteOptions.EnableRss` is true — maps `GET /rss.xml` to `BlogSiteContentService.GetRssXmlAsync`. |
| `RunBlogSiteAsync` | `public static Task RunBlogSiteAsync(this WebApplication app, string[] args)` | Thin delegate to `RunOrBuildAsync(args)`. |

## MonorailCSS — `Pennington.MonorailCss.MonorailServiceExtensions`

Defined in `src/Pennington.MonorailCss/MonorailServiceExtensions.cs`.

| Method | Full signature | Purpose |
|---|---|---|
| `AddMonorailCss` | `public static IServiceCollection AddMonorailCss(this IServiceCollection services, Func<IServiceProvider, MonorailCssOptions>? optionFactory = null)` | Registers `MonorailCssOptions` (singleton default, or transient via `optionFactory`), singleton `CssClassCollector`, transient `MonorailCssService`, and `CssClassCollectorProcessor` as `IResponseProcessor`. |
| `UseMonorailCss` | `public static WebApplication UseMonorailCss(this WebApplication app, string path = "/styles.css")` | Throws `InvalidOperationException` if `MonorailCssService` or `CssClassCollector` is not registered. Scans every file in `MonorailCssOptions.ContentPaths` against `app.Environment.WebRootFileProvider` for potential CSS classes, then maps `GET {path}` (default `/styles.css`) returning `MonorailCssService.GetStyleSheet()` as `text/css`. |

## Roslyn — `Pennington.Roslyn.RoslynExtensions`

Defined in `src/Pennington.Roslyn/RoslynExtensions.cs`.

| Method | Full signature | Purpose |
|---|---|---|
| `AddPenningtonRoslyn` | `public static IServiceCollection AddPenningtonRoslyn(this IServiceCollection services, Action<RoslynOptions>? configure = null)` | Always registers `RoslynOptions`, singleton `SyntaxHighlighter`, and `RoslynHighlighter : ICodeHighlighter`. When `RoslynOptions.SolutionPath` is non-empty, additionally registers singleton `ISolutionWorkspaceService` → `SolutionWorkspaceService`, singleton `ISymbolExtractionService` → `SymbolExtractionService` (wired back into the workspace service), and `RoslynCodeBlockPreprocessor : ICodeBlockPreprocessor` (xmldocid fence support). |

## Example

```cs:path
examples/AlexBlogExample/Program.cs
```

The minimal blog-site composition — `AddBlogSite` → `UseBlogSite` → `RunBlogSiteAsync` — demonstrates the typical shape of every extension-method pair in this page.

## See also

- Reference: [`PenningtonOptions`](/reference/options/pennington-options)
- Reference: [`DocSiteOptions`](/reference/options/doc-site-options)
- Reference: [`BlogSiteOptions`](/reference/options/blog-site-options)
- Background: [Unified dev-serve and static-build pipeline](/explanation/unified-build-pipeline)
