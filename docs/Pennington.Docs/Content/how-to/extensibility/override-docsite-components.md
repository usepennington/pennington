---
title: "Customize DocSite layouts and components"
description: "Inject head content, extra CSS, header/footer HTML, and extra @page assemblies through DocSiteOptions without forking the template."
uid: how-to.extensibility.override-docsite-components
order: 203070
sectionLabel: Extensibility
tags: [docsite, theming, slots, routing]
---

Use these seams when starting from `AddDocSite` and branding the chrome — head tags, header/footer HTML, extra CSS, or one or two additional routed pages — without giving up the bundled layout, content pipeline, SPA navigation, and MonorailCSS wiring. To replace the article content island itself, follow <xref:how-to.extensibility.island-renderer> instead. To rearrange the layout shell fundamentally, read <xref:explanation.core.docsite-positioning> before deciding whether `AddDocSite` is still the right starting point.

## Assumptions

- An existing Pennington site wired through `AddDocSite(...)` (see the <xref:tutorials.getting-started.first-site> tutorial if not).
- Edits made in the `DocSiteOptions` factory passed to `AddDocSite`, not the DocSite source — forking the template is out of scope (see <xref:explanation.core.docsite-positioning>).
- Awareness that `ExtraStyles` is appended to the generated `/styles.css`, so rules added there ship alongside the MonorailCSS utility output rather than as a separate stylesheet.
- Awareness that these seams are set at host-build time — changes require a restart, or `dotnet watch` for hot-reload.

For a working setup, see `examples/DocSiteChromeOverridesExample`. `SiteChromeOverrides.cs` returns a populated `DocSiteOptions` exercising all four seams, `Components/ExtraHeadFragment.razor` backs the head-slot fragment, and `Components/ExtraPage.razor` is the routed `@page` component showing that `AdditionalRoutingAssemblies` widened the router. `Program.cs` runs the DocSite end-to-end against those overrides.

---

## Steps

### 1. Build a populated `DocSiteOptions`

The whole code surface for this recipe lives in one factory method, so the four seams sit together on a single record initializer. The example sets `SiteTitle` and `Description` alongside the override seams, matching the shape produced by `AddDocSite(() => SiteChromeOverrides.BuildDocSiteOptions())`.

```csharp:xmldocid
M:DocSiteChromeOverridesExample.SiteChromeOverrides.BuildDocSiteOptions
```

### 2. Inject tags into `<head>` via `AdditionalHtmlHeadContent`

`AdditionalHtmlHeadContent` is a raw HTML string rendered inside every page's `<head>`, making it the right seam for meta tags, preconnect hints, analytics snippets, and font `<link>` elements that MonorailCSS does not know about. To author the fragment as a Razor component instead, render it with `ToHtmlString()` once at startup and pass the resulting string — the example pairs `SiteChromeOverrides.BuildHtmlHeadContent` with `Components/ExtraHeadFragment.razor` so both shapes sit side by side.

```csharp:xmldocid,bodyonly
M:DocSiteChromeOverridesExample.SiteChromeOverrides.BuildHtmlHeadContent
```

### 3. Append rules to the generated stylesheet via `ExtraStyles`

`ExtraStyles` is a CSS string concatenated onto the MonorailCSS-generated `/styles.css`, making it the right home for `@font-face` declarations, custom-property overrides, and any selector the utility-class scanner will not discover on its own. Keep this string small — anything expressible as MonorailCSS utilities in Razor markup gets picked up automatically by `CssClassCollectorProcessor`.

```csharp:xmldocid,bodyonly
M:DocSiteChromeOverridesExample.SiteChromeOverrides.BuildExtraStyles
```

### 4. Replace the header and footer HTML with the string slots

`HeaderContent` and `FooterContent` are raw HTML strings the DocSite layout splices into the top bar and footer regions — they accept anything an HTML fragment can hold, from a branded logo wordmark to a compliance notice. Because they are strings, no `AdditionalRoutingAssemblies` entry is needed for them; for a component-authored fragment, render it to HTML at startup the same way as the head snippet above.

```csharp
var options = new DocSiteOptions
{
    HeaderContent = "<span class=\"chrome-header\">Extensibility Lab</span>",
    FooterContent = "<span class=\"chrome-footer\">(c) 2026 Pennington</span>",
    // ...
};
```

### 5. Route your own `@page` components via `AdditionalRoutingAssemblies`

The DocSite shell only discovers `@page` directives in its own assembly by default; adding the host assembly to `AdditionalRoutingAssemblies` makes any `@page "/route"` component in that assembly routable alongside the bundled pages. The example returns `[typeof(SiteChromeOverrides).Assembly]` so a Razor component like `ExtraPage.razor` sitting next to `Program.cs` gets picked up without any additional DI wiring.

```csharp:xmldocid,bodyonly
M:DocSiteChromeOverridesExample.SiteChromeOverrides.BuildAdditionalRoutingAssemblies
```

### 6. Wire the options into `AddDocSite`

`AddDocSite` takes a `Func<DocSiteOptions>` factory, so the most direct wiring is to pass the helper as a method reference and keep the host file short. The example's `Program.cs` runs this exact shape end-to-end.

```csharp:path
examples/DocSiteChromeOverridesExample/Program.cs
```

### 7. Replace the content island through the islands system

To swap the article body itself — the island whose `IslandName` is `"content"` — follow <xref:how-to.extensibility.island-renderer> and register an `IIslandRenderer` with `IslandName => "content"` to displace the shipped `DocSiteArticleSlotRenderer`. This seam lives in the islands system rather than `DocSiteOptions`, which is why it is a separate recipe.

---

## Verify

- Run `dotnet run` and view page source on `/` — expect the `<meta name="x-extensibility-lab-head">` tag inside `<head>`, your `HeaderContent` and `FooterContent` markup in the layout, and the `.chrome-header` rule inside `/styles.css`.
- Navigate to a route defined by a Razor component in your app assembly (for example `/extra`) and confirm it renders. A 404 here means `AdditionalRoutingAssemblies` is not including the right assembly.
- Run `dotnet run -- build output` and search `output/index.html` for your head fragment and `output/styles.css` for your `ExtraStyles` rules to confirm the overrides survive publish.

## Related

- Reference: <xref:reference.options.docsite-options>
- Background: <xref:explanation.core.docsite-positioning>
- Related how-to: <xref:how-to.extensibility.island-renderer>
