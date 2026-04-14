---
title: "Customize DocSite layouts and components"
description: "Inject head content, extra CSS, header/footer HTML, and extra @page assemblies through DocSiteOptions without forking the template."
uid: how-to.extensibility.override-docsite-components
order: 70
sectionLabel: Extensibility
tags: [docsite, theming, slots, routing]
---

> **In this page.** _Paraphrase the TOC "Covers" line: the four DocSite override seams exposed on `DocSiteOptions` — `AdditionalHtmlHeadContent`, `ExtraStyles`, the string-HTML `HeaderContent` / `FooterContent` slots, and `AdditionalRoutingAssemblies` for your own `@page` components — plus pointing readers at the neighbouring island how-to when they want to replace the SPA content renderer._
>
> **Not in this page.** _Paraphrase the TOC "Does not cover": forking the DocSite template wholesale. When you need to rearrange the layout shell itself, drop to bare `AddPennington` and build your own Razor host — link to the positioning explainer so readers understand the trade-off before they walk away from the DocSite shortcut._

## When to use this

_Two sentences. Frame the reader as someone who started from `AddDocSite` and needs to brand the chrome (head tags, footer HTML, one or two extra routed pages) without giving up the bundled layout, content pipeline, SPA navigation, and MonorailCSS wiring. Point out the escape hatches: if they want to replace the article island itself they should follow [Register an island renderer](/how-to/extensibility/island-renderer), and if they want a fundamentally different shell they should read [When is DocSite the right starting point?](/explanation/core/docsite-positioning) first._

## Assumptions

_Three bullets. Each is realistic prior state — reader has an `AddDocSite` app, knows MonorailCSS emits `/styles.css`, and understands that these seams are set at host-build time (hot-reload on changes requires `dotnet watch`)._

- You have an existing Pennington site wired through `AddDocSite(...)` (see the [Getting Started tutorial](/tutorials/getting-started/first-site) if not).
- You are editing the `DocSiteOptions` factory you pass to `AddDocSite`, not the DocSite source — forking the template is out of scope (see [When is DocSite the right starting point?](/explanation/core/docsite-positioning)).
- You understand that `ExtraStyles` is appended to the generated `/styles.css`, so rules you add there ship alongside the MonorailCSS utility output rather than as a separate stylesheet.

To copy a working setup, see [`examples/ExtensibilityLabExample`](https://github.com/usepennington/pennington/tree/main/examples/ExtensibilityLabExample) — `SiteChromeOverrides.cs` is a compile-only helper that returns a populated `DocSiteOptions` exercising all four seams, and `Components/ExtraHeadFragment.razor` is the component that backs the head-slot fragment.

---

## Steps

### 1. Build a populated `DocSiteOptions`

_Two sentences. The whole page's code surface lives in one factory method — point the reader at it so they see how the four seams coexist on a single record initializer. The example sets `SiteTitle` and `Description` alongside the override seams so the shape matches what `AddDocSite(() => SiteChromeOverrides.BuildDocSiteOptions())` would produce._

```csharp:xmldocid
M:ExtensibilityLabExample.SiteChromeOverrides.BuildDocSiteOptions
```

### 2. Inject tags into `<head>` via `AdditionalHtmlHeadContent`

_Two sentences. `AdditionalHtmlHeadContent` is a raw HTML string rendered inside every page's `<head>`, so it is the right seam for meta tags, preconnect hints, analytics snippets, and font `<link>` elements that MonorailCSS does not know about. If you would rather author the fragment as a Razor component, render it with `ToHtmlString()` once at startup and pass the resulting string — the example pairs `SiteChromeOverrides.BuildHtmlHeadContent` with `Components/ExtraHeadFragment.razor` so you can see both shapes side by side._

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.SiteChromeOverrides.BuildHtmlHeadContent
```

### 3. Append rules to the generated stylesheet via `ExtraStyles`

_Two sentences. `ExtraStyles` is a CSS string concatenated onto the MonorailCSS-generated `/styles.css`, so it is the right home for `@font-face` declarations, custom-property overrides, and any selector the utility-class scanner will not discover on its own. Keep this string small — anything you can express as MonorailCSS utilities in your Razor markup gets picked up automatically by `CssClassCollectorProcessor`._

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.SiteChromeOverrides.BuildExtraStyles
```

### 4. Replace the header and footer HTML with the string slots

_Two sentences. `HeaderContent` and `FooterContent` are raw HTML strings the DocSite layout splices into the top bar and footer regions — they accept anything an HTML fragment can hold, from a branded logo wordmark to a compliance notice. Because they are strings, you do not need an `AdditionalRoutingAssemblies` entry for them; for a component-authored fragment, render it to HTML at startup just like the head snippet above._

TODO: Identify whether a short literal-string example (e.g. the `<span class="chrome-header">…</span>` lines from `SiteChromeOverrides.BuildDocSiteOptions`) should be pulled out into a dedicated short xmldocid target, or whether inlining the two lines as a plain markdown C# snippet is preferable.

### 5. Route your own `@page` components via `AdditionalRoutingAssemblies`

_Two sentences. The DocSite shell only discovers `@page` directives in its own assembly by default; adding your app's assembly to `AdditionalRoutingAssemblies` makes any `@page "/route"` component in that assembly routable alongside the bundled pages. The example returns `[typeof(SiteChromeOverrides).Assembly]` so a Razor component like `ExtraPage.razor` sitting next to `Program.cs` gets picked up without any additional DI wiring._

```csharp:xmldocid,bodyonly
M:ExtensibilityLabExample.SiteChromeOverrides.BuildAdditionalRoutingAssemblies
```

### 6. Wire the options into `AddDocSite`

_Two sentences. `AddDocSite` takes a `Func<DocSiteOptions>` factory, so the simplest wiring is to call the helper directly and keep the host file short. The example's `Program.cs` uses bare `AddPennington` instead (to keep the other six extensibility recipes visible in isolation), but a real DocSite app would write `builder.Services.AddDocSite(SiteChromeOverrides.BuildDocSiteOptions)` and then `app.UseDocSite().RunDocSiteAsync(args)`._

TODO: Decide whether to embed `DocSiteKitchenSinkExample/Program.cs` (or equivalent) via a `csharp:path` fence here to show the `AddDocSite(...)` + `UseDocSite()` + `RunDocSiteAsync(args)` trio, or leave as prose because `SiteChromeOverrides` is compile-only inside `ExtensibilityLabExample`.

### 7. Point to the island-renderer how-to for the content slot

_One sentence. Replacing the SPA content island itself — the renderer whose `IslandName` is `"content"` — is a separate recipe because it plugs into the islands system rather than `DocSiteOptions`; the DocSite ships `DocSiteArticleSlotRenderer` as the default, and a custom renderer registered with the same island name overrides it._

_Follow [Register an island renderer](/how-to/extensibility/island-renderer) and register your `IIslandRenderer` with `IslandName => "content"` to displace the shipped `DocSiteArticleSlotRenderer`._

---

## Verify

_Terse. Three bullets: one dev-server check, one build check, one sanity check that the routing-assembly seam actually widened the router._

- Run `dotnet run` and view page source on `/` — expect the `<meta name="x-extensibility-lab-head">` tag inside `<head>`, your `HeaderContent` / `FooterContent` markup in the layout, and the `.chrome-header` rule inside `/styles.css`.
- Navigate to the route defined by a Razor component in your app assembly (e.g. `/extra`) and confirm it renders — a 404 here means `AdditionalRoutingAssemblies` is not including the right assembly.
- Static build: `dotnet run -- build output` and grep `output/index.html` for your head fragment and `output/styles.css` for your `ExtraStyles` rules so you know the overrides survive publish.

## Related

- Reference: [`DocSiteOptions`](/reference/options/docsite-options)
- Background: [When is DocSite the right starting point?](/explanation/core/docsite-positioning)
- Related how-to: [Register an island renderer](/how-to/extensibility/island-renderer)
