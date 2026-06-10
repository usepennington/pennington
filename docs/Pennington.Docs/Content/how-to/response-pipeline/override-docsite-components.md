---
title: "Customize the DocSite chrome through DocSiteOptions"
description: "Use DocSiteOptions to inject head content, append CSS, replace the header/footer HTML, and route extra @page components without forking the template."
uid: how-to.response-pipeline.override-docsite-components
order: 3
sectionLabel: "Response Pipeline"
tags: [docsite, theming, slots, routing]
---

To replace the bundled DocSite header or footer (or inject head tags, append CSS, route additional `@page` components) without forking the template, populate the four extension points — the *slot seams* — on `DocSiteOptions`. The bundled layout, content pipeline, SPA navigation, and [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/) wiring keep working. To rearrange the layout shell fundamentally, read <xref:explanation.positioning.docsite-positioning> before deciding whether `AddDocSite` is still the right starting point.

## Before you begin

- An existing Pennington site wired through `AddDocSite(...)` (see <xref:tutorials.docsite.scaffold> if not).
- All edits go in the `DocSiteOptions` factory passed to `AddDocSite`, not the DocSite source.
- These extension points are set at host-build time — changes take effect on the next `dotnet run`, whose source watch reloads them.

For a working setup, see `examples/DocSiteChromeOverridesExample`. `SiteChromeOverrides.cs` returns a populated `DocSiteOptions` exercising all four extension points, `Components/ExtraHeadFragment.razor` backs the head-slot fragment, and `Components/ExtraPage.razor` is the routed `@page` component showing that `AdditionalRoutingAssemblies` widened the router. `Program.cs` runs the DocSite end-to-end against those overrides.

## Build the populated options

All the code for this recipe lives in one factory method, so the four extension points sit together on a single record initializer. The example sets `SiteTitle` and `SiteDescription` alongside the override properties, matching the options produced by `AddDocSite(() => SiteChromeOverrides.BuildDocSiteOptions())`.

```csharp:symbol
examples/DocSiteChromeOverridesExample/SiteChromeOverrides.cs > SiteChromeOverrides.BuildDocSiteOptions
```

### Inject tags into `<head>` via `AdditionalHtmlHeadContent`

`AdditionalHtmlHeadContent` is a raw HTML string rendered inside every page's `<head>`, making it the right place for meta tags, preconnect hints, analytics snippets, and font `<link>` elements that MonorailCSS does not know about. To author the fragment as a Razor component instead, render it with `ToHtmlString()` once at startup and pass the resulting string — the example pairs `SiteChromeOverrides.BuildHtmlHeadContent` with `Components/ExtraHeadFragment.razor` so both approaches sit side by side.

Use this string for static site-wide markup you do not want to write a class for; reach for an [`IHeadContributor`](xref:how-to.response-pipeline.head-contributor) instead when the tag must deduplicate against another writer, order against site or page defaults, or be computed per-page. Both routes flow through the same head reconciler, so either way the tags get a `data-head` stamp and survive SPA navigation.

```csharp:symbol,bodyonly
examples/DocSiteChromeOverridesExample/SiteChromeOverrides.cs > SiteChromeOverrides.BuildHtmlHeadContent
```

### Prepend rules to the generated stylesheet via `ExtraStyles`

`ExtraStyles` is a CSS string emitted above the MonorailCSS-generated rules inside `/styles.css`, making it the right home for `@font-face` declarations, custom-property overrides, and any selector the utility-class scanner will not discover on its own. Keep this string small — anything expressible as MonorailCSS utilities in Razor markup gets picked up automatically by the `MonorailCss.Discovery` pipeline.

```csharp:symbol,bodyonly
examples/DocSiteChromeOverridesExample/SiteChromeOverrides.cs > SiteChromeOverrides.BuildExtraStyles
```

### Replace the site-title link and footer with the string slots

`HeaderContent` substitutes for the default `<a href="/">SiteTitle</a>` link inside the header's title span; the rest of the header chrome (optional `HeaderIcon`, search button, theme toggle, repo link) keeps rendering around it. `FooterContent` is the raw HTML fragment the layout drops into the footer region. Both accept anything an HTML fragment can hold, from a branded logo wordmark to a compliance notice. Because they are strings, no `AdditionalRoutingAssemblies` entry is needed for them; for a component-authored fragment, render it to HTML at startup the same way as the head snippet above.

```csharp
var options = new DocSiteOptions
{
    HeaderContent = """<span class="chrome-header" data-chrome-overrides="docsite-header">Chrome Overrides</span>""",
    FooterContent = """<span class="chrome-footer" data-chrome-overrides="docsite-footer">(c) 2026 Pennington</span>""",
    // ...
};
```

The `data-chrome-overrides` attributes are not required by `DocSiteOptions` — they are markers that make the swapped-in chrome easy to spot in page source, matching what the example renders and what the Result describes below.

### Route your own `@page` components via `AdditionalRoutingAssemblies`

The DocSite shell only discovers `@page` directives in its own assembly by default; adding the host assembly to `AdditionalRoutingAssemblies` makes any `@page "/route"` component in that assembly routable alongside the bundled pages. The example returns `[typeof(SiteChromeOverrides).Assembly]` so a Razor component like `ExtraPage.razor` sitting next to `Program.cs` gets picked up without any additional DI wiring.

```csharp:symbol,bodyonly
examples/DocSiteChromeOverridesExample/SiteChromeOverrides.cs > SiteChromeOverrides.BuildAdditionalRoutingAssemblies
```

## Register the implementation

`AddDocSite` takes a `Func<DocSiteOptions>` factory, so the most direct wiring is to pass the helper as a method reference and keep the host file short. The example's `Program.cs` runs this exact shape end-to-end.

```csharp:symbol
examples/DocSiteChromeOverridesExample/Program.cs
```

## Result

The chrome on every page is replaced by the configured fragments, one outcome per extension point:

- **Header and footer.** The header title reads "Chrome Overrides" on the left, rendered as `<span class="chrome-header" data-chrome-overrides="docsite-header">` in place of the default `<a href="/">…</a>` link, with the rest of the header chrome (icon, search, theme toggle, repo link) intact; the footer carries the matching `data-chrome-overrides="docsite-footer"` copyright span.
- **Head content.** Every `<head>` gains the `<meta name="x-chrome-overrides-head">` tag and the `https://example.com` preconnect.
- **Styles.** `/styles.css` begins with the prepended `.chrome-header` / `.chrome-footer` rules, above the generated MonorailCSS utilities.
- **Routing.** Any `@page "/route"` component in the host assembly (for example `/extra`) routes alongside the bundled DocSite pages.

## Verify

- Run `dotnet run` and view page source on `/` — expect the `<meta name="x-chrome-overrides-head">` tag inside `<head>`, your `HeaderContent` and `FooterContent` markup in the layout, and the `.chrome-header` rule inside `/styles.css`.
- Navigate to a route defined by a Razor component in your app assembly (for example `/extra`) and confirm it renders. A 404 here means `AdditionalRoutingAssemblies` is not including the right assembly.
- Run `dotnet run -- build output` and search `output/index.html` for your head fragment and `output/styles.css` for your `ExtraStyles` rules to confirm the overrides survive publish.

## Related

- Reference: <xref:reference.api.doc-site-options> — the full set of properties, including `ConfigurePennington`, `CustomCssFrameworkSettings`, and every other override point beyond the four covered here.
- How-to: <xref:how-to.response-pipeline.head-contributor> — the typed alternative to `AdditionalHtmlHeadContent` when a head tag must dedup, order, or compute per-page.
- How-to: <xref:how-to.discovery.multiple-sources> — register extra markdown sources through `DocSiteOptions.ConfigurePennington`.
- How-to: <xref:how-to.content-services.custom-content-service> — register a custom `IContentService` alongside DocSite's own.
- Background: <xref:explanation.positioning.docsite-positioning> — when forking DocSite or dropping to bare `AddPennington` becomes the right move.
- Background: <xref:explanation.spa.islands> — `data-spa-region` semantics for SPA-aware layout components.
