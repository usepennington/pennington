---
title: "SPA Island Architecture"
description: "How Penn's island architecture enables instant client-side navigation on top of fully static HTML"
uid: "penn.under-the-hood.spa-island-architecture"
order: 3060
---

Penn generates fully static HTML. Every page works without JavaScript. Search engines index it, browsers render it, `curl` fetches it. Done.

But static HTML means every navigation is a full page load. For a documentation site where you are flipping between pages constantly, that gets noticeable. So Penn adds a layer on top: after the first page load, subsequent navigations swap just the dynamic regions -- called *islands* -- without a full reload. Click a sidebar link, and only the content area and the page outline update. The shell, navigation, and theme stay put.

This is progressive enhancement in the truest sense. SPA navigation makes things faster when available and breaks nothing when it is not.

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

For example, a content island renderer might look like:

```csharp
public class ContentIslandRenderer(
    ComponentRenderer renderer,
    IContentPipeline pipeline) : RazorIslandRenderer<ArticleContent>(renderer)
{
    public override string IslandName => "content";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(
        ContentRoute route)
    {
        // Look up the rendered content for this route
        var html = await GetRenderedHtml(route);
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

For a given route, it calls each registered `IIslandRenderer` and collects their HTML output into a dictionary keyed by island name. If no renderers produce content, it returns `null` (which becomes a 404 for the SPA data endpoint).

### SpaEnvelope

The data shape returned to the client:

```csharp:xmldocid
T:Penn.Islands.SpaEnvelope
```

A simple record with:

- **`Title`**: Page title (for `document.title`).
- **`Description`**: Optional description (for `meta[name="description"]`).
- **`Social`**: Optional social metadata.
- **`Islands`**: An `ImmutableDictionary<string, string>` mapping island names to rendered HTML.

### SPA Data Endpoint

The endpoint is registered via `UseSpaNavigation()`:

```
GET /_spa-data/{*slug}.json
```

Slugs are URL paths without the leading slash, with `"index"` for the root. The `SpaSlug` utility handles conversion:

| URL | Slug |
|---|---|
| `/` | `index` |
| `/docs/intro` | `docs/intro` |
| `/blog/my-post` | `blog/my-post` |

Island renderers never see slugs -- they receive `ContentRoute` objects with full URLs. The slug is purely a transport detail between client and server.

The endpoint resolves the page title by checking registered content services' TOC entries. If no match is found, the slug itself is used as the title (not ideal, but graceful degradation beats a 500 error).

## Client-Side Flow

### Link Interception

`spa-engine.js` intercepts internal link clicks. A link is eligible for SPA navigation if:

- It is an internal link (same origin).
- It does not have `target="_blank"` or a `download` attribute.
- No modifier keys are held (Ctrl/Cmd+click opens a new tab normally).
- It is not a hash-only anchor (`#section` on the current page).

Everything else gets a full page load. SPA navigation is opt-in at the link level.

### The Fetch Race

When a link is intercepted, the engine races the JSON fetch against a configurable delay (default 100ms):

```
Link clicked
  |
  v
Promise.race(fetch, delay)
  |
  +--> Fetch wins (fast/cached): Commit immediately with view transition
  |
  +--> Delay wins (slow): Show skeleton UI
         |
         v
       Await fetch completion
         |
         v
       Hold skeleton >= 250ms (avoid sub-frame flash)
         |
         v
       Commit with transition
```

The two-path approach prevents both problems: flickering skeletons on fast navigations and frozen UIs on slow ones.

- **Fast path**: The JSON fetch completes before the delay. Content commits immediately with a CSS view transition. No skeleton ever appears.
- **Slow path**: If the delay expires first, a skeleton UI appears. Once the fetch completes, the skeleton is held for a minimum duration (250ms) to avoid a jarring flash, then the real content replaces it.

### Island Discovery

On each navigation, the engine queries the DOM for elements with `[data-spa-island]` attributes and builds a map of island name to element. Each discovered island gets an auto-assigned `view-transition-name` (e.g., `spa-island-content`) so CSS view transitions animate each island independently.

### Content Injection

After the JSON arrives, the engine iterates `data.islands` and sets `innerHTML` on the matching DOM element. Islands present in the JSON but absent from the DOM are silently ignored. Islands present in the DOM but absent from the JSON are left unchanged.

This loose coupling is intentional. The server can produce islands that only certain layouts consume. A layout without a sidebar island simply ignores the sidebar data in the JSON.

### Post-Navigation Updates

After island HTML is injected, the engine handles:

1. **`document.title`** -- updated from the envelope's `Title` field.
2. **`meta[name="description"]`** -- updated from the envelope's `Description` field.
3. **History** -- `pushState` for forward navigation, `popstate` handler for back/forward.
4. **Scroll position** -- scrolls to top for new pages, restores position for back navigation.

Then it fires `spa:commit` on `document`. This is the integration point for the rest of Penn's JavaScript:

- `PageManager` reinitializes all component managers for the new content.
- Syntax highlighting for any remaining client-side code blocks.
- Tab state synchronization.
- Mermaid diagram rendering.
- Page outline reconstruction from new headings.
- Active navigation link updating.
- Social meta tag updates (`og:title`, `twitter:title`).

## Static Generation

During `dotnet run -- build`, SPA data files are generated alongside HTML pages. For every HTML page, a corresponding `/_spa-data/{slug}.json` file is produced by fetching it from the running app (same self-crawl approach as HTML pages).

The output looks like:

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

Both HTML pages and SPA data files are pre-generated static files. No server at runtime. A CDN or static file host serves everything. The SPA engine in the browser fetches `.json` files from the same host as the HTML -- no API server needed.

## Fallback Behavior

The SPA engine falls back to a full page load when:

- The `/_spa-data/{slug}.json` fetch returns a non-200 status.
- No `[data-spa-island]` elements exist in the current page layout.
- The link has `target="_blank"`, `download`, or modifier keys held.
- The link is a hash-only anchor on the current page.
- JavaScript is disabled or `spa-engine.js` failed to load.

Fallback is a full page load -- the browser navigates normally. The site works exactly as if the SPA layer did not exist. This is the progressive enhancement contract: SPA navigation improves the experience when available but never breaks navigation when it is not.

## Adding a Custom Island

To add a new island:

1. **Create the Razor component** that renders the island's content.

2. **Create an `IIslandRenderer`** (extend `RazorIslandRenderer<TComponent>`):

   ```csharp
   public class MyIslandRenderer(ComponentRenderer renderer)
       : RazorIslandRenderer<MyIslandComponent>(renderer)
   {
       public override string IslandName => "my-island";
   
       protected override Task<IDictionary<string, object?>?> BuildParametersAsync(
           ContentRoute route)
       {
           return Task.FromResult<IDictionary<string, object?>?>(
               new Dictionary<string, object?>
               {
                   ["Route"] = route
               });
       }
   }
   ```

3. **Register the renderer** in DI:

   ```csharp
   services.AddScoped<IIslandRenderer, MyIslandRenderer>();
   ```

4. **Add the DOM element** in your layout:

   ```html
   <div data-spa-island="my-island">
       <!-- SSR content here, replaced during SPA navigation -->
   </div>
   ```

The island name in `data-spa-island` must match the renderer's `IslandName`. The SPA engine handles everything else -- discovery, fetching, injection, and view transitions.

## The Full Flow

Here is a complete SPA navigation, step by step:

```
1. User clicks link to /docs/configuration
2. spa-engine.js intercepts the click
3. Starts fetch: GET /_spa-data/docs/configuration.json
4. Races fetch vs 100ms delay
5. (If fast) Fetch completes:
     { "title": "Configuration", "islands": { "content": "<article>...", "outline": "<nav>..." } }
6. Engine discovers [data-spa-island="content"] and [data-spa-island="outline"] in DOM
7. Sets innerHTML on both elements
8. Updates document.title to "Configuration"
9. Pushes /docs/configuration to browser history
10. Fires spa:commit event
11. PageManager reinitializes:
    - OutlineManager rebuilds from new headings
    - TabManager initializes any new code tabs
    - MermaidManager renders any new diagrams
12. Scrolls to top
13. Done. ~50ms total for a cached response.
```

The user sees an instant page transition. The shell never reloads. The theme, navigation state, and scroll position of fixed elements are preserved. For a documentation site where readers bounce between dozens of pages, this is a significant quality-of-life improvement -- built entirely on top of static HTML that works just fine without it.
