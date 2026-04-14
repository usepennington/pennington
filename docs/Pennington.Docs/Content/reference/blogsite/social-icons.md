---
title: "Built-in SocialIcons RenderFragments"
description: "The four static RenderFragment fields on Pennington.BlogSite.Components.SocialIcons and the syntax for plugging them into SocialLink.Icon."
sectionLabel: "BlogSite Built-ins"
order: 20
tags: [blogsite, icons, render-fragment]
uid: reference.blogsite.social-icons
---

> **In this page.** The four static `RenderFragment` fields on `Pennington.BlogSite.Components.SocialIcons` — `GithubIcon`, `LinkedInIcon`, `BlueskyIcon`, `MastodonIcon` — their SVG `viewBox` / `stroke` / `fill` conventions, and the field-reference (not component-tag) syntax used to populate `SocialLink.Icon`.
>
> **Not in this page.** Authoring new icon fragments — see the Razor-component how-to in Extensibility.

## Summary

_**One sentence: what it is.** A component type whose body is empty and whose only surface is four `public static readonly RenderFragment` fields, each a self-contained inline `<svg>` intended for use as the `Icon` on a `SocialLink`._
_**One sentence: where it lives.** Namespace `Pennington.BlogSite.Components`, declared at `src/Pennington.BlogSite/Components/SocialIcons.razor`, consumed by `BlogSiteOptions.Socials` via the `SocialLink` record in namespace `Pennington.BlogSite`._

## Declaration

```csharp:xmldocid
T:Pennington.BlogSite.Components.SocialIcons
```

_Declaration fence reveals the four `static readonly RenderFragment` fields so the reader can see they are field references, not component types — there is no class body to customize and no constructor to invoke._

## Icons

_One row per built-in fragment. All four share the same `xmlns`, `viewBox="0 0 24 24"`, `stroke="currentColor"`, and `fill="none"` opening — rendered at the `color` and sized by the surrounding anchor/container in `MainLayout.razor`. Stroke widths and line joins are fixed on the inner `<path>` elements (`stroke-width="1.5"`, `stroke-linecap="round"`, `stroke-linejoin="round"` where present)._

| Name | `viewBox` | `stroke` | `fill` | Notes |
|---|---|---|---|---|
| `GithubIcon` | `0 0 24 24` | `currentColor` | `none` | _One-sentence: single-path Octocat silhouette; inherits color from the surrounding anchor so hover/focus states theme automatically._ |
| `LinkedInIcon` | `0 0 24 24` | `currentColor` | `none` | _One-sentence: four-path "in" glyph inside a rounded square; stroke-only so background color bleeds through the fill area._ |
| `BlueskyIcon` | `0 0 24 24` | `currentColor` | `none` | _One-sentence: single-path butterfly silhouette following the same stroke-only, `currentColor` convention as the others._ |
| `MastodonIcon` | `0 0 24 24` | `currentColor` | `none` | _One-sentence: two-path elephant-trunk mark; stroke-only with the same `stroke-width="1.5"` / rounded joins as its siblings._ |

## Reference from `SocialLink.Icon`

```csharp:xmldocid
T:Pennington.BlogSite.SocialLink
```

_`SocialLink` is a `record SocialLink(RenderFragment Icon, string Url)`; the `Icon` parameter expects a `RenderFragment` value, so pass the static field directly — `SocialIcons.GithubIcon` — never as a component tag `<SocialIcons.GithubIcon />`._

One-line syntax:

```csharp
new SocialLink(SocialIcons.GithubIcon, "https://github.com/example")
```

## Example

_Minimal excerpt pulled from `BlogSiteHeroProjectsSocialsExample.Stage3.Run`, which populates `BlogSiteOptions.Socials` with all four built-in fragments in a single collection expression._

```csharp:xmldocid,bodyonly
M:BlogSiteHeroProjectsSocialsExample.Stage3.Run(System.String[])
```

_Single sentence of context: the `Socials = [...]` block inside `Run` is the only shape the reader needs to recognize — each element is a `new SocialLink(SocialIcons.<Name>, url)` pair._

## See also

- Related reference: [`BlogSiteOptions`](/reference/options/blogsite-options) (see the `SocialLink` helper record and the `Socials` property)
- Related reference: [Built-in BlogSite routes](/reference/blogsite/routes)
- How-to: [Configure the BlogSite homepage](/how-to/configuration/blogsite-homepage)
- How-to: [Customize DocSite layouts and components](/how-to/extensibility/override-docsite-components) <!-- TODO verify this is the correct Extensibility target for authoring new icon RenderFragments; docs-toc.md routes "custom icon components" to Extensibility but the matching how-to is not explicitly titled for icons -->
