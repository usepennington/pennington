---
title: "What the DocSite and BlogSite templates wire for you"
description: "DocSite and BlogSite are pre-assembled shortcuts on top of the AddPennington engine — what each template wires, and where you have to drop down to the engine itself."
uid: explanation.positioning.docsite-positioning
order: 1
sectionLabel: "Positioning"
tags: [docsite, blogsite, templates, architecture]
---

`AddDocSite` and `AddBlogSite` are shortcuts. Each one pre-assembles a host that `AddPennington` would otherwise have you wire by hand. This page is about what they assemble — and where that assembly stops.

## Context

`AddPennington` is the engine, and building on it is how Pennington sites are made: content discovery, the rendering pipeline, response processing, and diagnostics. It expects the host to bring its own layout, routing, and CSS wiring — which the [getting-started tutorials](xref:tutorials.getting-started.first-site) walk through end to end.

`AddDocSite` and `AddBlogSite` are templates layered on that engine. Each composes `AddPennington`, `AddMonorailCss` (Pennington's integration of [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/), the Tailwind-compatible .NET JIT compiler), and a Razor `App` component into a single call with a small options surface — `DocSiteOptions` or `BlogSiteOptions`. They exist to skip the wiring for two shapes that come up often: a Divio-style documentation site, and a site where the blog *is* the site. When a site is exactly one of those, the template is a real head start. When it is not, the host is built on `AddPennington` directly, which is a normal way to build a Pennington site.

The distinction that matters is that neither options record is a mirror of `PenningtonOptions`. Each is a curated subset plus a handful of template-specific options, and the things it deliberately does not expose mark the edge of what the template covers.

### One template per host

`AddDocSite` and `AddBlogSite` cannot run in the same app — a host wires one or the other, never both. The constraint is structural: each call registers its own Razor `App` component through `MapRazorComponents<App>`, claims the root route `/`, and composes its own MonorailCSS theme. Two templates in one host means two `App` components and two pages fighting for `/`, which is not a configuration the engine resolves. Pick the template that matches the site's primary shape.

The split is rarely a hard choice, because the overlap has a built-in answer. A documentation site that also wants a blog stays on DocSite: it has a native blog you switch on by adding a `Content/blog/` folder, with no `Program.cs` change. BlogSite is the right choice only when the blog *is* the site — its home page, archive, and tag routes are the whole front end. When a host genuinely needs both a doc-shaped and a blog-shaped surface that the native blog cannot express, the answer is not two templates but `AddPennington` directly.

The rest of this page examines DocSite in detail, then returns to where BlogSite differs. BlogSite is the same kind of shortcut with a narrower options surface, and most of the reasoning carries over.

## How it works

### What DocSite gives you for free

A single `AddDocSite` call wires quite a bit:

- The Pennington engine, with site title, description, canonical URL, and content root forwarded from `DocSiteOptions`.
- One `AddMarkdownContent<DocSiteFrontMatter>` registration rooted at the content folder.
- A pre-scoped `llms.txt` and search index, both defaulting to `#main-content`, the wrapper around the stock article.
- The Razor `App` component and `DocSiteArticle` rendering shell.
- Mdazor inline components — `Badge`, `Card`, `Step`, and others — registered so markdown can embed UI without per-site plumbing.
- MonorailCSS with the DocSite theme.
- SPA navigation through the `data-spa-region` markup the layout emits, with no extra DI registration — the client script lives in `Pennington.UI`.

The value is less about the feature count than about ordering: every one of those registrations lands in a compatible order with the others. Getting that ordering right, especially between the pipeline, the response processor, and the search index scoping, is most of what trips up a hand-rolled host on the first attempt.

### What DocSite caps

DocSite owns exactly one `AddMarkdownContent<DocSiteFrontMatter>` registration. Wiring a second front-matter type — say, a blog post shape alongside docs — is not reachable by setting a `DocSiteOptions` property. It takes either the [`ConfigurePennington`](xref:reference.api.doc-site-options) callback (a `Action<PenningtonOptions>` that runs after DocSite's defaults land) or dropping to bare `AddPennington` outright. The callback is enough for adding one more source. It falls short when the extra source needs different theming, a different layout, or a different slot renderer.

`DocSiteOptions.ColorScheme`, `DisplayFontFamily`, `BodyFontFamily`, `ExtraStyles`, and `CustomCssFrameworkSettings` offer tweak points against MonorailCSS, but the theme composition itself — `AddMonorailCss` plus the DocSite `App` component plus the `DocSiteArticle` shell — is fixed. Replacing the `App` component or introducing a non-article layout means registering extra routing assemblies via `AdditionalRoutingAssemblies` and accepting that custom components ride alongside DocSite's, not in place of them.

`ContentSelector` on `DocSiteOptions` defaults to `#main-content` — the wrapper the stock layout places around the article — and accepts any CSS selector, including the empty string to index the full body when the layout has been replaced. That one selector picks the body element that the search index, the llms.txt sidecars, and the build-time link audit all consume, so chrome is stripped once and all three read the same element.

### The escape hatch — DocSite's source as reference

When a site outgrows what the options expose, the productive move is not to fight `DocSiteOptions` but to copy the pattern out of `DocSiteServiceExtensions.AddDocSite` and paste what is needed into a bare `AddPennington` host. That method is a single composition with no hidden state: the service registrations, the option forwarding, and the middleware order are all visible. Reading it gives a checklist of what a Pennington-shaped host needs, one that can be freely subsetted or extended.

The [`ExtensibilityLabExample`](https://github.com/usepennington/pennington/tree/main/examples/ExtensibilityLabExample) project serves as the canonical bare-host reference. It wires extension points directly against `AddPennington`, uses its own markdown rendering via `MapGet`, and demonstrates that the engine runs happily without the DocSite template. For hosts whose shape the template does not fit, `examples/ExtensibilityLabExample/Program.cs` is a better starting skeleton than a blank `WebApplication.CreateBuilder`.

The example host is intentionally minimal — no Razor layout, no slot renderer, hand-written HTML strings. It demonstrates the engine surface, not a production layout pattern. The DocSite extension method is the right place to look for the layout recipe.

For a documented walkthrough rather than source to read, the [first-site tutorial](xref:tutorials.getting-started.first-site) builds a bare `AddPennington` host step by step — the same path the templates are a shortcut for — and is the recipe to follow when a host needs its own layout and routing from the start.

### The shape the template assumes

A template fits a particular kind of site. DocSite fits an article-centric documentation site rendered through a fixed Razor layout. A handful of site types fall outside it — and there the host is built on the engine, not the template:

- Multiple markdown front-matter types served from the same host with different themes or different layouts. `ConfigurePennington` can register a second source, but it cannot give that source a separate layout shell.
- Replacing the `App` component or the `DocSiteArticle` shell with a layout that is not article-shaped — a dashboard, a directory, a storefront.
- A non-Razor rendering story: custom `MapGet` handlers, Minimal API endpoints that emit HTML strings, or a reverse-proxy shape where the engine feeds a different front-end entirely.
- Embedding Pennington inside an existing ASP.NET app that already owns its routing, authentication, or layout conventions. Adding `AddDocSite` on top tends to fight those choices rather than cooperate with them.
- Shipping the engine as a library into another product where only the pipeline is needed, not the layout.

None of these is exotic. They are the common reason a host is built on `AddPennington` directly. The limits are narrow because the template is opinionated, and those opinions are about documentation sites specifically.

## Where BlogSite differs

BlogSite is the same kind of shortcut, aimed at a different shape: a site where the blog *is* the site. A single `AddBlogSite` call composes `AddPennington`, forwards site identity and content paths from `BlogSiteOptions`, registers one `AddMarkdownContent<BlogSiteFrontMatter>` source rooted at `Content/Blog/`, wires MonorailCSS with the BlogSite theme, and registers the same Mdazor inline components DocSite does. The same ordering value holds — every registration lands compatible with the others, which is the part a hand-rolled host gets wrong first.

What BlogSite adds beyond DocSite is its route surface. The template ships Home, Archive, Tag, Tags, and Blog Razor pages inside `Pennington.BlogSite.dll`, so the home listing, `/archive`, `/blog/<slug>/`, the tag pages, and the `/rss.xml` feed exist without the host authoring a single `@page`. That is the inverse of DocSite, whose pages all come from markdown under `Content/`; BlogSite's structural pages are compiled into the template and its content is the posts you drop in.

The caps are tighter than DocSite's in two ways. First, `BlogSiteOptions` has no `ConfigurePennington` callback — the post-defaults `Action<PenningtonOptions>` hook DocSite exposes. Reaching engine surface the options do not forward means dropping to bare `AddPennington` rather than threading a callback. Second, there is no `ContentSelector`: the body element the search index and llms.txt consume is fixed by the BlogSite layout rather than selectable. The tweak points it does share with DocSite — `ColorScheme`, `ExtraStyles`, `DisplayFontFamily`, `BodyFontFamily` — adjust the theme without swapping the composition, exactly as on DocSite. What `BlogSiteOptions` adds instead are blog-shaped knobs: author chrome (`AuthorName`, `AuthorBio`), homepage composition (`HeroContent`, `MyWork`, `Socials`), and feed toggles (`EnableRss`, `EnableSitemap`). The full surface is in the [`BlogSiteOptions` reference](xref:reference.api.blog-site-options).

The escape hatch is identical in spirit. `BlogSiteServiceExtensions.AddBlogSite` is a single visible composition, and when a site outgrows the options the move is again to copy what is needed into a bare `AddPennington` host rather than fight the template.

## Further reading

- Tutorial: [Create your first Pennington site](xref:tutorials.getting-started.first-site) — building a host on `AddPennington` directly, the path the templates are a shortcut for.
- Reference: [`DocSiteOptions` reference](xref:reference.api.doc-site-options) — the full list of tweak points with defaults and forwarding semantics.
- Reference: [`BlogSiteOptions` reference](xref:reference.api.blog-site-options) — the BlogSite option surface, including the homepage and feed knobs DocSite has no equivalent for.
- How-to: [Use multiple content sources](xref:how-to.discovery.multiple-sources) — the canonical guide for the "two front-matter types" case, covering both the `ConfigurePennington` path and the drop to bare `AddPennington`.
- How-to: [Override DocSite components](xref:how-to.response-pipeline.override-docsite-components) — the template-compatible extension path for layout tweaks before dropping a level.
