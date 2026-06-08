---
title: Page context
description: A component that reads the current file name, URL, and front matter from MdazorContext instead of tag attributes.
order: 30
---

Tag attributes aren't the only way data reaches a component. Pennington hands
every page a read-only `MdazorContext` — the source file, the canonical URL, the
page's front matter, and any derived metadata — that a component reads through a
`[CascadingParameter]`, with nothing written on the tag.

The `<PageFacts />` component is registered in `Program.cs` next to `PricingCard`
and reads three values straight from the page it is rendering on:

<PageFacts />

Every line above is supplied by the host, not by attributes on the tag. The whole
component is a cascading parameter and a couple of lookups:

```razor
@using Mdazor
@using Pennington.FrontMatter

<ul>
    <li>Source file: @Context?["FileName"]</li>
    <li>Canonical URL: @Context?["Url"]</li>
    <li>Front-matter title: @Title</li>
</ul>

@code {
    [CascadingParameter] public MdazorContext? Context { get; set; }

    // The "Metadata" key carries the page's parsed front matter as an IFrontMatter.
    private string? Title => (Context?["Metadata"] as IFrontMatter)?.Title;
}
```

`MdazorContext` exposes the bag through `Values`, an indexer (`Context["FileName"]`),
`TryGet`, and `Get<T>`. Pennington fills it with `SourceFile`, `FileName`,
`FileNameWithoutExtension`, `Url`/`CanonicalPath`, `OutputFile`, `Locale`, the
front-matter `Metadata` object, and the `Derived` enricher dictionary — all keyed
case-insensitively.

Because the context is delivered as a cascading value, it reaches a component and
any components nested inside it. It does not cross into an interactive
(WebAssembly/Server) island, so use it from the statically rendered components that
make up the page body.
