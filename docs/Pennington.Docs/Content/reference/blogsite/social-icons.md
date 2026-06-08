---
title: "Built-in SocialIcons RenderFragments"
description: "The four static RenderFragment fields on Pennington.BlogSite.Components.SocialIcons and the syntax for plugging them into SocialLink.Icon."
sectionLabel: "BlogSite Built-ins"
order: 2
tags: [blogsite, icons, render-fragment]
uid: reference.blogsite.social-icons
---

`SocialIcons` is a Razor component with no render body whose public API is four `public static readonly RenderFragment` fields, each a self-contained inline SVG for use as the `Icon` property on a `SocialLink`. It lives in namespace `Pennington.BlogSite.Components` and is consumed by `BlogSiteOptions.Socials` via the `SocialLink` record in namespace `Pennington.BlogSite`.

## Declaration

```razor:symbol
src/Pennington.BlogSite/Components/SocialIcons.razor
```

The declaration exposes four `static readonly RenderFragment` fields; there is no class body and no constructor.

## Icons

All four fragments share `viewBox="0 0 24 24"`, `stroke="currentColor"`, and `fill="none"`; color and size are inherited from the surrounding anchor or container. Inner `<path>` elements use `stroke-width="1.5"`, `stroke-linecap="round"`, and `stroke-linejoin="round"` where present.

| Name | `viewBox` | `stroke` | `fill` | Notes |
|---|---|---|---|---|
| `GithubIcon` | `0 0 24 24` | `currentColor` | `none` | Single-path GitHub mark. |
| `LinkedInIcon` | `0 0 24 24` | `currentColor` | `none` | Four-path "in" glyph inside a rounded square. |
| `BlueskyIcon` | `0 0 24 24` | `currentColor` | `none` | Single-path butterfly silhouette using the same stroke-only, `currentColor` convention as the other icons. |
| `MastodonIcon` | `0 0 24 24` | `currentColor` | `none` | Two-path elephant-trunk mark using `stroke-width="1.5"` and rounded joins. |

## `SocialLink.Icon` shape

```csharp:symbol
src/Pennington.BlogSite/BlogSiteOptions.cs > SocialLink
```

`SocialLink` is a `record SocialLink(RenderFragment Icon, string Url)`. The `Icon` property accepts the static `RenderFragment` directly (`SocialIcons.GithubIcon`), not the Razor component tag form. For consumer wiring see <xref:how-to.theming.blogsite-homepage>.

## See also

- Related reference: [`BlogSiteOptions`](xref:reference.api.blog-site-options) (see the `SocialLink` helper record and the `Socials` property)
- Related reference: [Built-in BlogSite routes](xref:reference.blogsite.routes)
- How-to: [Configure the BlogSite homepage](xref:how-to.theming.blogsite-homepage)
- How-to: [Customize DocSite layouts and components](xref:how-to.response-pipeline.override-docsite-components)
