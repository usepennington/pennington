---
title: "Adding SPA Navigation"
description: "Add instant page transitions to your Penn site with the island renderer architecture"
uid: "penn.guides.adding-spa-navigation"
order: 2520
---

Penn generates static HTML. Every page works without JavaScript. But static HTML means every click is a full page load, and for a content site where you flip between pages constantly, that gets noticeable.

SPA navigation fixes this without giving up static rendering. The first visit loads full HTML from the server. After that, clicking an internal link fetches a small JSON envelope containing pre-rendered HTML for each named region -- called an *island* -- in your layout. The SPA engine swaps those regions in place. The surrounding shell, navigation, and theme stay put.

If JavaScript is unavailable or the fetch fails, the browser falls back to a normal page load. Nothing breaks.

## How SPA Navigation Works

The system has two rendering paths, and they share the same Razor components.

**First load (SSR):** The browser requests a URL. ASP.NET renders the full page via Blazor static SSR, including all island content. The result is a complete HTML document.

**Subsequent navigation (SPA):** The user clicks an internal link. The SPA engine intercepts the click, fetches `/_spa-data/{slug}.json`, receives a JSON envelope, and replaces the innerHTML of each `data-spa-island` element with the corresponding HTML from the envelope. The browser URL updates via `history.pushState`. The page title and meta tags update. Scroll position resets to the top (or to a hash target, if present).

The JSON envelope looks like this:

```json
{
  "title": "Pasta Carbonara",
  "description": "A Roman classic",
  "islands": {
    "content": "<article>...rendered HTML...</article>",
    "recipe-info": "<div class=\"card\">...sidebar HTML...</div>"
  }
}
```

Each key in the `islands` dictionary maps to a `data-spa-island` attribute in the DOM. The engine discovers all islands on the current page, matches them by name, and swaps their contents.

## The Core Types

Four types make up the server-side island system.

### IIslandRenderer

The contract every island renderer implements:

```csharp
public interface IIslandRenderer
{
    string IslandName { get; }
    Task<string> RenderAsync(ContentRoute route, RenderContext context);
}
```

`IslandName` must match the `data-spa-island` attribute in your layout. `RenderAsync` receives the target page's `ContentRoute` and a `RenderContext` with the site's base URL, title, and locale. Return an empty string to skip this island for the current route.

### RazorIslandRenderer&lt;TComponent&gt;

A base class that removes the boilerplate of rendering a Razor component to an HTML string:

```csharp
public abstract class RazorIslandRenderer<TComponent>(
    ComponentRenderer renderer) : IIslandRenderer
    where TComponent : IComponent
{
    public abstract string IslandName { get; }

    protected abstract Task<IDictionary<string, object?>?> BuildParametersAsync(
        ContentRoute route);

    public async Task<string> RenderAsync(ContentRoute route, RenderContext context)
    {
        var parameters = await BuildParametersAsync(route);
        if (parameters is null) return "";
        return await renderer.RenderComponentAsync<TComponent>(parameters);
    }
}
```

Override `BuildParametersAsync` to fetch data and return a parameter dictionary. Return `null` to skip rendering -- the base class converts that to an empty string.

### ComponentRenderer

Wraps Blazor's `HtmlRenderer` to turn any `IComponent` into an HTML string:

```csharp
public sealed class ComponentRenderer(
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory) : IAsyncDisposable
{
    public Task<string> RenderComponentAsync<TComponent>(
        IDictionary<string, object?>? parameters = null)
        where TComponent : IComponent;
}
```

Register it as scoped -- it holds a `HtmlRenderer` instance tied to a service scope and disposes it at scope end.

### SpaPageDataService

The coordinator. It iterates over all registered `IIslandRenderer` implementations, calls `RenderAsync` on each, and assembles the results into a `SpaEnvelopeDto`:

```csharp
public sealed class SpaPageDataService
{
    public async Task<SpaEnvelopeDto?> GetPageDataAsync(
        ContentRoute route, string title, string? description = null);
}
```

If no renderer produces content for a route, the method returns `null` and the SPA data endpoint returns 404.

## Tutorial: Adding SPA Navigation to a Recipe Site

This walkthrough builds SPA navigation for a recipe site with two islands: a main content area and a recipe info sidebar. The complete working example lives in the `examples/SpaNavigationExample` directory.

<Steps>
<Step stepNumber="1">
### Define Your Front Matter

Create a front matter record with the recipe-specific fields your site needs. This record drives both the content pipeline and the island renderers.

```csharp
using Penn.FrontMatter;

public record RecipeFrontMatter : IFrontMatter, IDescribable, IOrderable,
    ITaggable, IDraftable, ICrossReferenceable, ISectionable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public int PrepTime { get; init; }
    public int CookTime { get; init; }
    public int Servings { get; init; }
    public string Difficulty { get; init; } = "Easy";
    public string[] Tags { get; init; } = [];
    public bool IsDraft { get; init; }
    public string? Uid { get; init; }
    public string? RedirectUrl { get; init; }
    public string? Section { get; init; }
    public int Order { get; init; } = int.MaxValue;
}
```

The capability interfaces (`IDescribable`, `IOrderable`, etc.) tell Penn which pipeline features to apply. `PrepTime`, `CookTime`, `Servings`, and `Difficulty` are custom fields that the recipe info island renderer will use.
</Step>

<Step stepNumber="2">
### Create Razor Components for Islands

Each island needs a Razor component that renders its HTML. These components serve double duty: Blazor SSR uses them on the first page load, and `ComponentRenderer` uses them to produce the HTML fragments inside SPA envelopes.

**RecipeContent.razor** -- the main content area:

```razor
<header class="mb-6 lg:mb-8">
    <h1 class="text-2xl lg:text-3xl font-bold tracking-tight">@Title</h1>
</header>
<div class="prose max-w-full">
    @((MarkupString)HtmlContent)
</div>

@code {
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string HtmlContent { get; set; } = "";
}
```

**RecipeInfoCard.razor** -- the sidebar metadata card:

```razor
<div class="rounded-lg border p-4 text-sm">
    <h3 class="font-semibold mb-3">Recipe Info</h3>
    <dl class="space-y-2">
        <div class="flex justify-between">
            <dt>Prep time</dt>
            <dd>@PrepTime min</dd>
        </div>
        <div class="flex justify-between">
            <dt>Cook time</dt>
            <dd>@CookTime min</dd>
        </div>
        <div class="flex justify-between">
            <dt>Servings</dt>
            <dd>@Servings</dd>
        </div>
        <div class="flex justify-between">
            <dt>Difficulty</dt>
            <dd>@Difficulty</dd>
        </div>
    </dl>
</div>

@code {
    [Parameter] public int PrepTime { get; set; }
    [Parameter] public int CookTime { get; set; }
    [Parameter] public int Servings { get; set; }
    [Parameter] public string Difficulty { get; set; } = "Easy";
}
```

Keep components focused on presentation. Data fetching belongs in the renderer.
</Step>

<Step stepNumber="3">
### Write Island Renderers

Each renderer extends `RazorIslandRenderer<TComponent>`, fetches the data it needs from a content service, and maps that data to component parameters.

**RecipeContentSlotRenderer.cs:**

```csharp
public class RecipeContentSlotRenderer(
    ContentHelper contentHelper,
    ComponentRenderer renderer) : RazorIslandRenderer<RecipeContent>(renderer)
{
    public override string IslandName => "content";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(
        ContentRoute route)
    {
        var result = await contentHelper.GetPageByUrlAsync(
            route.CanonicalPath.Value);
        if (result is null) return null;

        return new Dictionary<string, object?>
        {
            [nameof(RecipeContent.Title)] = result.Value.FrontMatter.Title,
            [nameof(RecipeContent.HtmlContent)] = result.Value.Html,
        };
    }
}
```

**RecipeInfoSlotRenderer.cs:**

```csharp
public class RecipeInfoSlotRenderer(
    ContentHelper contentHelper,
    ComponentRenderer renderer) : RazorIslandRenderer<RecipeInfoCard>(renderer)
{
    public override string IslandName => "recipe-info";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(
        ContentRoute route)
    {
        var result = await contentHelper.GetPageByUrlAsync(
            route.CanonicalPath.Value);
        if (result is null) return null;

        var fm = result.Value.FrontMatter;

        // Index page has no recipe metadata.
        if (fm is { PrepTime: 0, CookTime: 0, Servings: 0 })
            return null;

        return new Dictionary<string, object?>
        {
            [nameof(RecipeInfoCard.PrepTime)] = fm.PrepTime,
            [nameof(RecipeInfoCard.CookTime)] = fm.CookTime,
            [nameof(RecipeInfoCard.Servings)] = fm.Servings,
            [nameof(RecipeInfoCard.Difficulty)] = fm.Difficulty,
        };
    }
}
```

`IslandName` on each renderer (`"content"`, `"recipe-info"`) must match the `data-spa-island` attribute in the layout. Returning `null` from `BuildParametersAsync` means "skip this island for this route" -- the recipe info renderer does this for the index page, which has no recipe metadata.
</Step>

<Step stepNumber="4">
### Wire Up the Layout

Add `data-spa-island` attributes to the DOM elements that should update during SPA navigation. Everything outside these elements stays untouched.

```razor
<div class="flex flex-1">
    <!-- Left sidebar: navigation (NOT an island -- stays put) -->
    <nav class="w-64 border-r flex-shrink-0">
        <div class="sticky top-0 h-screen overflow-y-auto p-6">
            <TableOfContentsNavigation TableOfContents="@_tableOfContents" />
        </div>
    </nav>

    <!-- Main content: SPA island -->
    <div class="flex-1 min-w-0 p-6 lg:p-10">
        <article data-spa-island="content" data-spa-loading="skeleton">
            @Body
        </article>
    </div>

    <!-- Right sidebar: SPA island -->
    <aside class="w-64 flex-shrink-0 p-6 hidden lg:block">
        <div class="sticky top-6"
             data-spa-island="recipe-info"
             data-spa-loading="clear">
            <SectionOutlet SectionName="recipe-info" />
        </div>
    </aside>
</div>
```

The `data-spa-loading` attribute controls what happens while the JSON fetch is in progress. Three modes are available:

| Mode | Behavior |
|------|----------|
| `"keep"` | Leave the previous content visible until new data arrives. This is the default if the attribute is omitted. |
| `"skeleton"` | Replace the island with a shimmer placeholder. Good for main content areas where stale content would be confusing. You can supply a custom skeleton via a `<template data-spa-skeleton-for="content">` element. |
| `"clear"` | Empty the island immediately. Useful for small sidebar widgets where a skeleton would be overkill. |

The skeleton is only shown if the fetch takes longer than 100ms (configurable via `data-spa-skeleton-delay` on the `<html>` element). Fast navigations -- especially prefetched ones -- skip the skeleton entirely.
</Step>

<Step stepNumber="5">
### Register Services

In `Program.cs`, register Penn core, then add SPA navigation and your island renderers:

```csharp
using Penn.Infrastructure;
using Penn.Islands;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "My Recipe Book";
    penn.SiteDescription = "A cookbook powered by SPA islands";
    penn.ContentRootPath = "Content";
    penn.AddMarkdownContent<RecipeFrontMatter>(md =>
    {
        md.ContentPath = "Content";
        md.BasePageUrl = "";
    });
});

// SPA navigation
builder.Services.AddSpaNavigation();
builder.Services.AddScoped<ComponentRenderer>();
builder.Services.AddTransient<IIslandRenderer, RecipeContentSlotRenderer>();
builder.Services.AddTransient<IIslandRenderer, RecipeInfoSlotRenderer>();

var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>();
app.UseSpaNavigation();
app.UsePenn();
await app.RunOrBuildAsync(args);
```

Three registrations matter for SPA navigation:

- **`AddSpaNavigation()`** registers `SpaPageDataService`, a `SpaNavigationContentService` (which generates `/_spa-data/{slug}.json` routes during static builds), and the SPA data endpoint.
- **`ComponentRenderer`** is registered as scoped. It wraps Blazor's `HtmlRenderer`, which must be tied to a service scope and disposed at scope end.
- **Each `IIslandRenderer`** is registered as transient. `SpaPageDataService` resolves all of them via `IEnumerable<IIslandRenderer>` and calls each one per request.

You can optionally configure the data path:

```csharp
builder.Services.AddSpaNavigation(options =>
{
    options.DataPath = "/_my-data"; // default is "/_spa-data"
});
```
</Step>

<Step stepNumber="6">
### Include Scripts and Run

Add the Penn UI script bundle to your `App.razor` (or `_Host.cshtml`). The SPA engine is part of this bundle:

```html
<script src="/_content/Penn.UI/scripts.js" defer></script>
```

The `defer` attribute ensures the script runs after the DOM is ready.

Start the dev server:

```bash
dotnet run
```

Navigate to `http://localhost:5000` and click between recipe pages. Open the Network tab -- you'll see small JSON fetches to `/_spa-data/pasta-carbonara.json` instead of full HTML page loads. The content area and sidebar swap instantly.

The SPA engine also prefetches on hover. When the cursor moves over an internal link, the engine fires a fetch for that page's JSON data. By the time the click lands, the data is already cached.

During static builds (`dotnet run -- build`), Penn generates a `.json` file alongside every `.html` file. The SPA engine works identically whether served from ASP.NET or from a static host.
</Step>
</Steps>

## Responding to Navigation Events

The SPA engine dispatches two lifecycle events on `document`, allowing you to hook into the navigation cycle from your own scripts.

### spa:before-navigate

Fired after the user clicks a link but before the fetch starts. Use it to tear down transient UI state -- close modals, cancel animations, or detach event listeners from the outgoing content.

```javascript
document.addEventListener('spa:before-navigate', (e) => {
    const { url, slug } = e.detail;
    // Clean up before the page changes
    closeAllModals();
});
```

### spa:commit

Fired after the new island HTML has been injected into the DOM. Use it to reinitialize interactive features that depend on the content -- syntax highlighting, copy-to-clipboard buttons, scroll-linked animations.

```javascript
document.addEventListener('spa:commit', (e) => {
    const { url, slug, data } = e.detail;
    // data is the full SpaEnvelopeDto
    initCopyButtons();
    highlightCodeBlocks();
});
```

The `data` property contains the full envelope, including `title`, `description`, and the `islands` dictionary.

## How DocSite Uses This

If you're using `Penn.DocSite`, SPA navigation is already wired up. `AddDocSite()` calls `AddSpaNavigation()`, registers a `ComponentRenderer`, and adds a `DocSiteArticleSlotRenderer` that renders the `"content"` island. The `MainLayout.razor` in DocSite marks its `<article>` element with `data-spa-island="content"` and `data-spa-loading="skeleton"`.

You don't need any manual setup. The tutorial above exists so you can understand the mechanism, add extra islands to a DocSite layout, or build SPA navigation for a custom Penn project.

See [Using the DocSite Package](xref:penn.guides.using-docsite) for details on what DocSite provides out of the box.

## Fallback Behavior

If the fetch to `/_spa-data/{slug}.json` returns a non-200 status -- because the target page is a custom Razor page without island renderers, or because the route doesn't exist -- the engine abandons the SPA navigation and performs a normal full-page load via `location.href`. No error dialog, no broken state.

This means you can mix SPA-navigable content pages with traditional Razor pages in the same site. Pages that have island data get instant transitions. Pages that don't get a standard browser navigation.

The same applies when no `data-spa-island` elements are found in the current DOM. If the layout doesn't declare any islands, every link click falls through to the browser's default behavior.

## Further Reading

- [Razor Islands Reference](xref:penn.reference.razor-islands) -- full API reference for all island types, the SPA envelope format, and configuration options
- [SPA Island Architecture](xref:penn.under-the-hood.spa-island-architecture) -- the internal design: how the two rendering paths share components, how prefetching works, and how static builds produce JSON alongside HTML
- [Using the DocSite Package](xref:penn.guides.using-docsite) -- how DocSite wraps SPA navigation so you don't have to configure it yourself
