---
title: "The head subsystem"
description: "Why everything that writes to the document head — title, canonical, JSON-LD, OpenGraph, alternates, Standard Site links — funnels through one typed model, one rewriter that finalizes it, and a shared `data-head` attribute."
uid: explanation.core.head-subsystem
order: 6
sectionLabel: "Core Architecture"
tags: [head, contributors, response-processing, spa]
---

A surprising number of concerns want to write into the document `<head>`: the title and description, the canonical link, schema.org JSON-LD, OpenGraph and Twitter card meta, RSS and llms.txt alternates, `hreflang` locale alternates, the dev-host meta that live reload reads, and the Standard Site verification links. Why does Pennington route all of them through a single `IHeadContributor` extension point instead of letting each one emit its own markup where it already lives?

## Context

Before the head subsystem, those writers were spread across four unrelated mechanisms. Some were literal markup in each template's `App.razor` head. Two were head-scoped `IHtmlResponseRewriter`s — one for the canonical link, one for JSON-LD. Per-page meta was authored in Razor `<HeadContent>` blocks. The dev-host meta was a raw string insert that searched the response for `</head>`.

Four mechanisms meant four ways to reason about ordering, four places a duplicate `og:image` or a second `<title>` could slip through, and no shared notion of "this tag is already present, leave it alone." The ordering was the worst of it: the rewriters carried hand-picked integers that unrelated writers silently collided on, and nothing connected a rewriter's order to the literal markup it had to interleave with.

The worst of it showed up on the client. Pennington's [SPA navigation swaps page regions](xref:explanation.spa.islands) rather than reloading, so the head has to be reconciled in JavaScript on every soft navigation. The old client carried a hand-maintained allowlist naming exactly which head tags to carry across a swap. Every new head tag had to be added to that list by hand, and forgetting meant the tag silently vanished the moment a visitor clicked a link — a failure invisible on first paint and easy to ship.

## How it works

The subsystem has four parts: a typed data model, the `IHeadContributor` extension point, a single rewriter that finalizes the head, and a `data-head` attribute that ties the server and client halves together.

### The typed model

Everything that can land in the head is a typed `HeadTag` — a union of `TitleTag`, `MetaNameTag`, `MetaPropertyTag`, `LinkTag`, `ScriptTag`, and a `RawTag` case for markup the engine does not model. Each tag that should appear at most once carries a stable `HeadTagKey`: `title`, `meta:prop:og:image`, `link:rel:canonical`, and so on. Repeatable tags — `hreflang` alternates, JSON-LD blocks, preloads, the Standard Site links — carry no key and always append.

The key is what makes dedup work. A `HeadBuilder` keeps a keyed map where the first add at a key wins and later same-key adds are dropped, alongside a keyless list for repeatables.

```csharp:symbol,signatures
src/Pennington/Head/HeadBuilder.cs > HeadBuilder
```

### The contributor extension point

A contributor is an ordered, gated unit — the same form as the `IHtmlResponseRewriter` it often replaces.

```csharp:symbol
src/Pennington/Head/IHeadContributor.cs > IHeadContributor
```

`Order` does double duty. Contributors run lowest-first, and on a key collision the lowest order wins — so a page-level tag beats a site-level default for the same key without either side knowing about the other. To keep those numbers from drifting back into ad-hoc collisions, `Order` is chosen from named bands rather than raw integers:

```csharp:symbol,bodyonly
src/Pennington/Head/HeadOrder.cs > HeadOrder
```

A page-OpenGraph contributor at `Page` (40) and a site-default contributor at `Site` (60) can both try to set `og:image`; the page wins because it ran first, and the site default steps aside through the same dedup that would have collapsed two identical tags. The bands encode the precedence relationship that the old hand-picked integers only implied.

### The composition rewriter

A single rewriter, `HeadCompositionHtmlRewriter`, is the only place head tags are finalized. It runs inside the shared AngleSharp pass described in [the response-processing explanation](xref:explanation.core.response-processing) — so composing the entire head costs no extra parse or serialize, just another mutation of the already-parsed document.

Its order matters here for a subtle reason. It sits between locale rewriting and base-URL prefixing in the [shared rewriter chain](xref:explanation.core.response-processing), so any root-relative `href` a contributor emits — an asset, an alternate link — gets sub-path prefixed by the base-URL rewriter exactly as literal head markup would. A contributor never has to know the deployment base URL; it emits `/rss.xml` and the downstream rewriter handles the rest. That slot is the rewriter's own `Order`:

```csharp:symbol
src/Pennington/Head/HeadCompositionHtmlRewriter.cs > HeadCompositionHtmlRewriter.Order
```

The rewriter does two things in sequence. First it composes: it runs every registered contributor whose `ShouldContribute` returns true, lowest order first, into one `HeadBuilder`, then reconciles the built tags into the document head. Tags whose keys a page already authored are left untouched — contributors fill gaps, they do not overwrite page intent. Second, it normalizes what the page authored itself.

### The data-head attribute

Every finalized head element — whether a contributor emitted it or a page authored it — carries a `data-head` attribute. That one attribute drives both halves of the system. Server-side, it marks which elements the reconciler owns. Client-side, it collapses the old allowlist into one generic sweep: on a soft navigation, remove every `[data-head]` element and clone every `[data-head]` element from the fetched document. Every future head tag survives navigation automatically, because surviving is now a property of the attribute, not of a list someone has to remember to update.

This is also why page authoring keeps working. A page that writes `<PageTitle>` or a `<HeadContent>` block still renders through Blazor's `HeadOutlet`; the rewriter's normalization step pulls those rendered tags into the same model, stamps them, and dedups them against contributor output — with page authorship winning on a key collision. Markup the engine does not recognize passes through as a `RawTag`, untouched. So `<HeadContent>` does not compete with the contributors; it is one more input feeding the same model.

### The deliberate exception

Two things stay as literal markup on purpose: the theme-bootstrap inline `<script>` and the stylesheet link. Both are about avoiding a flash. The theme script must run before first paint to apply dark mode, so it cannot wait for a rewriter that runs after the document is built; the stylesheet stays put because the SPA's stylesheet sync deliberately never removes an existing sheet (removing and re-adding it flashes the unstyled page). The subsystem owns meta and discovery tags — the things whose ordering and dedup were the actual problem — and leaves the two pre-paint assets where they have to be.

## Why one subsystem rather than leaving writers where they lived

The alternative — every concern emits its own head markup at the site it already occupies — is what the codebase had, and it failed in three specific ways the consolidation fixes. Dedup had no home, so two writers targeting `og:image` produced two tags. Ordering lived in scattered integers and template line numbers with no shared scale, so precedence between a page tag and a site default was implicit and fragile. And client reconciliation depended on a hand-maintained list, so the cost of adding a head tag included a second, easy-to-forget edit in JavaScript.

The typed model gives dedup a key to work on, the single rewriter gives ordering one scale and one finalization point, and the `data-head` attribute gives the client a rule instead of a list. The price is indirection: a tag that used to be three lines of Razor is now a small class registered in DI. For a one-off tag on a single page that price is real, which is why `<HeadContent>` still works and is still the right tool for genuinely page-local markup. For anything cross-cutting — anything that needs to dedup, order against other writers, or survive navigation — a contributor is worth the indirection.

## Further reading

- How-to: [Add tags to the document head](xref:how-to.response-pipeline.head-contributor) — write and register an `IHeadContributor`.
- Related explanation: [The response-processing pipeline](xref:explanation.core.response-processing) — the shared AngleSharp pass the rewriter runs inside.
- Related explanation: [SPA navigation through region swaps](xref:explanation.spa.islands) — why the head needs client-side reconciliation at all.
