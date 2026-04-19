---
title: "When is DocSite the right starting point?"
description: "Why AddDocSite is a template-style fast path with a fixed skeleton — and the narrow set of shapes that push you down to bare AddPennington instead."
uid: explanation.core.docsite-positioning
order: 301050
sectionLabel: "Core Architecture"
tags: [docsite, templates, architecture]
---

When does `AddDocSite` earn its keep, and when is the ceremony of `AddPennington` plus a hand-wired layout the faster path?

## Context

Pennington ships two entry points that sit at different levels of the stack. `AddPennington` is the engine: content discovery, the rendering pipeline, response processing, and diagnostics. It assumes the host brings its own layout, routing, and CSS wiring. `AddDocSite` is a template built on top of that engine — it composes `AddPennington`, `AddMonorailCss`, `AddSpaNavigation`, a Razor `App` component, and a `DocSiteArticleSlotRenderer` into a single call with a small `DocSiteOptions` surface.

The distinction matters because `DocSiteOptions` is not a mirror of `PenningtonOptions`. It is a curated subset plus a handful of DocSite-specific knobs, and the things it deliberately does not expose are the signal for whether to stay on the template or drop a level.

## How it works

### What DocSite gives you for free

A single `AddDocSite` call wires quite a bit: the Pennington engine with site title, description, canonical URL, and content root forwarded from `DocSiteOptions`; one `AddMarkdownContent<DocSiteFrontMatter>` registration rooted at the content folder; a pre-scoped `llms.txt` and search index (both defaulting to `#main-content`, the wrapper around the stock article); the Razor `App` component with the article slot renderer; Mdazor inline components — `Badge`, `Card`, `CodeBlock`, `Step`, and others — registered so markdown can embed UI without per-site plumbing; MonorailCSS with the DocSite theme; and SPA navigation.

The payoff is not the feature count. It is that every one of those registrations lands in a compatible order with the others. Getting that ordering right — especially between the pipeline, the response processor, and the search index scoping — is most of what trips up a hand-rolled host on the first attempt.

The `DocSiteServiceExtensions.AddDocSite` method reads top-to-bottom as the specification for what "a DocSite" means. When a question arises about exactly what DocSite decides on a host's behalf, that method is the source of truth — more reliable than any page of prose that can drift over time.

### What DocSite caps

DocSite owns exactly one `AddMarkdownContent<DocSiteFrontMatter>` registration. Wiring a second front-matter type — say, a blog post shape alongside docs — is not reachable by setting a `DocSiteOptions` property. It takes either the `ConfigurePennington` escape hatch, which hands back the underlying `PenningtonOptions` after DocSite's defaults land, or dropping to bare `AddPennington` outright. The escape hatch is enough for adding one more source. It falls short when the extra source needs different theming, a different layout, or a different slot renderer.

`DocSiteOptions.ColorScheme`, `DisplayFontFamily`, `BodyFontFamily`, `ExtraStyles`, and `CustomCssFrameworkSettings` offer tweak points against MonorailCSS, but the theme composition itself — `AddMonorailCss` plus the DocSite `App` component plus the article slot renderer — is fixed. Replacing the `App` component or introducing a non-article layout means registering extra routing assemblies via `AdditionalRoutingAssemblies` and accepting that custom components ride alongside DocSite's, not in place of them.

Earlier versions of DocSite pinned `SearchIndexOptions.ContentSelector` and `LlmsTxtOptions.ContentSelector` to `#main-content` with no override. The current shape exposes `SearchIndexContentSelector` and `LlmsTxtContentSelector` on `DocSiteOptions`; both default to `#main-content` (because that is what the stock layout wraps the article in) but accept any CSS selector, including the empty string to index the full body when the layout has been replaced. Older how-tos and TOC notes still describe these as hard caps — they are now soft defaults that happen to match the stock layout.

To summarize the current caps precisely: one markdown source registration, extendable via `ConfigurePennington` or by dropping a level; a fixed theme composition (`AddMonorailCss` plus the DocSite layout shell) that is tweakable but not swappable; and a fixed slot renderer and `App` component that can only be extended through additional routing assemblies, not replaced.

### The escape hatch — DocSite's source as reference

When the caps start to bind, the productive move is not to fight `DocSiteOptions` but to copy the pattern out of `DocSiteServiceExtensions.AddDocSite` and paste what is needed into a bare `AddPennington` host. That method runs around 60 lines of composition with no hidden state: the service registrations, the option forwarding, and the middleware order are all visible. Reading it gives a checklist of what a Pennington-shaped host needs, one that can be freely subsetted or extended.

The `ExtensibilityLabExample` project serves as the canonical bare-host reference. It wires extension points directly against `AddPennington`, uses its own markdown rendering via `MapGet`, and demonstrates that the engine runs happily without the DocSite template. For hosts whose shape the template does not fit, `examples/ExtensibilityLabExample/Program.cs` is a better starting skeleton than a blank `WebApplication.CreateBuilder`.

The example host is intentionally minimal — no Razor layout, no slot renderer, hand-written HTML strings. It demonstrates the engine surface, not a production layout pattern. The DocSite extension method is the right place to look for the layout recipe.

### Signals that point toward bare AddPennington

A few shapes genuinely do not fit the template. Recognizing them early saves the cost of learning both surfaces in sequence:

- Multiple markdown front-matter types served from the same host with different themes or different layouts. `ConfigurePennington` can register a second source, but it cannot give that source a separate layout shell.
- Replacing the `App` component or the article slot renderer with a layout that is not article-shaped — a dashboard, a directory, a storefront.
- A non-Razor rendering story: custom `MapGet` handlers, Minimal API endpoints that emit HTML strings, or a reverse-proxy shape where the engine feeds a different front-end entirely.
- Embedding Pennington inside an existing ASP.NET app that already owns its routing, authentication, or layout conventions. Adding `AddDocSite` on top tends to fight those choices rather than cooperate with them.
- Shipping the engine as a library into another product where only the pipeline is needed, not the layout.

When none of those shapes describes the work, `AddDocSite` is almost certainly the right starting point. The caps are narrow because the template is opinionated, and those opinions cover the large majority of documentation-site shapes.

## Trade-offs

`AddDocSite` and `AddPennington` are two APIs with two learning surfaces. A host that starts on the template and later needs bare access has to learn both — the template's options, and then the underlying options the template was composing over. Pennington accepts this cost because the alternative — `AddPennington` with twenty configuration callbacks collapsed into one record — makes the common case noisier in exchange for making the advanced case fractionally more discoverable.

A single `AddPennington` with a `UseDefaultDocSiteLayout = true` flag was considered and set aside. It would force every bare-host consumer to carry the layout code in their dependency graph — Razor components, MonorailCSS configuration — even when the layout is unused. It also conflates "engine configuration" with "layout configuration" in one record that then has to explain which knobs apply in which mode.

The consequence of that decision is that `DocSiteOptions` is a curated subset of `PenningtonOptions` on purpose, and the design intent is for it to stay that way. New DocSite-specific knobs land when the template needs them; new engine knobs land on `PenningtonOptions`; and `ConfigurePennington` is the bridge when an engine knob is needed from inside a template-shaped host. The tradeoff is a clean separation that takes knowing which layer a given option belongs to, rather than one surface that tries to hold everything.

## Further reading

- Reference: [`DocSiteOptions` reference](xref:reference.api.doc-site-options) — the full list of tweak points with defaults and forwarding semantics.
- How-to: [Use multiple content sources](xref:how-to.configuration.multiple-sources) — the canonical guide for the "two front-matter types" case, covering both the `ConfigurePennington` path and the drop to bare `AddPennington`.
- How-to: [Override DocSite components](xref:how-to.extensibility.override-docsite-components) — the template-compatible extension path for layout tweaks before dropping a level.
