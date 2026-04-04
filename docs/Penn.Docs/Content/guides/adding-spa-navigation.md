---
title: "Adding SPA Navigation"
description: "Add instant page transitions to your Penn site with the island renderer architecture"
uid: "penn.guides.adding-spa-navigation"
order: 2520
---

Out of the box, Penn generates static HTML. Every link click triggers a full page load. That's fine for most sites, but for content-heavy pages with sidebar navigation it feels sluggish -- like opening a new book every time you turn a page.

SPA navigation fixes this. The first visit loads full static HTML (fast, no JavaScript required). After that, clicking an internal link fetches a small JSON envelope containing pre-rendered HTML for each named "island" in your layout, and the SPA engine swaps the content in place. No full reload, no flash of unstyled content, no existential dread about your bundle size.

## The Architecture

Penn's SPA system is built on three types:

- <xref:T:Penn.Islands.IIslandRenderer> -- the interface every island renderer implements
- <xref:T:Penn.Islands.RazorIslandRenderer`1> -- a base class for renderers that produce HTML from a Razor component
- <xref:T:Penn.Islands.SpaPageDataService> -- coordinates all registered renderers and assembles the JSON envelope
- <xref:T:Penn.Islands.ComponentRenderer> -- renders Blazor components to HTML strings via `HtmlRenderer`

### The Flow

1. User clicks a link. The SPA engine intercepts it.
2. The engine fetches `/_spa-data/{slug}.json` from the server.
3. <xref:T:Penn.Islands.SpaPageDataService> iterates over all registered `IIslandRenderer` implementations.
4. Each renderer receives a `ContentRoute` and `RenderContext`, produces HTML (or returns empty to skip).
5. The service assembles a `SpaEnvelopeDto` with the page title and a dictionary of `{ islandName: html }`.
6. The SPA engine replaces the contents of each `data-spa-island` element in the DOM.

## The IIslandRenderer Interface

This is the contract. Every island renderer implements it:

```csharp
public interface IIslandRenderer
{
    string IslandName { get; }
    Task<string> RenderAsync(ContentRoute route, RenderContext context);
}
```

`IslandName` must match the `data-spa-island` attribute in your layout HTML. `RenderAsync` receives the `ContentRoute` for the target page (including `CanonicalPath`, `Locale`, etc.) and a `RenderContext` with the site's base URL and title.

Returning an empty string means "I have nothing for this route" -- the island will be left alone or cleared depending on the `data-spa-loading` strategy.

## Using RazorIslandRenderer

Most island renderers render a Razor component. <xref:T:Penn.Islands.RazorIslandRenderer`1> handles the boilerplate:

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

You override `BuildParametersAsync` to fetch your data and return a parameter dictionary. Return `null` to skip rendering entirely. The base class calls <xref:T:Penn.Islands.ComponentRenderer> to turn your Razor component into an HTML string.

## Tutorial: Adding SPA Navigation to a Recipe Site

Let's wire up SPA navigation for a recipe site with two islands: a main content area and a recipe info sidebar.

### 1. Define Your Front Matter

```csharp
public class RecipeFrontMatter : IFrontMatter
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? Uid { get; set; }
    public int Order { get; set; }
    public bool IsDraft { get; set; }
    
    // Recipe-specific fields
    public string? PrepTime { get; set; }
    public string? CookTime { get; set; }
    public int Servings { get; set; }
    public string? Difficulty { get; set; }
}
```

### 2. Create Razor Components for Islands

These components render both the initial SSR page and the SPA updates. One component, two rendering paths:

**RecipeContent.razor**

```razor
<article class="prose dark:prose-invert">
    <h1>@Title</h1>
    @((MarkupString)HtmlContent)
</article>

@code {
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string HtmlContent { get; set; } = "";
}
```

**RecipeInfoCard.razor**

```razor
<div class="rounded-lg border p-4">
    <dl>
        <dt>Prep Time</dt><dd>@PrepTime</dd>
        <dt>Cook Time</dt><dd>@CookTime</dd>
        <dt>Servings</dt><dd>@Servings</dd>
        <dt>Difficulty</dt><dd>@Difficulty</dd>
    </dl>
</div>

@code {
    [Parameter] public string? PrepTime { get; set; }
    [Parameter] public string? CookTime { get; set; }
    [Parameter] public int Servings { get; set; }
    [Parameter] public string? Difficulty { get; set; }
}
```

### 3. Write Island Renderers

Each renderer fetches data for a `ContentRoute` and maps it to component parameters.

**RecipeContentSlotRenderer.cs**

```csharp
public class RecipeContentSlotRenderer(
    ContentResolver contentResolver,
    ComponentRenderer renderer) : RazorIslandRenderer<RecipeContent>(renderer)
{
    public override string IslandName => "content";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(
        ContentRoute route)
    {
        var page = await contentResolver.GetByUrlAsync(route.CanonicalPath.Value);
        if (page is null) return null;

        return new Dictionary<string, object?>
        {
            [nameof(RecipeContent.Title)] = page.Title,
            [nameof(RecipeContent.HtmlContent)] = page.Html,
        };
    }
}
```

**RecipeInfoSlotRenderer.cs**

```csharp
public class RecipeInfoSlotRenderer(
    ContentResolver contentResolver,
    ComponentRenderer renderer) : RazorIslandRenderer<RecipeInfoCard>(renderer)
{
    public override string IslandName => "recipe-info";

    protected override async Task<IDictionary<string, object?>?> BuildParametersAsync(
        ContentRoute route)
    {
        var page = await contentResolver.GetByUrlAsync(route.CanonicalPath.Value);
        if (page?.FrontMatter is not RecipeFrontMatter fm) return null;

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

Note how each renderer's `IslandName` (`"content"`, `"recipe-info"`) corresponds to a `data-spa-island` attribute in the layout.

### 4. Wire Up the Layout

Add `data-spa-island` attributes to the elements that should update during SPA navigation:

```razor
<div class="flex">
    <main data-spa-island="content" data-spa-loading="skeleton">
        <RecipeContent Title="@title" HtmlContent="@html" />
    </main>

    <aside data-spa-island="recipe-info" data-spa-loading="clear">
        <RecipeInfoCard PrepTime="@prepTime" CookTime="@cookTime"
                        Servings="@servings" Difficulty="@difficulty" />
    </aside>
</div>
```

The `data-spa-loading` attribute controls what happens while the fetch is in progress:

- `"skeleton"` -- shows a shimmer placeholder (good for main content areas)
- `"clear"` -- empties the island immediately (good for small sidebar widgets)

### 5. Register Services

In `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPenn(penn =>
{
    penn.SiteTitle = "Recipe Book";
    penn.AddMarkdownContent<RecipeFrontMatter>(md =>
    {
        md.ContentPath = "Content/recipes";
        md.BasePageUrl = "/recipes";
    });
});

// SPA navigation
builder.Services.AddSpaNavigation();
builder.Services.AddScoped<ComponentRenderer>();
builder.Services.AddTransient<IIslandRenderer, RecipeContentSlotRenderer>();
builder.Services.AddTransient<IIslandRenderer, RecipeInfoSlotRenderer>();

var app = builder.Build();

app.UsePenn();
app.UseSpaNavigation();

await app.RunOrBuildAsync(args);
```

Key registrations:

- `AddSpaNavigation()` registers <xref:T:Penn.Islands.SpaPageDataService> and maps the `/_spa-data/{slug}` endpoint.
- Each `IIslandRenderer` is registered as transient -- the `SpaPageDataService` iterates over all of them per request.
- `ComponentRenderer` is scoped because it wraps Blazor's `HtmlRenderer`, which is tied to a service scope.

### 6. Include the SPA Script

In your `App.razor`:

```html
<script src="/_content/Penn.UI/scripts.js" defer></script>
```

The SPA engine is included in the main script bundle. It intercepts link clicks, fetches the JSON envelope, and swaps island content.

### 7. Run It

```bash
dotnet watch
```

Click between recipes. The content and sidebar swap instantly. Open the Network tab -- you'll see small JSON fetches to `/_spa-data/recipes--pasta-carbonara.json` instead of full HTML page loads.

## Responding to Navigation Events

The SPA engine fires custom events on `document`:

```javascript
// After new content is injected
document.addEventListener('spa:commit', (e) => {
    // e.detail contains { url, data }
    // Reinitialise any interactive features
    window.pageManager?.syntaxHighlighter?.init();
});

// Before the fetch starts
document.addEventListener('spa:before-navigate', (e) => {
    // Good place to tear down transient UI state
});
```

## How DocSite Uses This

For a real-world example, Penn.DocSite wires up SPA navigation internally. Its `AddDocSite()` method calls `AddSpaNavigation()`, registers a `ComponentRenderer`, and adds a `DocSiteArticleSlotRenderer` that renders the article content island. You get SPA navigation without any manual setup. The pattern is identical to what's shown here -- DocSite just does it for you.

## Fallback Behavior

If the SPA data endpoint returns 404 (the page doesn't have island data -- maybe it's a custom Razor page), the SPA engine falls back to a normal full-page navigation. No error, no broken state. Pages that can be SPA-navigated are; pages that can't aren't. It's not clever, but it's reliable.
