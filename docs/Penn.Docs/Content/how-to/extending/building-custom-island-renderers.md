---
title: "Building Custom Island Renderers"
description: "Create island renderers by implementing IIslandRenderer directly or extending RazorIslandRenderer<T>, wire BuildParametersAsync for content resolution, and register with SpaPageDataService"
uid: "penn.how-to.building-custom-island-renderers"
order: 30
---

## Beat 1: The Island Architecture — Named Slots in the Layout

Explain what islands are in Penn's SPA navigation model: named regions of the page that update independently during client-side navigation without a full page reload.

### What to show
- A layout HTML snippet with `<div data-spa-island="team-activity"></div>` — the `data-spa-island` attribute marks a named slot
- The flow: on initial page load, island renderers run server-side and their HTML replaces the slot content. On SPA navigation, `T:Penn.Islands.SpaPageDataService` calls all registered renderers for the new route and delivers updated HTML via a JSON envelope
- The `T:Penn.Islands.SpaEnvelopeDto` record at `:path:src/Penn/Islands/SpaEnvelopeJsonContext.cs`: `SpaEnvelopeDto(string Title, string? Description, Dictionary<string, string> Islands, IReadOnlyList<Diagnostic>? Diagnostics, bool? Reload)` — the `Islands` dictionary maps island names to rendered HTML strings

### Key points
- Islands solve the problem of dynamic page regions (sidebars, breadcrumbs, table of contents) that need to update during SPA navigation
- Each island is independent — a page can have multiple islands, and each renderer decides per-route whether to produce content
- The island name in `data-spa-island` must exactly match the `IslandName` returned by the renderer

## Beat 2: The Two Implementation Paths

Present the two ways to build an island renderer and when to use each.

### What to show
- `T:Penn.Islands.IIslandRenderer` interface at `:path:src/Penn/Islands/IIslandRenderer.cs`:
  - `P:Penn.Islands.IIslandRenderer.IslandName` — string identifier matching the `data-spa-island` attribute
  - `M:Penn.Islands.IIslandRenderer.RenderAsync(Penn.Routing.ContentRoute,Penn.Islands.RenderContext)` — takes the current route and render context, returns an HTML string (or empty string to skip this island for this route)
- `T:Penn.Islands.RazorIslandRenderer`1` abstract class at `:path:src/Penn/Islands/RazorIslandRenderer.cs`:
  - Extends `T:Penn.Islands.IIslandRenderer`
  - Generic parameter `TComponent` constrained to `IComponent` — the Blazor component to render
  - Constructor takes `T:Penn.Islands.ComponentRenderer` (injected, renders Blazor components to HTML strings)
  - Abstract `M:Penn.Islands.RazorIslandRenderer`1.BuildParametersAsync(Penn.Routing.ContentRoute)` — returns `IDictionary<string, object?>?` or `null` to skip rendering
  - `M:Penn.Islands.IIslandRenderer.RenderAsync(Penn.Routing.ContentRoute,Penn.Islands.RenderContext)` implementation calls `BuildParametersAsync`, then delegates to `M:Penn.Islands.ComponentRenderer.RenderComponentAsync``1(System.Collections.Generic.IDictionary{System.String,System.Object})` if parameters are non-null
- Decision guide: use `RazorIslandRenderer<T>` when you want to render a Blazor component with parameters resolved from the route; use `IIslandRenderer` directly when you need to produce raw HTML or have custom rendering logic

### Key points
- `T:Penn.Islands.RazorIslandRenderer`1` handles all the Blazor plumbing — you only need to implement `BuildParametersAsync`
- Returning `null` from `BuildParametersAsync` signals "skip this island for this route" — the `RenderAsync` implementation returns empty string
- `T:Penn.Islands.ComponentRenderer` at `:path:src/Penn/Islands/ComponentRenderer.cs` uses `HtmlRenderer` for static server-side rendering — no WebSocket or SignalR involved

## Beat 3: The RenderContext and Route Data

Explain what information is available to renderers during execution.

### What to show
- `T:Penn.Islands.RenderContext` record at `:path:src/Penn/Islands/RenderContext.cs`: `RenderContext(UrlPath BaseUrl, string SiteTitle, string? Locale)`
  - `BaseUrl` — the site's base URL path (from `P:Penn.Infrastructure.PennOptions.CanonicalBaseUrl`)
  - `SiteTitle` — the site title (from `P:Penn.Infrastructure.PennOptions.SiteTitle`)
  - `Locale` — current locale or null for single-locale sites
- `T:Penn.Routing.ContentRoute` passed to `RenderAsync` and `BuildParametersAsync`:
  - `P:Penn.Routing.ContentRoute.CanonicalPath` — the page URL (e.g., `/guides/getting-started/`)
  - `P:Penn.Routing.ContentRoute.OutputFile` — the output file path (e.g., `guides/getting-started/index.html`)
  - `P:Penn.Routing.ContentRoute.Locale` — locale code for localized routes

### Key points
- Renderers can use `ContentRoute` to tailor output per page — e.g., showing page-relevant sidebar content
- `RenderContext` provides site-wide context that does not change per page
- For a typical island, `BuildParametersAsync` queries a service using the route, then maps results to component parameters

## Beat 4: Create the Blazor Component

Build a `TeamActivity.razor` component that displays a list of recent content activity items.

### What to show
- A `TeamActivity.razor` component with a `[Parameter] public ActivityItem[] Items { get; set; }` parameter
- The component renders a `<ul>` with each item showing author name, page title as a link, and a relative timestamp
- An `ActivityItem` record: `ActivityItem(string Author, string PageTitle, string Url, DateTime Timestamp)`
- ~25 lines of Razor markup with basic styling

### Key points
- This is a standard Blazor component — nothing Penn-specific about it
- The component receives already-resolved data via parameters; it does not call services directly
- Keep the component focused on presentation — data resolution belongs in the renderer

## Beat 5: Create the RazorIslandRenderer

Extend `T:Penn.Islands.RazorIslandRenderer`1` to render the `TeamActivity` component with route-aware parameters.

### What to show
- Class declaration: `public sealed class TeamActivityIslandRenderer : RazorIslandRenderer<TeamActivity>`
- Constructor: `TeamActivityIslandRenderer(ComponentRenderer renderer, IEnumerable<IContentService> contentServices) : base(renderer)` — inject `T:Penn.Islands.ComponentRenderer` (passed to base) and content services for data resolution
- `P:Penn.Islands.IIslandRenderer.IslandName` returns `"team-activity"`
- `M:Penn.Islands.RazorIslandRenderer`1.BuildParametersAsync(Penn.Routing.ContentRoute)` implementation:
  - Query `T:Penn.Content.IContentService` instances for recent `T:Penn.Content.ContentTocItem` entries via `M:Penn.Content.IContentService.GetContentTocEntriesAsync`
  - Map to `ActivityItem[]` with the current route's section emphasized
  - Return `new Dictionary<string, object?> { ["Items"] = items }` — the key `"Items"` must match the `[Parameter]` property name on the component
  - Return `null` if no activity items are relevant (island renders nothing for this route)
- ~30 lines total

### Key points
- The dictionary keys in `BuildParametersAsync` must exactly match `[Parameter]` property names on the component (case-sensitive)
- Returning `null` from `BuildParametersAsync` is the correct way to opt out of rendering for a specific route — the base class handles converting this to an empty string in `RenderAsync`
- The renderer can inject any service registered in DI — it participates in the normal DI container

## Beat 6: Register the Island

Wire the renderer into the DI container.

### What to show
- In `Program.cs`, register via DI: `services.AddTransient<IIslandRenderer, TeamActivityIslandRenderer>()`
- Reference DocSite's registration as an example: `:path src/Penn.DocSite/DocSiteServiceExtensions.cs` line 68 — `services.AddTransient<IIslandRenderer, Slots.DocSiteArticleSlotRenderer>()`
- `T:Penn.Islands.SpaPageDataService` receives all `IIslandRenderer` registrations via constructor injection and iterates them for each page
- The `IslandName` property on the renderer must match the `data-spa-island` attribute in the layout

### Key points
- Registration is by type, not instance — the DI container creates the renderer and injects its dependencies
- Multiple islands can be registered; each has a unique name
- Island names are case-sensitive strings

## Beat 7: Add the Island Slot to the Layout

Place the `data-spa-island` marker in the layout Razor component so the renderer output appears in the sidebar.

### What to show
- In the layout component, add `<div data-spa-island="team-activity"></div>` in the sidebar region
- Explain the two rendering paths:
  1. **Initial page load (SSR)**: Penn's middleware renders all island renderers and injects their HTML into the page. The `data-spa-island` div is replaced with the rendered content
  2. **SPA navigation**: The JavaScript client fetches `/_spa-data/{slug}.json` which returns the `T:Penn.Islands.SpaEnvelopeDto` with updated island HTML. The client replaces the island div content

### Key points
- The `data-spa-island` div acts as a placeholder on SSR and as a target for client-side updates during SPA navigation
- If a renderer returns empty string for a route, the island div remains empty — it does not show stale content from the previous page

## Beat 8: How SpaPageDataService Assembles the Envelope

Walk through `T:Penn.Islands.SpaPageDataService` to show how renderers are coordinated.

### What to show
- `M:Penn.Islands.SpaPageDataService.GetPageDataAsync(Penn.Routing.ContentRoute,System.String,System.String)` at `:path:src/Penn/Islands/SpaPageDataService.cs`:
  - Iterates all `IEnumerable<T:Penn.Islands.IIslandRenderer>` instances (injected via DI)
  - Calls `M:Penn.Islands.IIslandRenderer.RenderAsync(Penn.Routing.ContentRoute,Penn.Islands.RenderContext)` on each renderer with the current route and `T:Penn.Islands.RenderContext`
  - Collects non-empty results into a `Dictionary<string, string>` keyed by `IslandName`
  - Returns `T:Penn.Islands.SpaEnvelopeDto` with the title, description, and islands dictionary — or `null` if no renderer produced content
- Show the SPA navigation endpoint at `:path:src/Penn/Islands/SpaNavigationExtensions.cs` where `M:Penn.Islands.SpaNavigationExtensions.UseSpaNavigation(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder)` maps `GET /_spa-data/{slug}` and calls `GetPageDataAsync`

### Key points
- Renderers are called in registration order for each navigation event — keep `BuildParametersAsync` fast to avoid SPA navigation latency
- The envelope is serialized to JSON via `T:Penn.Islands.SpaEnvelopeSerializer` — HTML strings are embedded as JSON values
- The `Reload` flag on `T:Penn.Islands.SpaEnvelopeDto` signals the client to do a full page reload instead of a partial update (used when no renderers produce content)

## Beat 9: Alternative — Raw IIslandRenderer for Simple Cases

Show a minimal `T:Penn.Islands.IIslandRenderer` implementation for islands that do not need a Blazor component.

### What to show
- A `LastUpdatedIslandRenderer` implementing `T:Penn.Islands.IIslandRenderer` directly:
  - `P:Penn.Islands.IIslandRenderer.IslandName` returns `"last-updated"`
  - `M:Penn.Islands.IIslandRenderer.RenderAsync(Penn.Routing.ContentRoute,Penn.Islands.RenderContext)` returns a `<time>` element with the current UTC timestamp — no component needed, just raw HTML string construction
- Compare with the `RazorIslandRenderer<T>` approach: direct implementation is simpler for static or trivially computed content; the Razor approach is better when you have complex markup or want to use Blazor features like `@foreach`, `@if`, CSS isolation

### Key points
- Direct `IIslandRenderer` implementations bypass Blazor rendering entirely — no `ComponentRenderer` dependency
- Both approaches register the same way via `options.Islands.Register<T>(name)`
- Choose based on complexity: raw HTML for simple text/timestamps, Razor components for structured UI
