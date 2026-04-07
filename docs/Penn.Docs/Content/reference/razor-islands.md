---
title: "Razor Islands"
description: "Reference for the Razor Island architecture — IIslandRenderer, RazorIslandRenderer, ComponentRenderer, SPA envelopes, and lifecycle events"
uid: "penn.reference.razor-islands"
order: 4030
---

Penn's Razor Island system turns a static content site into something that navigates like an SPA. Each island is a named region of the page -- "content", "sidebar", "toc" -- that updates independently when the user follows a link. The first page load delivers full static HTML (search engines and users without JavaScript see fully formed content). Subsequent navigations fetch a JSON envelope and swap island contents in place. The surrounding layout never reloads.

## Architecture Overview

```
First load:  Browser  -->  GET /docs/intro  -->  Full HTML (all islands pre-rendered)
Navigation:  Browser  -->  GET /_spa-data/docs/intro.json  -->  SpaEnvelopeDto  -->  Swap islands
```

Four pieces make this work:

1. **`IIslandRenderer`** implementations produce HTML for named regions given a route.
2. **`SpaPageDataService`** coordinates all renderers into a single `SpaEnvelopeDto`.
3. **`ComponentRenderer`** renders Blazor components to static HTML strings on the server.
4. **The client-side SPA engine** (`spa-engine.js`) intercepts link clicks, fetches envelopes, and swaps DOM content.

For a guided walkthrough of adding SPA navigation to a site, see [Adding SPA Navigation](xref:penn.guides.adding-spa-navigation). For a deeper look at how the JavaScript engine handles prefetching, view transitions, and scroll restoration, see [SPA Island Architecture](xref:penn.under-the-hood.spa-island-architecture).

## IIslandRenderer

The core abstraction. Each implementation produces HTML for one named region of the page.

```csharp:path
src/Penn/Islands/IIslandRenderer.cs
```

| Member | Type | Description |
|--------|------|-------------|
| `IslandName` | `string` | Identifier that must match a `data-spa-island` attribute in the layout HTML. |
| `RenderAsync` | `Task<string>` | Receives the target `ContentRoute` and a `RenderContext`. Returns an HTML string, or an empty string to omit this island from the envelope. |

## ContentRoute and RenderContext

### ContentRoute

```csharp:path
src/Penn/Routing/ContentRoute.cs
```

| Property | Type | Description |
|----------|------|-------------|
| `CanonicalPath` | `UrlPath` | The URL path for the page, e.g. `/docs/getting-started`. |
| `OutputFile` | `FilePath` | The output file path for static generation, e.g. `docs/getting-started/index.html`. |
| `SourceFile` | `FilePath?` | The source file that produced this content, if known. |
| `Locale` | `string` | Locale code for localized content. Empty string for the default locale. |

Helper methods: `WithBaseUrl(UrlPath)` prepends a base URL, `AbsoluteUrl(UrlPath)` produces a full canonical URL, and `IsDefaultLocale` returns `true` when `Locale` is empty.

### RenderContext

```csharp:path
src/Penn/Islands/RenderContext.cs
```

| Property | Type | Description |
|----------|------|-------------|
| `BaseUrl` | `UrlPath` | The site's base URL, from `PennOptions.CanonicalBaseUrl`. |
| `SiteTitle` | `string` | The site title, from `PennOptions.SiteTitle`. |
| `Locale` | `string?` | The locale for this render pass, or `null`. |

## RazorIslandRenderer&lt;TComponent&gt;

Abstract base class for the common case: rendering a Blazor component to an HTML string. You provide the island name and build a parameter dictionary; the base class handles the `HtmlRenderer` ceremony.

```csharp:path
src/Penn/Islands/RazorIslandRenderer.cs
```

### BuildParametersAsync

The single abstract method you implement. Return a parameter dictionary or `null` to skip:

- A `Dictionary<string, object?>` of component parameters. Keys should use `nameof(Component.Property)` for refactor safety.
- `null` to skip this island for that route. `RenderAsync` returns an empty string and the island is omitted from the envelope.

### Example: DocSite's Article Island

Here is how `Penn.DocSite` implements its article content island:

```csharp
internal class DocSiteArticleSlotRenderer(
    ContentResolver contentResolver,
    ComponentRenderer renderer) : RazorIslandRenderer<DocSiteArticle>(renderer)
{
    public override string IslandName => "content";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(ContentRoute route)
    {
        var url = route.CanonicalPath.Value;
        var resolved = await contentResolver.GetContentByUrlAsync(url);
        if (resolved is null) return null;

        var navInfo = await contentResolver.GetNavigationInfoAsync(url);

        return new Dictionary<string, object?>
        {
            [nameof(DocSiteArticle.Title)] = resolved.Title,
            [nameof(DocSiteArticle.HtmlContent)] = resolved.Html,
            [nameof(DocSiteArticle.PreviousPageName)] = navInfo?.PreviousPage?.Title,
            [nameof(DocSiteArticle.PreviousPageHref)] = navInfo?.PreviousPage?.Route.CanonicalPath.Value,
            [nameof(DocSiteArticle.NextPageName)] = navInfo?.NextPage?.Title,
            [nameof(DocSiteArticle.NextPageHref)] = navInfo?.NextPage?.Route.CanonicalPath.Value,
        };
    }
}
```

> [!NOTE]
> Components rendered via `RazorIslandRenderer` support `@inject` for DI services but cannot use JavaScript interop, `NavigationManager`, or other browser-dependent APIs. These are server-side static renders -- there is no browser context.

## ComponentRenderer

Wraps Blazor's `HtmlRenderer` to render any `IComponent` to an HTML string. Registered as **Scoped** -- one instance per request, disposed at scope end.

```csharp:path
src/Penn/Islands/ComponentRenderer.cs
```

| Member | Description |
|--------|-------------|
| `RenderComponentAsync<TComponent>` | Renders a component with optional parameters and returns the HTML string. Dispatches through `HtmlRenderer.Dispatcher` to satisfy Blazor's threading requirements. |
| `DisposeAsync` | Disposes the underlying `HtmlRenderer`. Called automatically at scope end. |

`RazorIslandRenderer<TComponent>` calls `ComponentRenderer` internally. If you need to render a component outside the island system -- for an email template or RSS feed entry -- inject it directly:

```csharp
public class EmailService(ComponentRenderer renderer)
{
    public async Task<string> RenderEmailAsync(string subject, string body)
    {
        return await renderer.RenderComponentAsync<EmailTemplate>(
            new Dictionary<string, object?>
            {
                ["Subject"] = subject,
                ["Body"] = body
            });
    }
}
```

## SpaPageDataService

Coordinates all registered `IIslandRenderer` instances for a given route and produces the JSON envelope.

```csharp:path
src/Penn/Islands/SpaPageDataService.cs
```

`GetPageDataAsync` iterates every registered renderer in registration order. Renderers that return empty strings are omitted from the envelope. If no renderer produces content, the method returns `null` and the SPA endpoint returns 404.

## SpaEnvelopeDto

The JSON payload returned for each page during SPA navigation. The SPA engine uses this to update the page title, meta tags, and island contents.

```json
{
  "title": "Front Matter Properties",
  "description": "Reference for the capability-based front matter system",
  "islands": {
    "content": "<article>...</article>",
    "toc": "<nav>...</nav>"
  }
}
```

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `title` | `string` | No | Page title. The SPA engine sets `document.title` to `"{SiteTitle} - {title}"`. |
| `description` | `string?` | Yes | Page description. Updates `<meta name="description">` and Open Graph tags. |
| `islands` | `object` | No | Map of island name to HTML string. Keys correspond to `data-spa-island` attributes in the layout. |
| `diagnostics` | `array?` | Yes | Development-only. Diagnostic messages from the render pass, surfaced in the dev overlay. Omitted from production responses via `JsonIgnoreCondition.WhenWritingNull`. |

Serialized by `SpaEnvelopeSerializer` using `System.Text.Json` with camelCase naming and null-omission.

## Registration

### AddSpaNavigation

Call `AddSpaNavigation` on `IServiceCollection` to register the SPA infrastructure:

```csharp
builder.Services.AddSpaNavigation();
```

This registers:

| Service | Lifetime | Purpose |
|---------|----------|---------|
| `SpaNavigationOptions` | Singleton | Configuration (data path prefix) |
| `SpaPageDataService` | Transient | Coordinates island renderers into envelopes |
| `SpaNavigationContentService` | Transient | `IContentService` that generates `/_spa-data/*.json` routes for static builds |
| `RenderContext` | Singleton | Ambient site context for renderers |

`AddSpaNavigation` accepts an optional configuration delegate:

```csharp
builder.Services.AddSpaNavigation(options =>
{
    options.DataPath = "/_api/pages";  // default: "/_spa-data"
});
```

### UseSpaNavigation

Call `UseSpaNavigation` on the endpoint route builder to map the SPA data endpoint:

```csharp
app.UseSpaNavigation();
```

This maps a `GET /{DataPath}/{*slug}` endpoint that:

1. Converts the slug to a URL via `SpaSlug.ToUrl`.
2. Constructs a `ContentRoute` for the target page.
3. Resolves the page title from registered `IContentService` instances.
4. Calls `SpaPageDataService.GetPageDataAsync` to render all islands.
5. Resolves `xref:` links in the rendered HTML.
6. Returns the serialized `SpaEnvelopeDto` as `application/json`.

`ComponentRenderer` must be registered as **Scoped** because the underlying `HtmlRenderer` is stateful and must be disposed after each request. Island renderers are typically **Transient**.

## Data Attributes

The client-side engine discovers islands by querying `[data-spa-island]` elements in the DOM.

### data-spa-island

Marks an element as an island. The value must match an `IIslandRenderer.IslandName`.

```html
<article data-spa-island="content">
    @Body
</article>

<aside data-spa-island="toc">
    @* Table of contents rendered here *@
</aside>
```

### data-spa-loading

Controls what the user sees while the SPA engine fetches new content.

| Value | Behaviour |
|-------|-----------|
| `"skeleton"` | Shows a shimmer placeholder. Uses a custom `<template>` if one exists, otherwise falls back to a built-in shimmer animation. |
| `"clear"` | Empties the island immediately, showing blank space until content arrives. |
| `"keep"` | Leaves the previous content in place until new data arrives. This is the default. |

```html
<article data-spa-island="content" data-spa-loading="skeleton">
    @Body
</article>
```

The skeleton is shown only when the fetch takes longer than the configurable threshold (default: 100ms). Fast navigations -- including prefetched pages -- skip the skeleton entirely.

### data-spa-skeleton-for

Provide a custom skeleton template for a specific island. The `<template>` element's `data-spa-skeleton-for` value must match the island name.

```html
<template data-spa-skeleton-for="content">
    <div class="animate-pulse space-y-4">
        <div class="h-8 bg-base-200 rounded w-3/4"></div>
        <div class="h-4 bg-base-200 rounded w-full"></div>
        <div class="h-4 bg-base-200 rounded w-5/6"></div>
    </div>
</template>
```

### Configuration Attributes

The SPA engine reads additional configuration from `data-*` attributes on the `<html>` and `<body>` elements.

| Attribute | Element | Default | Purpose |
|-----------|---------|---------|---------|
| `data-base-url` | `<body>` | `""` | Base URL prefix for subdirectory deployments |
| `data-spa-data-path` | `<html>` | `"/_spa-data"` | URL prefix for page data JSON files |
| `data-spa-skeleton-delay` | `<html>` | `"100"` | Milliseconds to wait before showing a skeleton (fast fetches skip it) |
| `data-spa-min-skeleton` | `<html>` | `"250"` | Minimum milliseconds to display the skeleton once it becomes visible |

## Lifecycle Events

The SPA engine dispatches custom events on `document` at two points during navigation.

### spa:before-navigate

Fires before the fetch starts. The previous page content is still in the DOM.

```javascript
document.addEventListener('spa:before-navigate', (e) => {
    const { url, slug } = e.detail;
    // Close modals, dismiss tooltips, cancel pending animations
});
```

| Detail field | Type | Description |
|-------------|------|-------------|
| `url` | `URL` | The target URL |
| `slug` | `string` | The SPA data slug (e.g. `"docs/intro"` or `"index"`) |

### spa:commit

Fires after new island content has been injected into the DOM.

```javascript
document.addEventListener('spa:commit', (e) => {
    const { url, slug, data } = e.detail;
    // Re-highlight code blocks, rebuild table of contents,
    // update active navigation state, reinitialize copy buttons
});
```

| Detail field | Type | Description |
|-------------|------|-------------|
| `url` | `URL` | The navigated URL |
| `slug` | `string` | The SPA data slug |
| `data` | `object` | The full deserialized `SpaEnvelopeDto` (title, description, islands) |

## Creating Custom Islands

### Minimal: Implement IIslandRenderer Directly

For islands that don't need a Razor component -- raw HTML, or content fetched from a service:

```csharp
public class BreadcrumbIslandRenderer(
    IContentService contentService) : IIslandRenderer
{
    public string IslandName => "breadcrumbs";

    public async Task<string> RenderAsync(ContentRoute route, RenderContext context)
    {
        var entries = await contentService.GetContentTocEntriesAsync();
        var current = entries.FirstOrDefault(
            e => e.Route.CanonicalPath.Matches(route.CanonicalPath));

        if (current is null) return "";

        return $"""<nav aria-label="Breadcrumb"><a href="/">Home</a> / {current.Title}</nav>""";
    }
}
```

### Razor Component Path: RazorIslandRenderer&lt;TComponent&gt;

For islands backed by a Razor component with parameters:

```csharp
public class SearchIslandRenderer(
    SearchService search,
    ComponentRenderer renderer) : RazorIslandRenderer<SearchPanel>(renderer)
{
    public override string IslandName => "search";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(ContentRoute route)
    {
        var results = await search.GetSuggestionsForAsync(route.CanonicalPath.Value);
        return new Dictionary<string, object?>
        {
            [nameof(SearchPanel.Suggestions)] = results
        };
    }
}
```

Register the renderers and mark the layout:

```csharp
builder.Services.AddSpaNavigation();
builder.Services.AddScoped<ComponentRenderer>();
builder.Services.AddTransient<IIslandRenderer, BreadcrumbIslandRenderer>();
builder.Services.AddTransient<IIslandRenderer, SearchIslandRenderer>();
```

```html
<!-- Layout.razor -->
<nav data-spa-island="breadcrumbs" data-spa-loading="keep">
    <BreadcrumbComponent />
</nav>

<div data-spa-island="search" data-spa-loading="clear">
    <SearchPanel />
</div>
```

Include the SPA engine script in your `App.razor` or layout:

```html
<script src="/_content/Penn.UI/spa-engine.js" defer></script>
```

The engine activates automatically -- no initialization call needed.
