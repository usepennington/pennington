---
title: "When is DocSite the right starting point?"
description: "Why AddDocSite is a template-style fast path with a fixed skeleton — and the narrow set of shapes that push you down to bare AddPennington instead."
uid: explanation.core.docsite-positioning
order: 50
sectionLabel: "Core Architecture"
tags: [docsite, templates, architecture]
---

> **In this page.** _One sentence: `AddDocSite` is a template-style fast path that commits to one markdown front-matter type, one theme composition, and one layout skeleton — the remaining caps and the escape hatches are the menu below._
>
> **Not in this page.** _One sentence pointing away from feature-by-feature comparison with `AddPennington` — the bullet list here is the full menu, not a half of a comparison table that continues elsewhere._

## The question

_One sentence framed as the reader's question: "When does `AddDocSite` earn its keep, and when is the ceremony of `AddPennington` + hand-wired layout actually the faster path?" Do not answer yet; the rest of the page is the answer._

## Context

_Three to five sentences. Set the stage: Pennington ships two entry points. `AddPennington` is the engine — discovery, pipeline, response processing, diagnostics — and assumes you will bring your own layout, routing, and CSS wiring. `AddDocSite` is a template on top of that engine: it composes `AddPennington` + `AddMonorailCss` + `AddSpaNavigation` + a Razor `App` component + a `DocSiteArticleSlotRenderer` into a single `AddDocSite(...)` call with a small `DocSiteOptions` surface. The distinction matters because `DocSiteOptions` is not a reflection of `PenningtonOptions` — it is a curated subset plus a few DocSite-specific knobs, and the things `DocSiteOptions` deliberately does not expose are what tell you whether to stay on the template or drop a level._

## How it works

### What DocSite gives you for free

_Two short paragraphs. List, in prose not bullets, what the single `AddDocSite` call wires: the Pennington engine with site title, description, canonical URL, and content root forwarded from `DocSiteOptions`; one `AddMarkdownContent<DocSiteFrontMatter>` registration rooted at the content folder; a pre-scoped `llms.txt` and search index (both defaulting to `#main-content`, the wrapper around the stock article); the Razor `App` component with the article slot renderer; Mdazor inline components (`Badge`, `Card`, `CodeBlock`, `Step`, etc.) registered so markdown can embed UI without per-site plumbing; MonorailCSS with the DocSite theme, and SPA navigation. Emphasize that the payoff is not feature count — it is that every one of these is wired in a compatible order with the others, which is most of what gets miswired in a hand-rolled host._

```csharp:xmldocid
M:Pennington.DocSite.DocSiteServiceExtensions.AddDocSite(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{Pennington.DocSite.DocSiteOptions})
```

_After the fence, make one observation: the extension method reads top-to-bottom as the spec for what "a DocSite" means. If a reader wants to know exactly what DocSite decides on their behalf, that method is the source of truth, not this page._

### What DocSite caps

_Three short paragraphs — this is the load-bearing section of the page. Be specific about the actual current caps, which are narrower than `DocSiteOptions` might look at first glance._

_Paragraph one — the single-source cap. DocSite owns exactly one `AddMarkdownContent<DocSiteFrontMatter>` registration. Wiring a second front-matter type (say, a blog post shape alongside docs) is not reachable by setting a `DocSiteOptions` property — it requires either the `ConfigurePennington` escape hatch (which hands you the underlying `PenningtonOptions` after DocSite's defaults land) or dropping to bare `AddPennington` outright. The escape hatch is enough for "add one more source"; it is not enough when the extra source wants different theming, different layout, or a different slot renderer._

```csharp:xmldocid
P:Pennington.DocSite.DocSiteOptions.ConfigurePennington
```

_Paragraph two — the theme and layout cap. `DocSiteOptions.ColorScheme`, `DisplayFontFamily`, `BodyFontFamily`, `ExtraStyles`, and (as of the current source) `CustomCssFrameworkSettings` give you tweak points against MonorailCSS, but the theme composition itself — `AddMonorailCss` plus the DocSite `App` component plus the article slot renderer — is fixed. Replacing the `App` component or introducing a non-article layout means registering extra routing assemblies via `AdditionalRoutingAssemblies` and accepting that your custom components now ride alongside DocSite's, not in place of them._

```csharp:xmldocid
T:Pennington.DocSite.DocSiteOptions
```

_Paragraph three — the content-selector story. Earlier versions of DocSite pinned `SearchIndexOptions.ContentSelector` and `LlmsTxtOptions.ContentSelector` to `#main-content` with no override. The current shape exposes `SearchIndexContentSelector` and `LlmsTxtContentSelector` on `DocSiteOptions`; both default to `#main-content` (because that is what the stock layout wraps the article in) but accept any CSS selector, including the empty string to index the full body when you have replaced the layout. This is worth calling out explicitly because how-tos and older TOC notes still describe these as hard caps — they are now soft defaults._

```csharp:xmldocid
P:Pennington.DocSite.DocSiteOptions.SearchIndexContentSelector
P:Pennington.DocSite.DocSiteOptions.LlmsTxtContentSelector
```

_After the fences, summarize the current cap list precisely: one markdown source registration (extendable via `ConfigurePennington` or by dropping a level); fixed theme composition (`AddMonorailCss` + DocSite layout shell, tweakable but not swappable); fixed slot renderer and `App` component (extendable only through additional routing assemblies)._

### The escape hatch — DocSite's source as reference

_Two short paragraphs. When the caps start to bind, the right move is not to fight `DocSiteOptions` — it is to copy the pattern out of `DocSiteServiceExtensions.AddDocSite` and paste what you need into a bare `AddPennington` host. That method is ~60 lines of composition with no hidden state: the service registrations, the option forwarding, and the middleware order are all visible. Reading it gives you a checklist of "here is what a Pennington-shaped host needs" that you can subset or extend freely._

_The second paragraph points at `ExtensibilityLabExample` as the canonical bare-host reference. It wires seven extension points raw against `AddPennington`, uses its own markdown rendering via `MapGet`, and demonstrates that the engine is happy to run without the DocSite template at all. Readers who need a shape the template does not fit should treat that example as the starting skeleton, not a from-scratch `WebApplication.CreateBuilder`._

```csharp:path
examples/ExtensibilityLabExample/Program.cs
```

_After the fence, note that the example host is intentionally minimal (no Razor layout, no slot renderer, hand-written HTML strings) — it demonstrates the engine surface, not a production layout. The DocSite extension method is where you go for the layout recipe._

### Signals that say "drop to bare AddPennington"

_Four to six short bullets or a paragraph, factual and concrete — these are the signals, not a decision tree. Each should be a shape the template genuinely does not fit rather than a preference. Suggested set:_

- _You need more than one markdown front-matter type served from the same host with different themes or different layouts — `ConfigurePennington` can register a second source, but it cannot give it a separate layout shell._
- _You need to replace the `App` component or the article slot renderer with a layout that is not article-shaped (a dashboard, a directory, a storefront)._
- _You need a non-Razor rendering story — custom `MapGet` handlers, Minimal API endpoints that emit HTML strings, or a reverse-proxy shape where the engine is feeding a different front-end entirely._
- _You need to embed Pennington inside an existing ASP.NET app that already owns its routing, authentication, or layout conventions — adding `AddDocSite` on top would fight those choices._
- _You are shipping the engine as a library into another product and want only the pipeline, not the layout._

_After the list, add one sentence of judgment: if none of these shapes describes the work, `AddDocSite` is almost certainly the right starting point — the caps are narrow because the template is opinionated, and the opinions cover ~90% of the documentation-site shape._

## Trade-offs

_Three bullets. Name what the template-vs-bare split actually costs, not a generic "flexibility trade-off" platitude._

- **Cost:** _`AddDocSite` and `AddPennington` are two APIs with two learning surfaces. A user who starts on the template and later needs bare access has to learn both — the template's options, and then the underlying options the template was composing over. Pennington accepts this cost because the alternative (`AddPennington` with twenty configuration callbacks) makes the common case noisier in exchange for making the advanced case fractionally more discoverable._
- **Alternative considered — one config surface with feature flags:** _A single `AddPennington` with a `UseDefaultDocSiteLayout = true` flag was rejected because it forces every bare-host user to carry the layout code in their dependency graph (with its Razor components and MonorailCSS configuration) even when they are not using it, and because it conflates "engine configuration" with "layout configuration" in one record that then has to explain which knobs apply in which mode._
- **Consequence:** _`DocSiteOptions` is a curated subset of `PenningtonOptions` on purpose, and it will stay that way — new DocSite-specific knobs get added when the template needs them, new engine knobs land on `PenningtonOptions`, and `ConfigurePennington` is the bridge when a user needs an engine knob from inside a template-shaped host. If you find yourself wishing for a `DocSiteOptions` property that already exists on `PenningtonOptions`, reach for `ConfigurePennington` before filing a feature request — that is the designed answer._

## Further reading

_Three to four cross-quadrant links. Do NOT link to the next explanation in the same section — auto-generated. Suggested set:_

- Reference: [`DocSiteOptions` reference](/reference/docsite/options/) — the full list of tweak points with defaults and forwarding semantics. _TODO — confirm final URL from docs-toc before publish._
- How-to: [Use multiple content sources](/how-to/configuration/multiple-sources/) — the canonical how-to for the "two front-matter types" case, which either uses `ConfigurePennington` or drops to bare `AddPennington`.
- How-to: [Override DocSite components](/how-to/extensibility/override-docsite-components/) — the template-compatible extension path for layout tweaks before dropping a level.
- Example: [`ExtensibilityLabExample`](https://github.com/philsnotes/Pennington/tree/main/examples/ExtensibilityLabExample) — the canonical bare-`AddPennington` host that pairs with this page. _TODO — confirm repo URL before publish._
