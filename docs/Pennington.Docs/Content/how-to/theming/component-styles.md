---
title: "Override component styles"
description: "Replace individual utility classes on the sidebar navigation and outline rail through the style registry — Tailwind-aware merging keeps the classes you don't touch."
uid: how-to.theming.component-styles
order: 4
sectionLabel: "Theming"
tags: [styling, theming, style-registry, docsite]
---

To restyle a piece of template chrome — the sidebar's link color, the outline rail's typography — set the `Styles` dictionary on `DocSiteOptions` or `BlogSiteOptions`. Each entry targets one named slot in the style registry, and the value is merged over the slot's default with Tailwind-aware conflict resolution: utilities that conflict with yours are replaced, everything else survives. You change the color without retyping the padding, transitions, and state variants.

## Before you begin
- A running DocSite or BlogSite host (see <xref:tutorials.getting-started.first-site> if not).
- The slot key you want to change — run `dotnet run -- diag styles` to list every slot with its current classes, or browse the `StyleKeys` constants in `Pennington.UI.Styling`.

## Override a slot

Add entries to `Styles` keyed by `StyleKeys` constants (raw strings like `"toc.link"` work too):

```csharp
using Pennington.UI.Styling;

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    SiteDescription = "Documentation",
    Styles = new()
    {
        [StyleKeys.TocLink] = "text-emerald-600 dark:text-emerald-400",
    },
});
```

The default `toc.link` slot carries layout, transition, hover, and `data-[current=true]` classes alongside its text color. The override above replaces only the conflicting color utilities; the rest render unchanged. An unknown key throws at startup with the full list of valid keys, so typos surface immediately.

## Inspect the result

`diag styles` prints every slot, the layer that supplied it (`default`, `skin`, or `override`), and the effective classes — for overridden slots it also shows the base and override inputs:

```shell
dotnet run -- diag styles
```

```text
toc.link  override  flex items-center gap-1.5 px-2.5 py-1.5 ... text-emerald-600 dark:text-emerald-400
          base:     flex items-center gap-1.5 px-2.5 py-1.5 ... text-base-500 dark:text-base-400 ...
          override: text-emerald-600 dark:text-emerald-400
```

## Slot reference

TOC slots style the DocSite sidebar's `TableOfContentsNavigation`; outline slots style the `OutlineNavigation` rail. Each slot covers one rendered element — layout, color, and state variants together — so any single utility can be replaced without restating the rest.

| Key | Styles |
| --- | ------ |
| `toc.list` | The outer list — layout and the gap between top-level entries |
| `toc.section` | Each top-level list item (empty by default — a hook for per-section margins) |
| `toc.section-title` | A section's label — the plain heading, or the link when a top-level entry has children |
| `toc.section-list` | The nested list holding a section's entries |
| `toc.link` | Child-level links, including the `data-[current=true]` state |
| `toc.top-link` | Top-level links without children, including the `data-[current=true]` state |
| `outline.title` | The "On this page" eyebrow |
| `outline.container` | The outline's outer container |
| `outline.marker` | The moving highlight bar that tracks the active heading |
| `outline.list` | The outline list |
| `outline.link` | Each generated outline link, including the `data-[selected=true]` state |
| `outline.nested-link` | Extra classes appended to nested (H3-level) outline links |

A few classes never appear in any slot because the outline script depends on them: `relative` on the container, and `absolute` plus the `opacity-0`/`opacity-100` toggle on the marker. Overrides can't break the highlight behavior.

## Merge rules worth knowing

- **`dark:` variants don't conflict with bare utilities.** `text-emerald-600` replaces `text-base-500` but leaves `dark:text-base-400` standing — supply a `dark:` class when the dark palette should change too.
- **Unknown utilities pass through.** Custom utilities the merger doesn't recognize (`scrollbar-*`, `card-tint-*`) are kept on both sides.
- **Write values as string literals.** The stylesheet generator discovers classes by scanning compiled string literals; a class name assembled at runtime never makes it into `/styles.css`. Concatenating literals with `+` is fine — the compiler folds it.

> [!NOTE]
> Per-component parameters follow the same rule. Passing `LinkClass` directly to `TableOfContentsNavigation` in a custom layout merges that value over the resolved slot for that one instance — `Styles` changes every instance, a parameter changes one.
