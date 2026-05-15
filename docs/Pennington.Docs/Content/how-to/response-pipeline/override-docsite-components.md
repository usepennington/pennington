---
title: "Customize the DocSite chrome through DocSiteOptions"
description: "Use DocSiteOptions to inject head content, append CSS, replace the header/footer HTML, and route extra @page components without forking the template."
uid: how-to.response-pipeline.override-docsite-components
order: 210030
sectionLabel: "Response Pipeline"
tags: [docsite, theming, slots, routing]
---

To replace the bundled DocSite header or footer (or inject head tags, append CSS, route additional `@page` components) without forking the template, populate the four slot seams on `DocSiteOptions`. The bundled layout, content pipeline, SPA navigation, and MonorailCSS wiring keep working. To rearrange the layout shell fundamentally, read <xref:explanation.positioning.docsite-positioning> before deciding whether `AddDocSite` is still the right starting point.

## Before you begin

- An existing Pennington site wired through `AddDocSite(...)` (see <xref:tutorials.docsite.scaffold> if not).
- Edits made in the `DocSiteOptions` factory passed to `AddDocSite`, not the DocSite source — forking the template is out of scope (see <xref:explanation.positioning.docsite-positioning>).
- Awareness that `ExtraStyles` is appended to the generated `/styles.css`, so rules added there ship alongside the MonorailCSS utility output rather than as a separate stylesheet.
- Awareness that these seams are set at host-build time — changes require a restart, or `dotnet watch` for hot-reload.

For a working setup, see `examples/DocSiteChromeOverridesExample`. `SiteChromeOverrides.cs` returns a populated `DocSiteOptions` exercising all four seams, `Components/ExtraHeadFragment.razor` backs the head-slot fragment, and `Components/ExtraPage.razor` is the routed `@page` component showing that `AdditionalRoutingAssemblies` widened the router. `Program.cs` runs the DocSite end-to-end against those overrides.

## Build the populated options

The whole code surface for this recipe lives in one factory method, so the four seams sit together on a single record initializer. The example sets `SiteTitle` and `Description` alongside the override seams, matching the shape produced by `AddDocSite(() => SiteChromeOverrides.BuildDocSiteOptions())`.

```csharp:xmldocid
M:DocSiteChromeOverridesExample.SiteChromeOverrides.BuildDocSiteOptions
```

### Inject tags into `<head>` via `AdditionalHtmlHeadContent`

`AdditionalHtmlHeadContent` is a raw HTML string rendered inside every page's `<head>`, making it the right seam for meta tags, preconnect hints, analytics snippets, and font `<link>` elements that MonorailCSS does not know about. To author the fragment as a Razor component instead, render it with `ToHtmlString()` once at startup and pass the resulting string — the example pairs `SiteChromeOverrides.BuildHtmlHeadContent` with `Components/ExtraHeadFragment.razor` so both shapes sit side by side.

```csharp:xmldocid,bodyonly
M:DocSiteChromeOverridesExample.SiteChromeOverrides.BuildHtmlHeadContent
```

### Append rules to the generated stylesheet via `ExtraStyles`

`ExtraStyles` is a CSS string concatenated onto the MonorailCSS-generated `/styles.css`, making it the right home for `@font-face` declarations, custom-property overrides, and any selector the utility-class scanner will not discover on its own. Keep this string small — anything expressible as MonorailCSS utilities in Razor markup gets picked up automatically by the `MonorailCss.Discovery` pipeline.

```csharp:xmldocid,bodyonly
M:DocSiteChromeOverridesExample.SiteChromeOverrides.BuildExtraStyles
```

### Replace the header and footer with the string slots

`HeaderContent` and `FooterContent` are raw HTML strings the DocSite layout splices into the top bar and footer regions — they accept anything an HTML fragment can hold, from a branded logo wordmark to a compliance notice. Because they are strings, no `AdditionalRoutingAssemblies` entry is needed for them; for a component-authored fragment, render it to HTML at startup the same way as the head snippet above.

```csharp
var options = new DocSiteOptions
{
    HeaderContent = "<span class=\"chrome-header\">Extensibility Lab</span>",
    FooterContent = "<span class=\"chrome-footer\">(c) 2026 Pennington</span>",
    // ...
};
```

### Route your own `@page` components via `AdditionalRoutingAssemblies`

The DocSite shell only discovers `@page` directives in its own assembly by default; adding the host assembly to `AdditionalRoutingAssemblies` makes any `@page "/route"` component in that assembly routable alongside the bundled pages. The example returns `[typeof(SiteChromeOverrides).Assembly]` so a Razor component like `ExtraPage.razor` sitting next to `Program.cs` gets picked up without any additional DI wiring.

```csharp:xmldocid,bodyonly
M:DocSiteChromeOverridesExample.SiteChromeOverrides.BuildAdditionalRoutingAssemblies
```

## Register the implementation

`AddDocSite` takes a `Func<DocSiteOptions>` factory, so the most direct wiring is to pass the helper as a method reference and keep the host file short. The example's `Program.cs` runs this exact shape end-to-end.

```csharp:path
examples/DocSiteChromeOverridesExample/Program.cs
```

## Result

The chrome on every page is replaced by the configured fragments. The header reads "Chrome Overrides" on the left (rendered as `<span class="chrome-header" data-chrome-overrides="docsite-header">`), the footer carries the matching copyright span, every `<head>` gains the `<meta name="x-chrome-overrides-head">` tag and the `https://example.com` preconnect, and `/styles.css` ends with the appended `.chrome-header` / `.chrome-footer` rules. Any `@page "/route"` component in the host assembly (for example `/extra`) routes alongside the bundled DocSite pages.

## Verify

- Run `dotnet run` and view page source on `/` — expect the `<meta name="x-chrome-overrides-head">` tag inside `<head>`, your `HeaderContent` and `FooterContent` markup in the layout, and the `.chrome-header` rule inside `/styles.css`.
- Navigate to a route defined by a Razor component in your app assembly (for example `/extra`) and confirm it renders. A 404 here means `AdditionalRoutingAssemblies` is not including the right assembly.
- Run `dotnet run -- build output` and search `output/index.html` for your head fragment and `output/styles.css` for your `ExtraStyles` rules to confirm the overrides survive publish.

## Related

- Reference: <xref:reference.api.doc-site-options> — the full property surface, including `ConfigurePennington`, `CustomCssFrameworkSettings`, and every other override seam beyond the four covered here.
- How-to: <xref:how-to.discovery.multiple-sources> — register extra markdown sources through `DocSiteOptions.ConfigurePennington`.
- How-to: <xref:how-to.content-services.custom-content-service> — register a custom `IContentService` alongside DocSite's own.
- Background: <xref:explanation.positioning.docsite-positioning> — when forking DocSite or dropping to bare `AddPennington` becomes the right move.
- Background: <xref:explanation.spa.islands> — `data-spa-region` semantics for SPA-aware layout components.
