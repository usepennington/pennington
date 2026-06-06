---
title: "Add tags to the document head"
description: "Implement IHeadContributor to emit title, meta, link, or script tags into <head> with central deduplication, band-based ordering, and automatic SPA-navigation survival."
uid: how-to.response-pipeline.head-contributor
order: 5
sectionLabel: "Response Pipeline"
tags: [head, contributors, extensibility, response-pipeline]
---

To add a head tag that deduplicates against other writers, orders predictably against site and page defaults, and survives SPA navigation, implement `IHeadContributor`. For genuinely page-local markup that no other writer touches, a Razor `<HeadContent>` block on the page is still the right tool — the head reconciler normalizes it into the same model. Reach for a contributor when the tag is cross-cutting: emitted on many pages, or competing with another writer for the same slot. For background on why the head funnels through one seam, see <xref:explanation.core.head-subsystem>.

## Before you begin

- An existing Pennington site. `AddPennington` (and therefore `AddDocSite`/`AddBlogSite`) already registers the head composition rewriter, so contributors activate as soon as you register one.
- A sense of which slot the tag occupies: does it appear at most once (a `<title>`, a canonical link, one `og:image`), or can it repeat (alternates, JSON-LD)? That choice drives whether you add it with a dedup key or as a repeatable.

## Write the contributor

Implement `IHeadContributor` as a sealed class. The interface is three members:

```csharp:symbol,bodyonly
src/Pennington/Head/IHeadContributor.cs > IHeadContributor
```

Push tags through the `HeadBuilder` handed to `ContributeAsync`. Its helpers cover the common cases — `Title`, `Meta` (name/content), `Property` (OpenGraph), and `Link` (rel/href) each add under a dedup key, while `AddRepeatable` appends a tag that may occur more than once.

```csharp:symbol,signatures
src/Pennington/Head/HeadBuilder.cs > HeadBuilder
```

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

## Pick an order band

`Order` is chosen from the `HeadOrder` bands, not a raw integer. Contributors run lowest-first, and on a dedup-key collision the lowest order wins — so a tag in a lower band overrides the same key in a higher one.

```csharp:symbol,bodyonly
src/Pennington/Head/HeadOrder.cs > HeadOrder
```

Use `Page` (40) for tags computed from the current page that should beat site defaults, `Site` (60) for site-wide defaults, and `Discovery` (80) for structured-data and verification payloads. The generator meta above sits at `Site` because it is a constant site default with no page-level override.

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

`HeadContext` carries the request and the content record resolved for it, so a contributor can compute tags from the page's front matter.

```csharp:symbol,bodyonly
src/Pennington/Head/HeadContext.cs > HeadContext
```

`Record` is `null` on endpoint and 404 pages, so guard it. `FullPath` is the request path with the locale segment reattached — the same key the content registry and structured-data join on.

### Override a built-in tag

To replace a tag a built-in contributor emits, add the same key from a lower band. A page-level OpenGraph contributor at `Page` overrides the `Site`-band default for `meta:prop:og:image` purely through the lowest-order-wins rule — neither contributor references the other. The site default's own dedup makes it step aside.

### Keep authoring in Razor

A `<HeadContent>` or `<PageTitle>` block on a page still works. The reconciler pulls whatever `HeadOutlet` rendered into the same model, stamps it, and dedups it against contributor output — with the page winning on a key collision. Use Razor for one-off, page-local head markup; use a contributor when the tag is shared or needs to beat another writer.

## Verify

- Run `dotnet run` and view-source on any page. The contributed tag is present and carries a `data-head` attribute (the stamp that drives dedup and SPA-navigation survival).
- Navigate between pages without a full reload and confirm the tag is still present — the generic `[data-head]` sweep carries it across the region swap with no per-tag wiring.
- Static build: `dotnet run -- build output` and grep an output HTML file to confirm the tag is emitted at publish time too.

## Related

- Background: [The head subsystem](xref:explanation.core.head-subsystem) — the model, ordering bands, and `data-head` invariant this how-to builds on.
- Related how-to: [Rewrite HTML attributes after parsing](xref:how-to.response-pipeline.html-rewriter) — for whole-document edits outside the head.
- Reference: [`DocSiteOptions` and host extensions](xref:reference.host.extensions)
