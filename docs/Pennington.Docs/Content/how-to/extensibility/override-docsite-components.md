---
title: "Customize DocSite layouts and components"
description: "Replace a Pennington.UI component by registering your own in DI, override slots, and add content to AdditionalHtmlHeadContent."
section: "extensibility"
order: 70
tags: []
uid: how-to.extensibility.override-docsite-components
isDraft: true
search: false
llms: false
---

> **In this page.** The honest surface DocSite exposes for customization: `AdditionalHtmlHeadContent`, `ExtraStyles`, the string-HTML slots (`HeaderIcon`, `HeaderContent`, `FooterContent`), overriding specific URLs via `AdditionalRoutingAssemblies`, and replacing the SPA `"content"` island renderer for client-side navigation.
>
> **Not in this page.** Forking DocSite wholesale — if you need to swap Razor components inside `MainLayout.razor` (`TableOfContentsNavigation`, `OutlineNavigation`, `LanguageSwitcher`), drop `AddDocSite`/`UseDocSite` and compose the pieces yourself with plain `AddPennington` + `UsePennington`. There is no DI seam for substituting those components.

## When to use this

- Outline bullet: You already have a DocSite wired with `AddDocSite` + `UseDocSite` + `RunDocSiteAsync` and want to bolt on scripts/stylesheets/fonts (`AdditionalHtmlHeadContent`), add a floating widget (`ExtraStyles` or custom `IResponseProcessor`), carve out a specific URL with your own Razor page, or replace what the SPA client sees when it navigates between pages.
- Outline bullet: You tried this page expecting DI-based substitution of individual Razor components in the DocSite layout and need the real story — see the prominent note under each step.

## Assumptions

- Outline bullet: You have a working DocSite (see `/tutorials/getting-started/first-site` if not).
- Outline bullet: You can edit `Program.cs` and have a `new DocSiteOptions { ... }` factory in place.
- Outline bullet: You understand that `DocSiteOptions` is verified at `src/Pennington.DocSite/DocSiteOptions.cs` and that the layout lives at `src/Pennington.DocSite/Components/Layout/MainLayout.razor` — it is not configurable at runtime.
- Outline bullet: To copy a working setup, see [`examples/SearchExample`](https://github.com/usepennington/pennington/tree/main/examples/SearchExample) (most override knobs set in one place — head content, styles, routing assembly, custom `@page`). Do not walk the example end-to-end; this is a recipe.

---

## Steps

### 1. Inject `<head>` content with `AdditionalHtmlHeadContent`

- Outline bullet: Set `AdditionalHtmlHeadContent` on `DocSiteOptions` to a raw HTML string. The DocSite layout (`src/Pennington.DocSite/Components/App.razor`) emits it verbatim inside `<head>`, after the default `<link rel="stylesheet">` and Pennington.UI scripts and before the `<HeadOutlet/>` marker.
- Outline bullet: Typical payloads: Google Fonts preconnect + stylesheet, analytics tags, Open Graph meta overrides, custom favicons. Use a C# raw string literal (`"""..."""`) so you do not have to escape quotes.
- Outline bullet: This is a string slot — no Razor component resolution happens. If you need dynamic values, interpolate them in `Program.cs` before passing the string.
- Outline bullet: Example — fonts and preconnect tags:

```csharp:path
examples/SearchExample/Program.cs
```

### 2. Add CSS with `ExtraStyles` and preload fonts with `FontPreloads`

- Outline bullet: `ExtraStyles` is a raw CSS string; MonorailCSS appends it to the generated stylesheet via `MonorailCssOptions.ExtraStyles`. Use it for utility-class gaps, brand-specific selectors, or `@font-face` rules.
- Outline bullet: `FontPreloads` is a `FontPreload[]`; each entry renders a `<link rel="preload" as="font" type="..." crossorigin>` before the main stylesheet. Use this when you host font files yourself and want the browser to fetch them in parallel with HTML parsing.
- Outline bullet: Use `BodyFontFamily` / `DisplayFontFamily` to point the compiled CSS at the families you just loaded — they feed MonorailCSS's theme variables.

### 3. Override a specific URL with your own Razor `@page`

- Outline bullet: The DocSite ships a catch-all page at `/{*fileName:nonfile}` (see `src/Pennington.DocSite/Components/Layout/Pages.razor`). Blazor's router prefers more specific `@page` directives, so a custom Razor component with a concrete route wins over the catch-all for that URL.
- Outline bullet: Put your `.razor` file in your app project with a route directive (e.g. `@page "/random/{*fileName:nonfile}"`), then point `DocSiteOptions.AdditionalRoutingAssemblies` at the assembly containing it. The DocSite router (`Routes.razor`) passes those assemblies to `<Router AdditionalAssemblies="...">`.
- Outline bullet: Your component renders inside `MainLayout` — you get the full shell (header, sidebar, outline nav). If you need a different shell for these pages, set a `@layout` directive on the component.
- Outline bullet: Example — the `Random` razor page registered via `AdditionalRoutingAssemblies = [typeof(Random).Assembly]`:

```razor:path
examples/SearchExample/Services/Random.razor
```

- Outline bullet: See `examples/SearchExample/Program.cs` for the matching `DocSiteOptions.AdditionalRoutingAssemblies = [typeof(Random).Assembly]` wiring.

### 4. Replace the SPA "content" island renderer (client-side navigation only)

- Outline bullet: DocSite registers an `IIslandRenderer` named `"content"` (`Pennington.DocSite.Slots.DocSiteArticleSlotRenderer`) that feeds the SPA engine when the user clicks a link and the page navigates without a full reload.
- Outline bullet: To swap what the client-side engine renders into the `<article data-spa-island="content">` slot, register your own `IIslandRenderer` with `IslandName => "content"` **after** `AddDocSite`. `Pennington.Islands.SpaPageDataService` writes island HTML into a dictionary keyed by `IslandName` (`islands[renderer.IslandName] = html;`), so the last renderer registered for that name wins.
- Outline bullet: **Caveat — this does not change the initial server render.** `Pages.razor` calls `<DocSiteArticle>` directly, not through the island system. Your override takes effect only on subsequent SPA navigations; the first page load still goes through the built-in `DocSiteArticle` component.
- Outline bullet: Subclass `RazorIslandRenderer<TComponent>` for the common case of rendering a Razor component with a parameter dictionary. The base class resolves `ComponentRenderer` from DI and handles the SSR plumbing.
- Outline bullet: Example pattern — a custom content island renderer (from a non-DocSite SPA sample):

```csharp:xmldocid
T:SpaNavigationExample.Slots.RecipeContentSlotRenderer
```

- Outline bullet: In `Program.cs`, register **after** `AddDocSite`: `builder.Services.AddTransient<IIslandRenderer, MyContentSlotRenderer>();`. Do not call `penn.Islands.Register<T>("content")` inside the `AddDocSite` factory — that API belongs to raw `AddPennington` wiring.

### 5. Add a floating widget or inject markup before `</body>` with `IResponseProcessor`

- Outline bullet: DocSite runs every response through `ResponseProcessingMiddleware`, which sorts `IResponseProcessor` implementations by their `Order`. Register your own to inject a cookie banner, feedback button, or analytics tag that needs the post-render HTML.
- Outline bullet: Pick an `Order` above the built-ins (10/20/30) and below `LocaleLinkHtmlRewriter` concerns; 500 is a safe "user widget" band — see `examples/ForgePortalExample/FeedbackWidgetProcessor.cs`.
- Outline bullet: If you only need to append one string to the head, prefer `AdditionalHtmlHeadContent` (step 1). Reach for `IResponseProcessor` when you need access to `HttpContext` (request-scoped data, feature flags, diagnostics) or need to mutate the body.
- Outline bullet: See the dedicated recipe at `/how-to/extensibility/response-processor` for the full interface contract.

### 6. If none of these slots are enough, fork to `AddPennington`

If these slots aren't enough, drop `AddDocSite`/`UseDocSite` and compose the pipeline yourself with `AddPennington` + `UsePennington`. You keep the engine (Markdig pipeline, response processors, search, llms.txt, sitemap, localization, feeds) but have to hand-roll the shell. See `examples/SpaNavigationExample/Program.cs` and `examples/MultipleContentSourceExample/Program.cs` for hand-rolled starting points.

---

## Verify

- Outline bullet: `dotnet run` the site and view source on any page — confirm your `AdditionalHtmlHeadContent` markup appears inside `<head>` after the default `<link rel="stylesheet" href="/styles.css">` line and before `<HeadOutlet/>`.
- Outline bullet: Navigate to a URL handled by your custom `@page` (step 3) — confirm the page title, content, and that the DocSite shell (header, sidebar) still wraps it.
- Outline bullet: For a custom `IIslandRenderer` (step 4), open DevTools Network and click an internal link — confirm the `/_spa-data/*.json` response contains your island's HTML under the `"content"` key.

## Related

- Reference: [`DocSiteOptions`](/reference/options/docsite-options) — every field on `DocSiteOptions` with types and defaults.
- Reference: [Island rendering interfaces](/reference/extension-points/islands) — `IIslandRenderer`, `RazorIslandRenderer<T>`, `SpaEnvelope`.
- Background: [The response-processing pipeline](/explanation/core/response-processing) — when to reach for `IResponseProcessor` vs `IHtmlResponseRewriter`.
