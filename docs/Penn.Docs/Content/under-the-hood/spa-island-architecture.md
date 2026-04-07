---
title: "SPA Island Architecture"
description: "How Penn's island architecture enables instant client-side navigation on top of fully static HTML"
uid: "penn.under-the-hood.spa-island-architecture"
order: 3060
---

Penn generates fully static HTML. Every page works without JavaScript. Search engines index it, browsers render it, `curl` fetches it. Done.

But static HTML means every navigation is a full page load. For a documentation site where readers flip between pages constantly, that gets noticeable. So Penn adds a layer on top: after the first page load, subsequent navigations swap just the dynamic regions -- called *islands* -- without a full reload. Click a sidebar link, and only the content area and the page outline update. The shell, navigation, and theme stay put.

This is progressive enhancement in the truest sense. The SPA layer makes navigation faster when JavaScript is available and breaks nothing when it is not. The page still works. The links still link. The HTML is still HTML.

## The Two Rendering Paths

Every island has a single Razor component that serves both paths. There is no separate "SPA template." The same component renders during Blazor SSR (first load) and via `HtmlRenderer` (SPA navigation).

```
Path 1: First Load (SSR)                   Path 2: SPA Navigation
========================                    ======================
Browser requests /docs/intro                User clicks /docs/config link
  --> ASP.NET routes to Page.razor            --> spa-engine.js intercepts click
  --> Page.razor renders islands              --> Fetches /_spa-data/docs/config.json
  --> Full HTML page returned                 --> Server: SpaPageDataService
  --> Browser renders everything                --> Each IIslandRenderer renders
                                                --> JSON envelope returned
                                              --> Client swaps island innerHTML
                                              --> Updates title, URL, scroll

Same Razor components render in both paths.
```

Path 1 returns a complete HTML document -- `<html>`, `<head>`, layout shell, all islands pre-rendered. Path 2 returns a JSON envelope containing just the island HTML fragments and page metadata. The client handles the rest: swapping content, updating the document title, pushing history state.

The key insight is that Razor components are rendered to strings on the server in both cases. During SSR, Blazor's built-in static rendering pipeline produces the HTML. During SPA navigation, Penn's `ComponentRenderer` uses Blazor's `HtmlRenderer` to produce the same strings outside the request pipeline. Same components, same parameters, same output.

## Server-Side Components

### IIslandRenderer

The interface for server-side island rendering:

```csharp:xmldocid
T:Penn.Islands.IIslandRenderer
```

Each renderer has a name (`IslandName`) and produces HTML for a given route. The name must match the `data-spa-island` attribute in the page's DOM. During SPA navigation, only islands whose names appear in both the JSON response and the DOM get updated.

### RazorIslandRenderer\<TComponent\>

The base class that most island renderers extend:

```csharp:xmldocid
T:Penn.Islands.RazorIslandRenderer`1
```

Subclasses implement `BuildParametersAsync()` to produce the component's parameter dictionary, or return `null` to skip the island for a given route. The base class handles the actual rendering via `ComponentRenderer`.

A typical implementation:

```csharp
public class ContentIslandRenderer(
    ComponentRenderer renderer,
    IContentPipeline pipeline) : RazorIslandRenderer<ArticleContent>(renderer)
{
    public override string IslandName => "content";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(
        ContentRoute route)
    {
        var html = await pipeline.RenderAsync(route);
        if (html is null) return null;

        return new Dictionary<string, object?>
        {
            ["Html"] = html,
            ["Outline"] = outline
        };
    }
}
```

### ComponentRenderer

Wraps Blazor's `HtmlRenderer` to render Razor components to HTML strings:

```csharp:xmldocid
T:Penn.Islands.ComponentRenderer
```

Registered as scoped -- one `HtmlRenderer` instance is shared across all island renders within a single request, then disposed at scope end. Components rendered this way have access to `@inject` services from the DI container. They do *not* have access to `NavigationManager`, JavaScript interop, or other browser-dependent APIs. This is server-side string rendering, not an interactive Blazor circuit.

### SpaPageDataService

The orchestrator that assembles the JSON envelope:

```csharp:xmldocid
T:Penn.Islands.SpaPageDataService
```

For a given route, it iterates every registered `IIslandRenderer`, calls `RenderAsync`, and collects non-empty HTML output into a dictionary keyed by island name. If no renderers produce content, it returns `null`, which becomes a 404 at the SPA data endpoint.

### SpaEnvelopeDto

The data shape returned to the client:

```csharp:xmldocid
T:Penn.Islands.SpaEnvelopeDto
```

A record with four fields:

- **`Title`**: Page title (used for `document.title` and social meta tags).
- **`Description`**: Optional page description (for `meta[name="description"]`).
- **`Islands`**: A `Dictionary<string, string>` mapping island names to rendered HTML strings.
- **`Diagnostics`**: Optional diagnostic messages, included only in development when `DiagnosticContext` has entries.

Serialization uses `System.Text.Json` with camelCase naming and null-value omission, via `SpaEnvelopeSerializer`.

## The SPA Data Endpoint

The endpoint is registered by calling `UseSpaNavigation()` on the endpoint route builder:

```
GET /_spa-data/{*slug}.json
```

The path prefix is configurable via `SpaNavigationOptions.DataPath` (default: `/_spa-data`).

Slugs are URL paths with the leading slash removed. The root URL maps to `"index"`. The `SpaSlug` utility handles conversions:

| URL | Slug | JSON path |
|---|---|---|
| `/` | `index` | `/_spa-data/index.json` |
| `/docs/intro` | `docs/intro` | `/_spa-data/docs/intro.json` |
| `/blog/my-post` | `blog/my-post` | `/_spa-data/blog/my-post.json` |

Island renderers never see slugs -- they receive `ContentRoute` objects with full canonical URLs. The slug is purely a transport detail between client and server.

The endpoint handler does three things:

1. Converts the slug back to a URL via `SpaSlug.ToUrl()`, then constructs a `ContentRoute`.
2. Resolves the page title by iterating registered `IContentService` instances and matching TOC entries against the URL. If no match is found, the slug itself is used as the title.
3. Calls `SpaPageDataService.GetPageDataAsync()`, then runs xref resolution on the island HTML (because response processors do not run on JSON responses).

## Client-Side Flow

### Link Interception

`spa-engine.js` intercepts internal link clicks via a single delegated `click` listener on `document`. A link qualifies for SPA navigation if all of the following are true:

- Same origin as the current page.
- No `target="_blank"` or `download` attribute.
- No modifier keys held (Ctrl, Cmd, Shift, Alt -- these open new tabs normally).
- Not a hash-only anchor (`#section` on the current page).
- No `data-spa-reload` attribute (an explicit opt-out).
- The link does not point to the `/_spa-data` path itself.

Everything else falls through to a normal browser navigation.

### Island Discovery

On each navigation, the engine queries the DOM for all elements with `[data-spa-island]` attributes and builds a map of island name to element reference. If no islands are found in the current layout, the engine aborts and performs a full page load instead.

Each discovered island also gets an auto-assigned `view-transition-name` style (e.g., `spa-island-content`) so that CSS view transitions animate each island independently rather than cross-fading the entire page.

### The Fetch Race

When a link is intercepted, the engine races the JSON fetch against a configurable delay threshold:

```
Link clicked
  |
  v
Promise.race(fetch, delay(100ms))
  |
  +--> Fetch wins (fast/cached): Commit immediately with view transition
  |
  +--> Delay wins (slow): Show loading state
         |
         v
       Await fetch completion
         |
         v
       Hold skeleton >= 250ms minimum
         |
         v
       Commit with view transition
```

This two-path approach prevents both failure modes: flickering skeletons on fast navigations, and a frozen UI on slow ones.

- **Fast path**: The fetch completes within the delay threshold. Content commits immediately inside a view transition. No skeleton ever appears.
- **Slow path**: The delay expires first. The engine applies loading states to each island (see next section), then waits for the fetch. Once data arrives, it holds the skeleton for a minimum duration before committing.

If the fetch fails entirely (non-200 status), the engine falls back to `location.href` assignment -- a full page load.

Prefetching further biases toward the fast path. On `pointerover` and `focusin` events, the engine starts fetching the target page's JSON into a bounded cache (20 entries, LRU eviction). By the time the user actually clicks, the data is often already available.

## Loading Strategies

Each island declares its loading behavior via the `data-spa-loading` attribute:

| Strategy | Attribute value | Behavior during slow navigation |
|---|---|---|
| **Keep** | `keep` (default) | Previous content stays visible until new content arrives. |
| **Skeleton** | `skeleton` | Shows a shimmer placeholder (or a custom `<template>` if one exists with `data-spa-skeleton-for="island-name"`). |
| **Clear** | `clear` | Empties the island immediately. |

The `keep` strategy works well for islands where the old content is structurally similar to the new content (a sidebar navigation, for example). The `skeleton` strategy is better for the main content area, where stale content would be confusing. The `clear` strategy is for islands that should visually disappear while loading.

### The Timing Contract

Two constants govern the loading UX:

- **`SKELETON_DELAY`** (100ms, configurable via `data-spa-skeleton-delay` on `<html>`): How long to wait before showing any loading state. If the fetch completes within this window, loading states are never applied.
- **`MIN_SKELETON_MS`** (250ms, configurable via `data-spa-min-skeleton` on `<html>`): Once a skeleton is shown, it stays visible for at least this long. This prevents a jarring sub-frame flash where the skeleton appears and vanishes almost instantly.

The combined effect: navigations under 100ms feel instant. Navigations between 100ms and 350ms show a brief skeleton. Navigations over 350ms show the skeleton for their full duration. The thresholds were chosen to match the perceptual boundaries where users notice delay (100ms) and where animation feels intentional rather than glitchy (250ms).

### Custom Skeleton Templates

For the `skeleton` strategy, the default shimmer is a set of animated gradient bars. To provide a custom skeleton, add a `<template>` element:

```html
<template data-spa-skeleton-for="content">
    <div class="skeleton-header"></div>
    <div class="skeleton-paragraph"></div>
    <div class="skeleton-paragraph"></div>
</template>
```

When the engine applies loading state to the `content` island, it clones this template's content instead of using the default shimmer.

## View Transitions and Accessibility

### View Transitions

The engine wraps every content commit in `document.startViewTransition()` when the browser supports the View Transitions API. Each island gets a distinct `view-transition-name` (auto-assigned as `spa-island-{name}`), which means the browser can animate each island's old and new content independently.

A minimal stylesheet is injected at startup:

- `::view-transition-group(*)` gets a 150ms animation duration.
- Under `prefers-reduced-motion: reduce`, all view transition animations are disabled.

For browsers that do not support `startViewTransition`, the commit function runs synchronously. The content still updates; there is just no cross-fade.

### ARIA Live Region

On initialization, the engine creates a visually hidden `<div>` with `role="status"`, `aria-live="polite"`, and `aria-atomic="true"`. After each navigation, it sets the element's text to `"Navigated to {title}"` (or `"Page updated"` if no title is available).

The text is cleared before being set, with a `requestAnimationFrame` gap between the clear and the set. This ensures screen readers register the change as a new announcement even when navigating to a page with the same title.

### Focus Management

After content is committed, focus moves to the first heading (`h1`, `h2`, or `h3`) inside the first island. If no heading is found, the island element itself receives focus. A `tabindex="-1"` is added to the target if it does not already have one, making non-interactive elements focusable without adding them to the tab order.

`preventScroll: true` is passed to `focus()` so the focus change does not fight with the engine's own scroll handling.

## Post-Navigation Updates

After island HTML is injected and the view transition completes, the engine handles several housekeeping tasks:

1. **Document title** -- set to `"{SiteTitle} - {PageTitle}"` from the envelope's `Title` field. The site title is derived once at startup from the initial page's `document.title`.
2. **Meta tags** -- `description`, `og:title`, `og:description`, `og:url`, `twitter:title`, `twitter:description`, and `link[rel="canonical"]` are all updated from the envelope.
3. **History** -- `pushState` for forward navigation. The current entry's `scrollY` is saved via `replaceState` before leaving, so back navigation can restore scroll position.
4. **Scroll** -- scrolls to the hash target if the URL has one, otherwise scrolls to top. Back/forward navigation restores the saved `scrollY` from history state.
5. **`spa:commit` event** -- dispatched on `document` with `{ url, slug, data }` as detail. This is the integration point for the rest of Penn's JavaScript (see xref:penn.under-the-hood.javascript-architecture). `PageManager` listens for this event and reinitializes all component managers: `OutlineManager` rebuilds from new headings, `TabManager` initializes code tabs, `MermaidManager` renders diagrams, and navigation managers update active link state.
6. **`spa:diagnostics` event** -- dispatched with the envelope's diagnostics array (or empty). The dev overlay uses this to display per-page warnings.
7. **Stylesheet reload** -- on `localhost`, the engine reloads the first stylesheet with a cache-busting query parameter so CSS changes appear without a full refresh.

## Static Generation

During `dotnet run -- build`, Penn generates SPA data files alongside HTML pages. `SpaNavigationContentService` registers as an `IContentService` and discovers every HTML page from all other content services. For each page, it yields a `DiscoveredItem` with a route pointing to `/_spa-data/{slug}.json`.

The static site generator's self-crawl then fetches these URLs from the running app, hitting the SPA data endpoint, and writes the JSON responses to disk. The result:

```
output/
+-- index.html
+-- docs/
|   +-- intro/
|       +-- index.html
+-- _spa-data/
|   +-- index.json
|   +-- docs/
|       +-- intro.json
+-- scripts.js
```

Both HTML pages and SPA data files are pre-generated static files. No server at runtime. A CDN or static file host serves everything. The SPA engine in the browser fetches `.json` files from the same host as the HTML -- no API server, no serverless functions, no dynamic backend.

`SpaNavigationContentService` skips itself when iterating content services (to avoid infinite recursion) and only generates JSON for routes with `.html` or `.htm` output files (skipping CSS, JS, images, and other static assets).

## Fallback Behavior

The SPA engine falls back to a full page load when:

- The `/_spa-data/{slug}.json` fetch returns a non-200 status.
- No `[data-spa-island]` elements exist in the current page layout.
- The link has `target="_blank"`, a `download` attribute, or modifier keys are held.
- The link has a `data-spa-reload` attribute (explicit opt-out).
- The link is a hash-only anchor on the current page (the browser handles scrolling natively).
- JavaScript is disabled or `spa-engine.js` failed to load.

In every case, fallback means the browser navigates normally. The site works exactly as if the SPA layer did not exist. This is the progressive enhancement contract: SPA navigation improves the experience when available but never gates functionality on its presence.

In-flight navigations are also cancellable. Each navigation creates an `AbortController`. If the user clicks another link before the current fetch completes, the previous navigation is aborted and the new one takes over. The latest click always wins.

## The Full Flow

Here is a complete SPA navigation from click to rendered page:

```
 1. User hovers over link to /docs/configuration
 2. spa-engine.js prefetches /_spa-data/docs/configuration.json into cache
 3. User clicks the link
 4. Click handler intercepts (same origin, no modifiers, no data-spa-reload)
 5. Saves current scroll position into history state via replaceState
 6. Fires spa:before-navigate event
 7. Checks prefetch cache -- hit. Starts resolving cached promise.
 8. Races cached fetch vs 100ms delay
 9. Cached data resolves in ~2ms -- fast path wins
10. Discovers islands: [data-spa-island="content"], [data-spa-island="outline"]
11. Wraps commit in document.startViewTransition()
12. Sets innerHTML on content and outline elements from JSON envelope
13. Updates document.title to "Penn - Configuration"
14. Updates meta tags (description, og:title, canonical URL)
15. Pushes /docs/configuration to browser history
16. Announces "Navigated to Configuration" via ARIA live region
17. Moves focus to first heading in content island
18. Fires spa:commit event
19. PageManager reinitializes:
      - OutlineManager rebuilds from new headings
      - TabManager initializes any new code tabs
      - MermaidManager renders any new diagrams
      - Navigation managers update active link highlighting
20. Scrolls to top
21. Done. ~50ms total for a prefetched response.
```

The user sees an instant page transition. The shell never reloads. The theme, navigation state, and scroll position of fixed elements are preserved. For a documentation site where readers bounce between dozens of pages, this is a significant quality-of-life improvement -- built entirely on top of static HTML that works without any of it.

## See Also

- xref:penn.under-the-hood.javascript-architecture -- the `PageManager` system that reinitializes after SPA navigation.
