---
title: "Adding SPA Navigation with Islands"
description: "Convert a server-rendered Pennington site to client-side SPA navigation by implementing island renderers, wiring the SPA data endpoint, and handling lifecycle events"
uid: "penn.tutorials.adding-spa-navigation-with-islands"
order: 20
---

## Beat 1: Starting Point — Full-Page Reloads

Set the scene. The reader has a multi-page Pennington site with a sidebar and content area. Every link click triggers a full-page HTML download. Open the browser Network tab and click between pages to see 15-30 KB HTML documents on every navigation.

### What to show
- Describe the existing site structure: a two-column layout with sidebar navigation and a content area
- Instruct the reader to open the browser Network tab and click between 2-3 pages, noting the full HTML document transfers
- State the goal: after this tutorial, clicking between pages will swap only the content area (approximately 2 KB of JSON instead of a full HTML document), and the sidebar will stay in place

### Key points
- SPA navigation is an enhancement layer, not a replacement for server-rendered HTML — the first page load is always full static HTML
- Pennington's island system works with pre-rendered HTML, not a client-side JavaScript framework — there is no virtual DOM, no component hydration

## Beat 2: Register SPA Navigation Services

Islands are named DOM regions that get swapped during client-side navigation instead of reloading the entire page. When a user clicks a link, the spa-engine JavaScript intercepts the click and fetches a JSON envelope from `/_spa-data/{slug}.json`. This envelope contains pre-rendered HTML for each island, keyed by name. The engine swaps the HTML inside each matching `data-spa-island` element and updates the URL via the History API. The envelope shape looks like this:

```json
{
  "title": "Page Title",
  "description": "Page description",
  "islands": {
    "content": "<html>...</html>",
    "sidebar": "<html>...</html>"
  }
}
```

Now wire up the SPA infrastructure in the service collection and middleware pipeline.

### What to show
- Code reference: `M:Pennington.Islands.SpaNavigationExtensions.AddSpaNavigation(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Pennington.Islands.SpaNavigationOptions})` — show the extension method; explain it registers `SpaPageDataService`, `SpaNavigationContentService`, and a default `RenderContext`
- Code reference: `M:Pennington.Islands.SpaNavigationExtensions.UseSpaNavigation(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder)` — show the extension method; explain it maps the `/_spa-data/{*slug}` endpoint
- Code reference: `T:Pennington.Islands.SpaNavigationOptions` — show the class with its `DataPath` property (defaults to `/_spa-data`)
- Code reference: `T:Pennington.Islands.SpaEnvelopeDto` — the record that represents the JSON envelope: `Title`, `Description`, `Islands` dictionary, `Diagnostics`, `Reload`
- Show how DocSite registers these: `:path src/Pennington.DocSite/DocSiteServiceExtensions.cs` (line 62 for `AddSpaNavigation()`, line 86 for `UseSpaNavigation()`)
- For a custom app, show the registration:
  ```csharp
  builder.Services.AddSpaNavigation();
  // ... later ...
  app.UseSpaNavigation();
  ```

### Key points
- `SpaNavigationContentService` automatically generates `/_spa-data/{slug}.json` routes for every HTML content page, so the static site builder produces JSON files alongside the HTML
- The `DataPath` is configurable but the default `/_spa-data` is conventional and should rarely need changing
- **Important**: `AddSpaNavigation` does NOT register `ComponentRenderer`. Custom (non-DocSite) apps must register it manually: `services.AddScoped<ComponentRenderer>()`. DocSite does this automatically in `AddDocSite` (`:path src/Pennington.DocSite/DocSiteServiceExtensions.cs` line 65)

## Beat 3: Create the Article Island Renderer

Every island renderer implements `IIslandRenderer`, which defines two members: `IslandName` (matching the `data-spa-island` attribute in HTML) and `RenderAsync(ContentRoute route, RenderContext context)` which returns an HTML string. For Razor-based islands, `RazorIslandRenderer<TComponent>` provides a base class that handles the rendering pipeline — you only override `BuildParametersAsync` to supply the parameter dictionary for your component, and return `null` to skip the island for a given route.

Now create the content island renderer.

### What to show
- Code reference: `T:Pennington.Islands.IIslandRenderer` — show the full interface:
  - `string IslandName { get; }` — the name that matches the `data-spa-island` attribute in HTML
  - `Task<string> RenderAsync(ContentRoute route, RenderContext context)` — renders the island's HTML for a given route
- Code reference: `T:Pennington.Routing.ContentRoute` — show the record (particularly `CanonicalPath`, `OutputFile`, `Locale`, `IsFallback`) to explain what the renderer receives
- Code reference: `T:Pennington.Islands.RenderContext` — show the record: `BaseUrl`, `SiteTitle`, `Locale`
- Code reference: `T:Pennington.Islands.RazorIslandRenderer``1` — show the full abstract class:
  - Constructor receives `ComponentRenderer renderer`
  - Abstract `IslandName` property
  - Abstract `M:Pennington.Islands.RazorIslandRenderer``1.BuildParametersAsync(Pennington.Routing.ContentRoute)` method — returns a parameter dictionary or `null` to skip the island
  - The `RenderAsync` implementation: calls `BuildParametersAsync`, then `renderer.RenderComponentAsync<TComponent>(parameters)`
- Code reference for the real-world example: `:path src/Pennington.DocSite/Slots/DocSiteArticleSlotRenderer.cs` — show the full class (approximately 38 lines):
  - Extends `RazorIslandRenderer<DocSiteArticle>`
  - `IslandName => "content"`
  - `BuildParametersAsync` resolves content via `ContentResolver`, gets navigation info, and returns a dictionary with `Title`, `HtmlContent`, `PreviousPageName`, `PreviousPageHref`, `NextPageName`, `NextPageHref`, `FallbackRequestedLocale`, `FallbackDefaultLocale`
- Show the Razor component it renders: `:path src/Pennington.DocSite/Slots/Components/DocSiteArticle.razor` — the component that receives these parameters

### Key points
- `IslandName` must exactly match the `data-spa-island` value in your layout HTML — case-sensitive
- `RenderAsync` returns an HTML string (not a component) — the SPA engine injects this raw HTML into the DOM
- Returning an empty string from `RenderAsync` means the island has no content for this route and will be skipped in the envelope
- `BuildParametersAsync` returns `null` to signal that this island has no content for the given route — the island will be omitted from the JSON envelope
- The parameter dictionary keys must match the `[Parameter]` property names on the target Razor component exactly (use `nameof()` for safety)
- `RazorIslandRenderer<T>` handles the Razor-to-HTML rendering via `ComponentRenderer` — you only need to supply the parameters

## Beat 4: Register the Island Renderer

Show how to register the island renderer in the DI container.

### What to show
- Register the island renderer via direct DI registration (what DocSite uses): `services.AddTransient<IIslandRenderer, ArticleIslandRenderer>();` — reference `:path src/Pennington.DocSite/DocSiteServiceExtensions.cs` line 68
- `SpaPageDataService` collects all `IIslandRenderer` implementations from DI and iterates them for each page

### Key points
- The `IslandName` property on the renderer must match the `data-spa-island` attribute in the layout HTML — case-sensitive
- Multiple island renderers can be registered — `SpaPageDataService` iterates all of them for each page and includes any that produce non-empty HTML

## Beat 5: Mark Layout Regions with data-spa-island

Show the reader how to annotate their layout HTML to designate island regions.

### What to show
- Reference the DocSite MainLayout as a real example: `:path src/Pennington.DocSite/Components/Layout/MainLayout.razor` (line 190) — show the article element:
  ```html
  <article id="main-content" data-spa-island="content" data-spa-loading="skeleton" class="...">
      @Body
  </article>
  ```
- Explain the three `data-spa-loading` modes from the spa-engine.js documentation:
  - `"keep"` (default) — leave previous content in place until new data arrives; best for navigation sidebars
  - `"skeleton"` — replace content with a shimmer placeholder immediately; best for the main content area
  - `"clear"` — empty the element immediately; for elements that should not show stale content
- Show how the spa-engine discovers islands: `:path src/Pennington.UI/wwwroot/spa-engine.js` (lines 124-141) — the `discoverIslands()` function that queries `[data-spa-island]` elements and reads their loading mode

### Key points
- The `data-spa-island` value must match an `IslandName` from a registered `IIslandRenderer`
- You can have as many islands as you need — common setups use 1 (content only) or 2 (content + sidebar)
- The first page load always renders full HTML normally; the island attributes only take effect on subsequent client-side navigations

## Beat 6: Add a Loading Skeleton

Show how to provide a custom skeleton template for the content island.

### What to show
- Explain the skeleton system: when `data-spa-loading="skeleton"` is set, the spa-engine looks for a `<template data-spa-skeleton-for="name">` element. If found, it clones the template content into the island during loading. If not found, it uses a built-in default shimmer
- Show a custom skeleton template:
  ```html
  <template data-spa-skeleton-for="content">
      <div class="animate-pulse">
          <div class="h-8 bg-base-200 dark:bg-base-800 rounded w-3/4 mb-4"></div>
          <div class="h-4 bg-base-200 dark:bg-base-800 rounded w-full mb-2"></div>
          <div class="h-4 bg-base-200 dark:bg-base-800 rounded w-5/6 mb-2"></div>
          <div class="h-4 bg-base-200 dark:bg-base-800 rounded w-4/5"></div>
      </div>
  </template>
  ```
- Reference the timing configuration from spa-engine.js: `SKELETON_DELAY` (default 100ms — skeleton only appears after this delay, preventing flash on fast connections) and `MIN_SKELETON_MS` (default 250ms — once shown, skeleton stays for at least this long to avoid jarring flash)

### Key points
- The skeleton delay prevents the placeholder from appearing on fast navigations — if the JSON loads in under 100ms, the user sees a direct swap instead
- The minimum skeleton duration prevents a jarring flash when the skeleton appears for only a few milliseconds
- Both values can be overridden via `data-spa-skeleton-delay` and `data-spa-min-skeleton` attributes on the root HTML element

## Beat 7: Listen for SPA Lifecycle Events

Show how to hook into the navigation lifecycle for custom JavaScript behavior.

### What to show
- Explain the two events dispatched on `document`:
  - `spa:before-navigate` — fired before the JSON fetch begins; detail payload: `{ url, slug }`
  - `spa:commit` — fired after island content has been swapped; detail payload: `{ url, slug, data }`
- Show a practical example script:
  ```javascript
  document.addEventListener('spa:commit', (e) => {
      // Reinitialize copy-to-clipboard buttons on new content
      document.querySelectorAll('[data-copy-button]').forEach(btn => {
          btn.addEventListener('click', handleCopy);
      });
  });
  ```
- Reference the event dispatching in the spa-engine: `:path src/Pennington.UI/wwwroot/spa-engine.js` — `fire('spa:before-navigate', { url, slug })` and `fire('spa:commit', { url, slug, data })`
- Mention the View Transitions API integration: the spa-engine uses `document.startViewTransition()` when available, providing smooth CSS transitions between page states

### Key points
- `spa:before-navigate` is useful for canceling ongoing animations, aborting XHR requests, or saving scroll position
- `spa:commit` is the right place to reinitialize any imperative JavaScript that operates on the new DOM content
- Links with `data-spa-reload` attribute are excluded from SPA navigation and cause a full page reload (used by the language switcher, for example)

## Beat 8: Run and Verify

Walk through the final verification steps.

### What to show
- Run command: `dotnet run`
- Open the browser Network tab, filter for XHR/Fetch requests
- Click between pages and observe:
  - The sidebar navigation stays in place (no flash, active item updates)
  - The content area swaps with a brief transition
  - The URL in the address bar updates
  - The Network tab shows small JSON requests to `/_spa-data/{slug}.json` instead of full HTML documents
  - The browser back/forward buttons work correctly
- Throttle the network to "Slow 3G" in DevTools — observe the skeleton placeholder appearing in the content area while JSON loads
- Inspect a JSON response in the Network tab to see the envelope structure with `title`, `description`, and `islands`

### Key points
- If a page returns a `Reload: true` envelope (no island renderers produced content), the spa-engine falls back to a full page navigation gracefully
- The spa-engine sets `aria-live="polite"` announcements on navigation for screen readers, and moves focus to the first content island
- View Transitions provide a subtle fade animation in browsers that support the API; browsers without it get an instant swap
- During static builds (`dotnet run -- build`), JSON files are generated automatically alongside HTML pages — no additional configuration needed
