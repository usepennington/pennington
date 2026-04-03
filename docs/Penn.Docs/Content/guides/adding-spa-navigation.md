---
title: "Adding SPA Navigation"
description: "Add instant page transitions to your content site with the Razor Island system"
uid: "docs.guides.adding-spa-navigation"
order: 2520
---

Out of the box, MyLittleContentEngine generates static HTML — every link click triggers a full page load. That's
fast enough for most sites, but for content-heavy sites with sidebar navigation it can feel sluggish. SPA
navigation fixes this by fetching only the page data on subsequent clicks and swapping content in-place, while
keeping the first page load as fast static HTML.

In this tutorial, you'll add SPA navigation to a recipe site. The same approach works for documentation,
blogs, or any content site.

## What You'll Build

- A recipe site where clicking between recipes swaps content instantly — no full page reload
- Named **islands** in the layout that update independently (article content + a recipe info sidebar)
- Razor components that render both the initial static page and SPA updates from a single source of truth

## How It Works

The first visit to your site loads full static HTML — normal browser behaviour. After that, clicking an
internal link fetches a small JSON file containing pre-rendered HTML for each named island, and the SPA engine
swaps the content without a reload. If the JSON isn't available (custom Razor pages, external links), it
falls back to a normal page load gracefully.

## Prerequisites

- A working MyLittleContentEngine site (see [Creating Your First Site](xref:docs.getting-started.creating-first-site))
- Familiarity with Blazor components and front matter

<Steps>
<Step stepNumber="1">

## Define Your Front Matter

Create a front matter class with the fields your site needs. For a recipe site, that includes cooking
metadata alongside the standard `IFrontMatter` properties:

```csharp:path
examples/SpaNavigationExample/RecipeFrontMatter.cs
```

The `PrepTime`, `CookTime`, `Servings`, and `Difficulty` fields are recipe-specific — your site would have
whatever domain fields make sense. The SPA system doesn't care about the shape of your front matter; it just
needs `IFrontMatter` for the base contract.

</Step>
<Step stepNumber="2">

## Create Razor Components for Your Islands

Each island needs a Razor component that produces the HTML to inject. These components are normal `.razor` files
with `[Parameter]` properties — no special base class or interface needed.

First, the main content component that renders the article title and markdown body:

```razor:path
examples/SpaNavigationExample/Slots/Components/RecipeContent.razor
```

Then a sidebar component for recipe metadata:

```razor:path
examples/SpaNavigationExample/Slots/Components/RecipeInfoCard.razor
```

> [!TIP]
> These same components are used by both the initial SSR page load and the Razor Island system. One
> component, two rendering paths — no duplicated markup.

</Step>
<Step stepNumber="3">

## Write Island Renderers

An island renderer connects your data to your Razor component. It fetches the content page, extracts the data
it needs, and returns a parameter dictionary. The base class `RazorIslandRenderer<TComponent>` handles the
actual rendering via Blazor's `HtmlRenderer`.

The content island renderer fetches the markdown content and passes it to the `RecipeContent` component:

```csharp:path
examples/SpaNavigationExample/Slots/RecipeContentSlotRenderer.cs
```

The sidebar island renderer reads recipe-specific front matter fields:

```csharp:path
examples/SpaNavigationExample/Slots/RecipeInfoSlotRenderer.cs
```

Each renderer declares an `IslandName` — this must match the `data-spa-island` attribute you'll add to the layout
in the next step.

</Step>
<Step stepNumber="4">

## Wire Up the Layout

Add `data-spa-island` attributes to the layout elements that should update during SPA navigation. The
attribute value must match the `IslandName` from your renderers.

```razor:path
examples/SpaNavigationExample/Components/Layout/MainLayout.razor
```

The key attributes:

- `data-spa-island="content"` marks the article area — the SPA engine replaces its contents on navigation
- `data-spa-island="recipe-info"` marks the sidebar — same treatment
- `data-spa-loading="skeleton"` shows a shimmer placeholder while slow fetches complete
- `data-spa-loading="clear"` empties the island immediately (good for small sidebar elements)

</Step>
<Step stepNumber="5">

## Use the Components in Your SSR Page

Your `Pages.razor` renders the initial static HTML. Use the same Razor components you created for the island
renderers:

```razor:path
examples/SpaNavigationExample/Components/Layout/Pages.razor
```

Notice `RecipeContent` and `RecipeInfoCard` appear here — the same components the island renderers use. Change
the component once, both SSR and SPA update.

</Step>
<Step stepNumber="6">

## Register Services and Include Scripts

In `Program.cs`, chain `.WithSpaNavigation<T>()` after your markdown content service and register your island
renderers:

```csharp:path
examples/SpaNavigationExample/Program.cs
```

Then include the SPA engine script in your `App.razor` (after the main `scripts.js`):

```razor:path
examples/SpaNavigationExample/Components/App.razor
```

The `spa-engine.js` script handles link interception, fetch racing, skeleton display, view transitions,
history management, and scroll position — all driven by the `data-spa-island` attributes in your layout.

</Step>
<Step stepNumber="7">

## Add Some Content

Create a few markdown files with your front matter fields. Here's an example recipe:

```markdown:path
examples/SpaNavigationExample/Content/pasta-carbonara.md
```

</Step>
<Step stepNumber="8">

## Run It

```bash
dotnet watch
```

Navigate to your site and click between recipes. The content and sidebar swap instantly without a full page
reload. Open the browser's Network tab — you'll see small `.json` fetches instead of full HTML page loads.

</Step>
</Steps>

## What Success Looks Like

When you click a recipe link in the sidebar:

1. The article content swaps to the new recipe (title, instructions, ingredients)
2. The sidebar card updates with the new recipe's prep time, cook time, and servings
3. The URL updates in the address bar and browser back/forward works
4. The first visit to any page still loads full static HTML — no JavaScript required for the initial render

If a fetch takes longer than 100ms (slow network, cold cache), you'll see a shimmer skeleton in the content
area that holds for at least 250ms to avoid a flash.

## Responding to Navigation Events

The SPA engine fires `spa:commit` on `document` after new content is injected. Use this to reinitialise
interactive features:

```javascript
document.addEventListener('spa:commit', (e) => {
    // e.detail contains { url, data }
    window.pageManager?.syntaxHighlighter?.init();
    window.pageManager?.tabManager?.init();
});
```

A `spa:before-navigate` event fires before the fetch starts, useful for clearing transient UI state.

## Next Steps

- [Razor Islands Reference](xref:docs.reference.razor-islands) — full API reference for interfaces, data
  attributes, JSON envelope, and lifecycle events
- [Using DocSite](xref:docs.getting-started.using-docsite) — DocSite uses SPA navigation out of the box with
  built-in content managers and outline rebuilding
- [Custom Content Service](xref:docs.guides.custom-content-service) — build an island renderer backed by a
  non-markdown content source
