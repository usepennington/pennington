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

Add entries to `Styles` keyed by `StyleKeys` constants (raw strings like `"toc.link-color"` work too):

```csharp
using Pennington.UI.Styling;

builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Docs",
    SiteDescription = "Documentation",
    Styles = new()
    {
        [StyleKeys.TocLinkColor] = "text-emerald-600 dark:text-emerald-400",
    },
});
```

The default `toc.link-color` slot carries transition, hover, and `data-[current=true]` classes alongside its text color. The override above replaces only the conflicting color utilities; the rest render unchanged. An unknown key throws at startup with the full list of valid keys, so typos surface immediately.

## Inspect the result

`diag styles` prints every slot, the layer that supplied it (`default`, `skin`, or `override`), and the effective classes — for overridden slots it also shows the base and override inputs:

```shell
dotnet run -- diag styles
```

```text
toc.link-color  override  transition-colors duration-150 ... text-emerald-600 dark:text-emerald-400
                base:     transition-colors duration-150 text-base-500 dark:text-base-400 ...
                override: text-emerald-600 dark:text-emerald-400
```

## Slot reference

TOC slots style the DocSite sidebar's `TableOfContentsNavigation`; outline slots style the `OutlineNavigation` rail. Each element splits into a structure slot (layout, spacing, typography) and a color slot (palette and state), so a recolor never has to restate the layout.

| Key | Styles |
| --- | ------ |
| `toc.list-gap` | Gap on the outer list (fixed per-instance in the stock DocSite layout — override has no effect there) |
| `toc.child-list` | The nested list holding a section's entries |
| `toc.section-header-structure` / `toc.section-header-color` | Section headers |
| `toc.link-structure` / `toc.link-color` | Child-level links |
| `toc.root-link-structure` / `toc.root-link-color` | Top-level links without children |
| `outline.title-structure` / `outline.title-color` | The "On this page" eyebrow |
| `outline.container-structure` / `outline.container-color` | The outline's outer container |
| `outline.list-structure` / `outline.list-color` | The outline list |
| `outline.link-structure` / `outline.link-color` | Each generated outline link |

## Merge rules worth knowing

- **`dark:` variants don't conflict with bare utilities.** `text-emerald-600` replaces `text-base-500` but leaves `dark:text-base-400` standing — supply a `dark:` class when the dark palette should change too.
- **Unknown utilities pass through.** Custom utilities the merger doesn't recognize (`scrollbar-*`, `card-tint-*`) are kept on both sides.
- **Write values as string literals.** The stylesheet generator discovers classes by scanning compiled string literals; a class name assembled at runtime never makes it into `/styles.css`. Concatenating literals with `+` is fine — the compiler folds it.

> [!NOTE]
> Per-component parameters still win. Passing `LinkColorClass` directly to `TableOfContentsNavigation` in a custom layout uses that value verbatim for that instance — no merge, no registry.
