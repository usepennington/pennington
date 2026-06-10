---
title: "Add tags to the document head"
description: "Implement IHeadContributor to emit title, meta, link, or script tags into <head> with central deduplication, band-based ordering, and automatic SPA-navigation survival."
uid: how-to.response-pipeline.head-contributor
order: 5
sectionLabel: "Response Pipeline"
tags: [head, contributors, extensibility, response-pipeline]
---

To add a head tag that deduplicates against other writers, orders predictably against site and page defaults, and survives SPA navigation, implement `IHeadContributor`. Reach for a contributor when the tag is shared across pages — emitted on many pages, or competing with another writer for the same slot. For one-off, page-local markup there are two lighter options instead: a Razor `<HeadContent>` block (see [Keep authoring in Razor](#keep-authoring-in-razor)) or, on a DocSite, the `AdditionalHtmlHeadContent` string (see [Customize the DocSite chrome](xref:how-to.response-pipeline.override-docsite-components)). For background on why the head funnels through one extension point that every writer goes through, see <xref:explanation.core.head-subsystem>.

## Before you begin

- An existing Pennington site. `AddPennington` (and therefore `AddDocSite`/`AddBlogSite`) already registers the head composition rewriter, so contributors activate as soon as you register one.
- A sense of which slot the tag occupies: does it appear at most once (a `<title>`, a canonical link, one `og:image`), or can it repeat (alternates, JSON-LD)? That choice drives whether you add it with a dedup key or as a repeatable.

## Write the contributor

Implement `IHeadContributor` as a sealed class. The interface is three members — `Order`, `ShouldContribute`, and `ContributeAsync` (see [`IHeadContributor`](xref:reference.api.i-head-contributor) for the member catalog).

Push tags through the `HeadBuilder` handed to `ContributeAsync`. Its helpers cover the common cases — `Title`, `Meta` (name/content), `Property` (OpenGraph), and `Link` (rel/href) each add under a dedup key, while `AddRepeatable` appends a tag that may occur more than once (see [`HeadBuilder`](xref:reference.api.head-builder) for the full surface).

A minimal contributor that stamps a site-wide `generator` meta tag on every page:

```csharp
using Pennington.Head;

internal sealed class GeneratorMetaHeadContributor : IHeadContributor
{
    public int Order => HeadOrder.Site;

    public bool ShouldContribute(HeadContext context) => true;

    public Task ContributeAsync(HeadContext context, HeadBuilder head)
    {
        head.Meta("generator", "Pennington");
        return Task.CompletedTask;
    }
}
```

The page authored no generator tag, so before composition its `<head>` carries none:

```html
<head>
  <title>Getting started</title>
  <!-- no generator meta -->
</head>
```

After composition the rewriter appends the contributed tag and stamps it with `data-head` — the value is the dedup key, here `meta:name:generator`:

```html
<head>
  <title data-head="title">Getting started</title>
  <meta name="generator" content="Pennington" data-head="meta:name:generator">
</head>
```

That `data-head` stamp is what later same-key contributors dedup against and what the SPA engine carries across a soft navigation.

## Pick an order band

`Order` is chosen from the `HeadOrder` bands, not a raw integer. Contributors run lowest-first, and on a dedup-key collision the lowest order wins — so a tag in a lower band overrides the same key in a higher one.

Use `Page` (40) for tags computed from the current page that should beat site defaults, `Site` (60) for site-wide defaults, and `Discovery` (80) for structured-data and verification payloads. The generator meta above sits at `Site` because it is a constant site default with no page-level override. See [`HeadOrder`](xref:reference.api.head-order) for the complete band list with values.

## Register it

Register with `AddHeadContributor<T>()` after the host wiring. Registration is transient, which is what you want for a contributor that reads file-watched state such as the content registry.

```csharp
builder.Services.AddDocSite(() => new DocSiteOptions { /* … */ });
builder.Services.AddHeadContributor<GeneratorMetaHeadContributor>();
```

## Options

### Emit a repeatable tag

When a tag can appear more than once — an `hreflang` alternate, a JSON-LD block, an RSS alternate — add it with `AddRepeatable` and no key, building the `LinkTag`/`ScriptTag` directly. The shipped RSS-alternate contributor shows the shape, including extra attributes in emission order:

```csharp:symbol
src/Pennington.BlogSite/BlogSiteHeadContributor.cs > BlogSiteHeadContributor.ContributeAsync
```

### Gate with `ShouldContribute`

Return `false` from `ShouldContribute` to skip a contributor entirely for a request. This is the cheap precondition check — a missing config value, a feature flag, a content type that should not carry the tag. The canonical-link contributor self-gates on whether a base URL is configured, so registering it unconditionally is harmless:

```csharp:symbol
src/Pennington/Head/Contributors/CanonicalHeadContributor.cs > CanonicalHeadContributor.ShouldContribute
```

### Read the resolved page record

`HeadContext` carries the request and the content record resolved for it, so a contributor can compute tags from the page's front matter. Its members are `HttpContext`, `FullPath`, and the nullable `Record` (see [`HeadContext`](xref:reference.api.head-context)).

`Record` is `null` on endpoint and 404 pages, so guard it. `FullPath` is the request path with the locale segment reattached — the same key the content registry and structured-data join on.

### Override a built-in tag

To replace a tag a built-in contributor emits, add the same key from a lower band. A page-level OpenGraph contributor at `Page` overrides the `Site`-band default for `meta:prop:og:image` purely through the lowest-order-wins rule — neither contributor references the other. The site default's own dedup makes it step aside.

### Keep authoring in Razor

A `<HeadContent>` or `<PageTitle>` block on a page still works. The reconciler pulls whatever `HeadOutlet` rendered into the same model, stamps it, and dedups it against contributor output — with the page winning on a key collision.

There are three routes to "add a head tag", and the decision rule is which scope owns it: a contributor for anything shared across pages or competing for a slot (it dedups, orders, and survives navigation); a Razor `<HeadContent>` block for one-off markup local to a single page; and, on a DocSite, [`AdditionalHtmlHeadContent`](xref:how-to.response-pipeline.override-docsite-components) for a raw site-wide string (analytics snippets, preconnect hints) you do not want to write a class for. The string route runs through the same head reconciler, so its tags also get a `data-head` stamp.

## Verify

- Run `dotnet run` and view-source on any page. The contributed tag is present and carries a `data-head` attribute (the stamp that drives dedup and SPA-navigation survival) — for the generator example above, expect `<meta name="generator" content="Pennington" data-head="meta:name:generator">`.
- Navigate between pages without a full reload and confirm the tag is still present — the generic `[data-head]` sweep carries it across the region swap with no per-tag wiring.
- Static build: `dotnet run -- build output`, then grep a published page for the stamped tag to confirm it ships at publish time too — `grep 'data-head="meta:name:generator"' output/index.html`.

## Related

- Background: [The head subsystem](xref:explanation.core.head-subsystem) — the model, ordering bands, and `data-head` invariant this how-to builds on.
- Related how-to: [Rewrite HTML attributes after parsing](xref:how-to.response-pipeline.html-rewriter) — for whole-document edits outside the head.
- Reference: [`DocSiteOptions` and host extensions](xref:reference.host.extensions)
