---
title: "DI and middleware extension methods"
description: "At-a-glance index of every IServiceCollection and WebApplication extension method Pennington ships, grouped by package, each pointing to the relevant options reference page."
section: "host"
order: 10
tags: []
uid: reference.host.extensions
isDraft: true
search: false
llms: false
---

> **In this page.** Every `IServiceCollection` and `WebApplication` extension method shipped by Pennington, grouped by package, with one-line purposes and links to the relevant options or reference pages.
>
> **Not in this page.** The underlying services these extensions wire up — see the individual reference pages for `PenningtonOptions`, `DocSiteOptions`, `BlogSiteOptions`, `MonorailCssOptions`, `RoslynOptions`, and the content pipeline.

## Summary

The public DI and middleware surface area is composed of static extension methods on `IServiceCollection` and `WebApplication`, grouped by product package.
They live under `Pennington.Infrastructure`, `Pennington.Islands`, `Pennington.DocSite`, `Pennington.BlogSite`, `Pennington.MonorailCss`, and `Pennington.Roslyn`.

## Core — `Pennington.Infrastructure.PenningtonExtensions`

Defined in `src/Pennington/Infrastructure/PenningtonExtensions.cs`.

| Method | Signature | Purpose |
|---|---|---|
| `AddPennington` | `AddPennington(this IServiceCollection services, Action<PenningtonOptions> configure)` | Registers core Pennington services (pipeline, highlighters, content services, xref, diagnostics, locale, feeds, search, llms.txt, output generation). See [`PenningtonOptions`](/reference/options/pennington-options). |
| `UsePennington` | `UsePennington(this WebApplication app)` | Wires the middleware pipeline — static files, locale routing, live reload, response processing, feed/sitemap/search/llms.txt endpoints. |
| `UsePenningtonLocaleRouting` | `UsePenningtonLocaleRouting(this WebApplication app)` | Registers locale detection, request culture providers, and routing (idempotent; no-op when not multi-locale). See [`LocalizationOptions`](/reference/options/localization-options). |
| `RunOrBuildAsync` | `RunOrBuildAsync(this WebApplication app, string[] args)` | Dispatches on `args[0]`: `"build"` runs `OutputGenerationService` then exits; anything else falls through to `app.RunAsync()`. See [Dev mode and build mode share one code path](/explanation/core/dev-vs-build). |

## Live reload — `Pennington.Infrastructure.LiveReloadExtensions`

Defined in `src/Pennington/Infrastructure/LiveReloadServer.cs`.

| Method | Signature | Purpose |
|---|---|---|
| `UsePenningtonLiveReload` | `UsePenningtonLiveReload(this WebApplication app)` | Mounts the `/__pennington/reload` WebSocket endpoint when `DOTNET_WATCH` is set; otherwise a no-op. |

## SPA navigation — `Pennington.Islands.SpaNavigationExtensions`

Defined in `src/Pennington/Islands/SpaNavigationExtensions.cs`.

| Method | Signature | Purpose |
|---|---|---|
| `AddSpaNavigation` | `AddSpaNavigation(this IServiceCollection services, Action<SpaNavigationOptions>? configure = null)` | Registers the SPA data service and content service for island-based client navigation. See [Island rendering interfaces](/reference/extension-points/islands). |
| `UseSpaNavigation` | `UseSpaNavigation(this IEndpointRouteBuilder app)` | Maps the SPA page-data endpoint (default `/_spa-data/{*slug}`) returning a `SpaEnvelope`. |

## File-watched services — `Pennington.Infrastructure.FileWatchedServiceExtensions`

Defined in `src/Pennington/Infrastructure/FileWatchedServiceExtensions.cs`.

| Method | Signature | Purpose |
|---|---|---|
| `AddFileWatched<T>` | `AddFileWatched<T>(this IServiceCollection services) where T : class` | Registers a transient `T` rebuilt on every `IFileWatcher` change. |
| `AddFileWatched<TService, TImplementation>` | `AddFileWatched<TService, TImplementation>(this IServiceCollection services)` | As above with separate abstraction and implementation types. |

## Doc site — `Pennington.DocSite.DocSiteServiceExtensions`

Defined in `src/Pennington.DocSite/DocSiteServiceExtensions.cs`.

| Method | Signature | Purpose |
|---|---|---|
| `AddDocSite` | `AddDocSite(this IServiceCollection services, Func<DocSiteOptions> configureOptions)` | Composes `AddPennington` + `AddMonorailCss` + `AddSpaNavigation` around a DocSite. See [`DocSiteOptions`](/reference/options/docsite-options). |
| `UseDocSite` | `UseDocSite(this WebApplication app)` | Wires locale routing, static files, Razor components, MonorailCSS, SPA navigation, and `UsePennington`. |
| `RunDocSiteAsync` | `RunDocSiteAsync(this WebApplication app, string[] args)` | Thin delegate to `RunOrBuildAsync(args)`. |

## Blog site — `Pennington.BlogSite.BlogSiteServiceExtensions`

Defined in `src/Pennington.BlogSite/BlogSiteServiceExtensions.cs`.

| Method | Signature | Purpose |
|---|---|---|
| `AddBlogSite` | `AddBlogSite(this IServiceCollection services, Func<BlogSiteOptions> configureOptions)` | Composes `AddPennington` + `AddMonorailCss` around a BlogSite with `BlogSiteFrontMatter` markdown content. See [`BlogSiteOptions`](/reference/options/blogsite-options). |
| `UseBlogSite` | `UseBlogSite(this WebApplication app)` | Wires static files, Razor components, MonorailCSS, `UsePennington`, and (when enabled) the RSS endpoint. |
| `RunBlogSiteAsync` | `RunBlogSiteAsync(this WebApplication app, string[] args)` | Thin delegate to `RunOrBuildAsync(args)`. |

## MonorailCSS — `Pennington.MonorailCss.MonorailServiceExtensions`

Defined in `src/Pennington.MonorailCss/MonorailServiceExtensions.cs`.

| Method | Signature | Purpose |
|---|---|---|
| `AddMonorailCss` | `AddMonorailCss(this IServiceCollection services, Func<IServiceProvider, MonorailCssOptions>? optionFactory = null)` | Registers MonorailCSS services and the CSS-class response processor. See [`MonorailCssOptions`](/reference/options/monorail-css-options). |
| `UseMonorailCss` | `UseMonorailCss(this WebApplication app, string path = "/styles.css")` | Maps the stylesheet endpoint (default `/styles.css`). |

## Roslyn — `Pennington.Roslyn.RoslynExtensions`

Defined in `src/Pennington.Roslyn/RoslynExtensions.cs`.

| Method | Signature | Purpose |
|---|---|---|
| `AddPenningtonRoslyn` | `AddPenningtonRoslyn(this IServiceCollection services, Action<RoslynOptions>? configure = null)` | Registers the Roslyn-based highlighter and (when `SolutionPath` is set) the workspace, symbol-extraction, and xmldocid fence services. See [`RoslynOptions`](/reference/options/roslyn-options). |

## Example

```cs:path
examples/AlexBlogExample/Program.cs
```

The minimal blog-site composition — `AddBlogSite` → `UseBlogSite` → `RunBlogSiteAsync` — demonstrates the typical shape of every extension-method pair in this page.

## See also

- Reference: [`PenningtonOptions`](/reference/options/pennington-options)
- Reference: [`DocSiteOptions`](/reference/options/docsite-options)
- Reference: [`BlogSiteOptions`](/reference/options/blogsite-options)
- Background: [Dev mode and build mode share one code path](/explanation/core/dev-vs-build)
