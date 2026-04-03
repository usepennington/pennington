---
title: "Razor Islands"
description: "Reference for the Razor Island system — interfaces, data attributes, JSON envelope, and lifecycle events"
uid: "docs.reference.razor-islands"
order: 4030
---

Razor Islands add SPA navigation to static content sites. Each island is a named region of the page that
updates independently when the user navigates, while the surrounding layout stays in place. The first page
load is always full static HTML; subsequent navigations fetch a JSON envelope and swap island contents.

## Registration

```csharp
services.AddContentEngineService(...)
    .WithMarkdownContentService<TFrontMatter>(...)
    .WithSpaNavigation<TFrontMatter>(spa =>
    {
        spa.AddIsland<MyContentIslandRenderer>();
        spa.AddIsland<MySidebarIslandRenderer>();
    });

// In the middleware pipeline:
app.UseSpaNavigation();
```

`WithSpaNavigation<TFrontMatter>()` registers:

| Service | Lifetime | Purpose |
|---------|----------|---------|
| `SpaPageDataService` | Transient | Resolves metadata from `PageToGenerate.Metadata` across all content services and orchestrates island renderers into a `SpaPageEnvelope` |
| `ComponentRenderer` | Scoped | Wraps Blazor's `HtmlRenderer` for Razor-based renderers |
| `SpaNavigationContentService` | Transient | `IContentService` that generates `_spa-data/*.json` for all registered content services during static builds |

`UseSpaNavigation()` maps the endpoint that serves the JSON envelope for each page.

## ISpaIslandRenderer

The core interface for producing HTML content for a named island.

```csharp
public interface ISpaIslandRenderer
{
    string IslandName { get; }
    Task<string?> RenderAsync(string url);
}
```

| Member | Description |
|--------|-------------|
| `IslandName` | Must match a `data-spa-island` attribute in the layout. Multiple renderers for the same name are allowed — only the last registered one runs. |
| `RenderAsync` | Receives the content page URL (e.g. `"/"`, `"/pasta-carbonara"`). Return `null` to omit this island from the envelope. |

## RazorIslandRenderer&lt;TComponent&gt;

Abstract base class that renders a Blazor component to a string via `ComponentRenderer`. Subclasses provide
the island name and build a parameter dictionary; the base class handles the `HtmlRenderer` ceremony.

```csharp
public abstract class RazorIslandRenderer<TComponent>(
    ComponentRenderer renderer) : ISpaIslandRenderer
    where TComponent : IComponent
{
    public abstract string IslandName { get; }
    protected abstract Task<IDictionary<string, object?>?> BuildParametersAsync(string url);
}
```

`BuildParametersAsync` returns `null` to skip the island for that page, or a dictionary of component
parameters keyed by `nameof(Component.Property)`.

> [!NOTE]
> Components rendered via `RazorIslandRenderer` support `@inject` for DI services but cannot use
> JavaScript interop, `NavigationManager`, or other browser-dependent APIs.

## Page Metadata

The `title` and `description` fields in the JSON envelope are resolved automatically from
`PageToGenerate.Metadata` across all registered `IContentService` instances. Any content service
that populates `Metadata.Title` on its pages will participate in SPA navigation — no additional
registration is needed.

## SpaPageEnvelope

The JSON payload returned for each page.

```json
{
  "title": "Pasta Carbonara",
  "description": "Classic Roman carbonara with guanciale and pecorino",
  "islands": {
    "content": "<header>…</header><div class=\"prose\">…</div>",
    "recipe-info": "<div class=\"rounded-lg\">…</div>"
  }
}
```

| Field | Type | Description |
|-------|------|-------------|
| `title` | `string` | Page title — used by the SPA engine to set `document.title` |
| `description` | `string?` | Page description — updates `<meta name="description">` |
| `islands` | `object` | Map of island name → HTML string. Keys match `data-spa-island` attributes. |

## Data Attributes

Add these to layout elements to make them Razor Islands.

### data-spa-island

Marks an element as an island. The value is the island name.

```html
<article data-spa-island="content">
    @Body
</article>
```

### data-spa-loading

Controls what happens to the island while a slow fetch is in progress.

| Value | Behaviour |
|-------|-----------|
| `"skeleton"` | Shows a shimmer placeholder (or a custom `<template>`, see below) |
| `"clear"` | Empties the island immediately |
| `"keep"` | Leaves previous content in place until new data arrives (default) |

```html
<article data-spa-island="content" data-spa-loading="skeleton">
```

### Custom Skeleton Templates

Provide a `<template>` element with `data-spa-skeleton-for` matching the island name:

```html
<template data-spa-skeleton-for="content">
    <div class="animate-pulse space-y-4">
        <div class="h-8 bg-neutral-200 rounded w-3/4"></div>
        <div class="h-4 bg-neutral-200 rounded w-full"></div>
        <div class="h-4 bg-neutral-200 rounded w-5/6"></div>
    </div>
</template>
```

If no custom template exists, the engine shows a default shimmer skeleton.

## Configuration via &lt;html&gt; Attributes

The SPA engine reads configuration from `data-*` attributes on the `<html>` element.

| Attribute | Default | Purpose |
|-----------|---------|---------|
| `data-base-url` (on `<body>`) | `""` | Base URL prefix for subdirectory deployments (set automatically by `BaseUrlRewritingMiddleware`) |
| `data-spa-data-path` | `"/_spa-data"` | URL prefix for page data JSON files |
| `data-spa-skeleton-delay` | `"100"` | Milliseconds before showing the skeleton (fast fetches skip it) |
| `data-spa-min-skeleton` | `"250"` | Minimum milliseconds to show the skeleton once visible |

## Lifecycle Events

The SPA engine dispatches custom events on `document`.

### spa:before-navigate

Fires before the fetch starts. Use it to clear transient UI state.

```javascript
document.addEventListener('spa:before-navigate', (e) => {
    const { url } = e.detail;
    // Clear tooltips, close modals, etc.
});
```

### spa:commit

Fires after new island content is injected into the DOM. Use it to reinitialise interactive features.

```javascript
document.addEventListener('spa:commit', (e) => {
    const { url, data } = e.detail;
    // Re-highlight code, rebuild outline, update active nav links, etc.
});
```

| Detail field | Type | Description |
|-------------|------|-------------|
| `url` | `URL` | The navigated URL |
| `data` | `object` | The full `SpaPageEnvelope` (title, description, islands) |

## SpaNavigationBuilder

Fluent builder passed to the `WithSpaNavigation<T>()` configure callback.

| Method | Description |
|--------|-------------|
| `AddIsland<TRenderer>()` | Registers an `ISpaIslandRenderer` implementation |
| `WithDataPath(string)` | Changes the data endpoint path (default `"/_spa-data"`) |

## Scripts

Include in your `App.razor` after `scripts.js`:

```html
<script src="/_content/MyLittleContentEngine.UI/spa-engine.js" defer></script>
```

The engine activates automatically — no initialisation call needed. It discovers islands from
`data-spa-island` attributes, intercepts same-origin link clicks, and handles history, view transitions,
and scroll position.
