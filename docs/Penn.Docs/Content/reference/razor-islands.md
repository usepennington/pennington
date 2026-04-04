---
title: "Razor Islands"
description: "Reference for the Razor Island architecture — IIslandRenderer, RazorIslandRenderer, ComponentRenderer, SPA envelopes, and lifecycle events"
uid: "penn.reference.razor-islands"
order: 4030
---

Razor Islands turn a static content site into something that feels like an SPA without actually being one. Each island is a named region of the page — "content", "sidebar", "toc", whatever you like — that updates independently when the user navigates. The first page load is full static HTML (search engines love this). Subsequent navigations fetch a JSON envelope and swap island contents in place. The surrounding layout never reloads.

It's not a full SPA framework. It's just enough to make navigation fast. Penn is, as always, sufficient to the purpose.

## Architecture Overview

```
First load:  Browser → Full HTML page (all islands pre-rendered)
Navigation:  Browser → GET /_spa-data/page.json → SpaEnvelopeDto → Swap islands
```

The flow involves four pieces:

1. **`IIslandRenderer`** — Produces HTML for a named island given a route
2. **`SpaPageDataService`** — Coordinates all renderers into a `SpaEnvelopeDto`
3. **`ComponentRenderer`** — Renders Blazor components to HTML strings (server-side)
4. **Client-side SPA engine** — Intercepts link clicks, fetches envelopes, swaps DOM

## IIslandRenderer

The core interface. Implement this to produce HTML for a named region of your page.

```csharp:path
src/Penn/Islands/IIslandRenderer.cs
```

| Member | Description |
|--------|-------------|
| `IslandName` | Must match a `data-spa-island` attribute in the layout. |
| `RenderAsync` | Receives the `ContentRoute` being navigated to and a `RenderContext`. Return an empty string to omit this island from the envelope. |

### ContentRoute

The route carries everything a renderer needs to resolve content:

```csharp:path
src/Penn/Routing/ContentRoute.cs
```

Key properties: `CanonicalPath` (e.g. `/docs/getting-started`), `OutputFile`, `SourceFile`, and `Locale`. The `NavigationPath` property ensures a trailing slash for consistent URL matching.

### RenderContext

```csharp:path
src/Penn/Islands/RenderContext.cs
```

Ambient context for the render pass — the site's base URL, title, and locale. Available to all island renderers during a request.

## RazorIslandRenderer&lt;TComponent&gt;

Abstract base class for the common case: rendering a Blazor component to an HTML string. You provide the island name and build a parameter dictionary; the base class handles the `HtmlRenderer` ceremony.

```csharp:path
src/Penn/Islands/RazorIslandRenderer.cs
```

`BuildParametersAsync` returns `null` to skip the island for that route, or a dictionary of component parameters. Keys should use `nameof(Component.Property)` for refactor safety.

> [!NOTE]
> Components rendered via `RazorIslandRenderer` support `@inject` for DI services but cannot use JavaScript interop, `NavigationManager`, or other browser-dependent APIs. These are server-side renders — there is no browser.

### Example: A Real Island Renderer

Here's how `Penn.DocSite` implements its article content island:

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
            [nameof(DocSiteArticle.PreviousPageHref)] = navInfo?.PreviousPage?.Route.NavigationPath.Value,
            [nameof(DocSiteArticle.NextPageName)] = navInfo?.NextPage?.Title,
            [nameof(DocSiteArticle.NextPageHref)] = navInfo?.NextPage?.Route.NavigationPath.Value,
        };
    }
}
```

The pattern: inject your content resolver, look up the content for the route, return `null` if nothing matches, otherwise build the parameter dictionary.

## ComponentRenderer

Wraps Blazor's `HtmlRenderer` to render any `IComponent` to an HTML string. Registered as Scoped — shared across a request, disposed at scope end.

```csharp:path
src/Penn/Islands/ComponentRenderer.cs
```

You typically won't use `ComponentRenderer` directly. `RazorIslandRenderer<TComponent>` calls it for you. But if you need to render a component outside the island system — say, for an email template or an RSS feed item — you can inject it:

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

Each renderer runs in registration order. Renderers that return empty strings are omitted from the envelope. If no renderer produces content, `GetPageDataAsync` returns `null`.

## SpaEnvelopeDto

The JSON payload returned for each page during SPA navigation.

```json
{
  "title": "Front Matter Properties",
  "description": "Reference for the capability-based front matter system",
  "islands": {
    "content": "<article>…</article>",
    "toc": "<nav>…</nav>"
  }
}
```

| Field | Type | Description |
|-------|------|-------------|
| `title` | `string` | Page title — the SPA engine sets `document.title` |
| `description` | `string?` | Page description — updates `<meta name="description">` |
| `islands` | `object` | Map of island name to HTML string. Keys match `data-spa-island` attributes in the layout. |

## Registration

Island renderers are registered as `IIslandRenderer` implementations in the DI container. You can register them directly or use `PennOptions.Islands`:

```csharp
// Option 1: Direct DI registration
services.AddTransient<IIslandRenderer, MyContentIslandRenderer>();
services.AddTransient<IIslandRenderer, MySidebarIslandRenderer>();

// Option 2: Via PennOptions
services.AddPenn(options =>
{
    options.Islands.Register<MyContentIslandRenderer>("content");
    options.Islands.Register<MySidebarIslandRenderer>("sidebar");
});
```

Don't forget to register `ComponentRenderer` as Scoped if you're using `RazorIslandRenderer`:

```csharp
services.AddScoped<ComponentRenderer>();
```

## Data Attributes

Add these to layout elements to wire them up as islands.

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
| `"skeleton"` | Shows a shimmer placeholder (or a custom template) |
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
        <div class="h-8 bg-base-200 rounded w-3/4"></div>
        <div class="h-4 bg-base-200 rounded w-full"></div>
        <div class="h-4 bg-base-200 rounded w-5/6"></div>
    </div>
</template>
```

## Configuration via &lt;html&gt; Attributes

The client-side SPA engine reads configuration from `data-*` attributes on the `<html>` element.

| Attribute | Default | Purpose |
|-----------|---------|---------|
| `data-base-url` (on `<body>`) | `""` | Base URL prefix for subdirectory deployments |
| `data-spa-data-path` | `"/_spa-data"` | URL prefix for page data JSON files |
| `data-spa-skeleton-delay` | `"100"` | Milliseconds before showing skeleton (fast fetches skip it) |
| `data-spa-min-skeleton` | `"250"` | Minimum milliseconds to show skeleton once visible |

## Lifecycle Events

The SPA engine dispatches custom events on `document`.

### spa:before-navigate

Fires before the fetch starts. Use it to tear down transient UI state.

```javascript
document.addEventListener('spa:before-navigate', (e) => {
    const { url } = e.detail;
    // Close modals, dismiss tooltips, reset scroll indicators
});
```

### spa:commit

Fires after new island content is injected into the DOM. Use it to reinitialise interactive features.

```javascript
document.addEventListener('spa:commit', (e) => {
    const { url, data } = e.detail;
    // Re-highlight code blocks, rebuild table of contents,
    // update active navigation links, reinit copy buttons
});
```

| Detail field | Type | Description |
|-------------|------|-------------|
| `url` | `URL` | The navigated URL |
| `data` | `object` | The full `SpaEnvelopeDto` (title, description, islands) |

## Creating a Custom Island Renderer

The minimal path — implement `IIslandRenderer` directly:

```csharp
public class TableOfContentsRenderer : IIslandRenderer
{
    public string IslandName => "toc";

    public Task<string> RenderAsync(ContentRoute route, RenderContext context)
    {
        // Build TOC HTML from route, return empty string to skip
        return Task.FromResult("<nav>...</nav>");
    }
}
```

The Razor path — use `RazorIslandRenderer<TComponent>` when you want to render a Blazor component:

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

## Scripts

Include the SPA engine in your `App.razor` after other scripts:

```html
<script src="/_content/Penn/spa-engine.js" defer></script>
```

The engine activates automatically — no initialisation call needed. It discovers islands from `data-spa-island` attributes, intercepts same-origin link clicks, and handles browser history, view transitions, and scroll position restoration.
