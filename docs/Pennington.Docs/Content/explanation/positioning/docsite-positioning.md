---
title: "What the DocSite and BlogSite templates wire for you"
description: "DocSite and BlogSite are pre-assembled shortcuts on top of the AddPennington engine — what each template wires, and where the wiring stops and the engine takes over."
uid: explanation.positioning.docsite-positioning
order: 1
sectionLabel: "Positioning"
tags: [docsite, blogsite, templates, architecture]
---

`AddDocSite` and `AddBlogSite` are shortcuts. Each one pre-assembles a host that `AddPennington` would otherwise have you wire by hand. This page is about what they assemble — and where that assembly stops.

## Context

`AddPennington` is the engine, and building on it is how Pennington sites are made: content discovery, the rendering pipeline, response processing, and diagnostics. It expects the host to bring its own layout, routing, and CSS wiring — which the [getting-started tutorials](xref:tutorials.getting-started.first-site) walk through end to end.

`AddDocSite` and `AddBlogSite` are templates layered on that engine. Each composes `AddPennington`, `AddMonorailCss`, and a Razor `App` component into a single call with a small options surface — `DocSiteOptions` or `BlogSiteOptions`. They exist to skip the wiring for two shapes that come up often: a Divio-style documentation site, and a site where the blog *is* the site. When a site is exactly one of those, the template is a real head start. When it is not, the host is built on `AddPennington` directly — the ordinary case, not a fallback.

The rest of this page examines DocSite in detail. BlogSite is the same kind of shortcut with a narrower options surface, and the same reasoning carries over.

The distinction that matters is that `DocSiteOptions` is not a mirror of `PenningtonOptions`. It is a curated subset plus a handful of DocSite-specific knobs, and the things it deliberately does not expose mark the edge of the template's shape.

## How it works

### What DocSite gives you for free

A single `AddDocSite` call wires quite a bit: the Pennington engine with site title, description, canonical URL, and content root forwarded from `DocSiteOptions`; one `AddMarkdownContent<DocSiteFrontMatter>` registration rooted at the content folder; a pre-scoped `llms.txt` and search index (both defaulting to `#main-content`, the wrapper around the stock article); the Razor `App` component and `DocSiteArticle` rendering shell; Mdazor inline components — `Badge`, `Card`, `Step`, and others — registered so markdown can embed UI without per-site plumbing; MonorailCSS with the DocSite theme; and SPA navigation through the `data-spa-region` markup the layout emits (no extra DI registration required — the client script lives in `Pennington.UI`).

The payoff is not the feature count. It is that every one of those registrations lands in a compatible order with the others. Getting that ordering right — especially between the pipeline, the response processor, and the search index scoping — is most of what trips up a hand-rolled host on the first attempt.

### What DocSite caps

DocSite owns exactly one `AddMarkdownContent<DocSiteFrontMatter>` registration. Wiring a second front-matter type — say, a blog post shape alongside docs — is not reachable by setting a `DocSiteOptions` property. It takes either the [`ConfigurePennington`](xref:reference.api.doc-site-options) escape hatch (a `Action<PenningtonOptions>` callback that runs after DocSite's defaults land) or dropping to bare `AddPennington` outright. The escape hatch is enough for adding one more source. It falls short when the extra source needs different theming, a different layout, or a different slot renderer.

`DocSiteOptions.ColorScheme`, `DisplayFontFamily`, `BodyFontFamily`, `ExtraStyles`, and `CustomCssFrameworkSettings` offer tweak points against MonorailCSS, but the theme composition itself — `AddMonorailCss` plus the DocSite `App` component plus the `DocSiteArticle` shell — is fixed. Replacing the `App` component or introducing a non-article layout means registering extra routing assemblies via `AdditionalRoutingAssemblies` and accepting that custom components ride alongside DocSite's, not in place of them.

`ContentSelector` on `DocSiteOptions` defaults to `#main-content` — the wrapper the stock layout places around the article — and accepts any CSS selector, including the empty string to index the full body when the layout has been replaced. One selector drives the shared site projection consumed by every corpus aggregator (search index, llms.txt sidecars, build-time link audit), so chrome is stripped once and every channel sees the same body element.

To summarize the current caps precisely: one markdown source registration, extendable via `ConfigurePennington` or by dropping a level; a fixed theme composition (`AddMonorailCss` plus the DocSite layout shell) that is tweakable but not swappable; and a fixed `App` component that can only be extended through additional routing assemblies, not replaced.

### The escape hatch — DocSite's source as reference

When the caps start to bind, the productive move is not to fight `DocSiteOptions` but to copy the pattern out of `DocSiteServiceExtensions.AddDocSite` and paste what is needed into a bare `AddPennington` host. That method is a single composition with no hidden state: the service registrations, the option forwarding, and the middleware order are all visible. Reading it gives a checklist of what a Pennington-shaped host needs, one that can be freely subsetted or extended.

The [`ExtensibilityLabExample`](https://github.com/usepennington/pennington/tree/main/examples/ExtensibilityLabExample) project serves as the canonical bare-host reference. It wires extension points directly against `AddPennington`, uses its own markdown rendering via `MapGet`, and demonstrates that the engine runs happily without the DocSite template. For hosts whose shape the template does not fit, `examples/ExtensibilityLabExample/Program.cs` is a better starting skeleton than a blank `WebApplication.CreateBuilder`.

The example host is intentionally minimal — no Razor layout, no slot renderer, hand-written HTML strings. It demonstrates the engine surface, not a production layout pattern. The DocSite extension method is the right place to look for the layout recipe.

### The shape the template assumes

A template fits a shape. DocSite's shape is an article-centric documentation site rendered through a fixed Razor layout. A handful of site shapes fall outside it — and there the host is built on the engine, not the template:

- Multiple markdown front-matter types served from the same host with different themes or different layouts. `ConfigurePennington` can register a second source, but it cannot give that source a separate layout shell.
- Replacing the `App` component or the `DocSiteArticle` shell with a layout that is not article-shaped — a dashboard, a directory, a storefront.
- A non-Razor rendering story: custom `MapGet` handlers, Minimal API endpoints that emit HTML strings, or a reverse-proxy shape where the engine feeds a different front-end entirely.
- Embedding Pennington inside an existing ASP.NET app that already owns its routing, authentication, or layout conventions. Adding `AddDocSite` on top tends to fight those choices rather than cooperate with them.
- Shipping the engine as a library into another product where only the pipeline is needed, not the layout.

None of these is exotic. They are the everyday reason a host is built on `AddPennington` directly. The caps are narrow because the template is opinionated, and those opinions are about documentation-site shapes specifically.

## Trade-offs

`AddDocSite` and `AddPennington` are two APIs with two learning surfaces. A host that starts on the template and later needs bare access has to learn both — the template's options, and then the underlying options the template was composing over. Pennington accepts this cost because the alternative — `AddPennington` with twenty configuration callbacks collapsed into one record — makes the common case noisier in exchange for making the advanced case fractionally more discoverable.

A single `AddPennington` with a `UseDefaultDocSiteLayout` flag would force every bare-host consumer to carry the layout code — Razor components, MonorailCSS configuration — even when unused, and would conflate engine configuration with layout configuration in one record that has to explain which knobs apply in which mode.

The consequence of that decision is that `DocSiteOptions` is a curated subset of `PenningtonOptions` on purpose, and the design intent is for it to stay that way. New DocSite-specific knobs land when the template needs them; new engine knobs land on `PenningtonOptions`; and `ConfigurePennington` is the bridge when an engine knob is needed from inside a template-shaped host. The tradeoff is a clean separation that takes knowing which layer a given option belongs to, rather than one surface that tries to hold everything.

## Further reading

- Tutorial: [Create your first Pennington site](xref:tutorials.getting-started.first-site) — building a host on `AddPennington` directly, the path the templates are a shortcut for.
- Reference: [`DocSiteOptions` reference](xref:reference.api.doc-site-options) — the full list of tweak points with defaults and forwarding semantics.
- How-to: [Use multiple content sources](xref:how-to.discovery.multiple-sources) — the canonical guide for the "two front-matter types" case, covering both the `ConfigurePennington` path and the drop to bare `AddPennington`.
- How-to: [Override DocSite components](xref:how-to.response-pipeline.override-docsite-components) — the template-compatible extension path for layout tweaks before dropping a level.
