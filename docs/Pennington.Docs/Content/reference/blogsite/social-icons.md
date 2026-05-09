---
title: "Built-in SocialIcons RenderFragments"
description: "The four static RenderFragment fields on Pennington.BlogSite.Components.SocialIcons and the syntax for plugging them into SocialLink.Icon."
sectionLabel: "BlogSite Built-ins"
order: 408020
tags: [blogsite, icons, render-fragment]
uid: reference.blogsite.social-icons
---

`SocialIcons` is a Razor component with no render body whose only surface is four `public static readonly RenderFragment` fields, each a self-contained inline SVG for use as the `Icon` property on a `SocialLink`. It lives in namespace `Pennington.BlogSite.Components` and is consumed by `BlogSiteOptions.Socials` via the `SocialLink` record in namespace `Pennington.BlogSite`.

## Declaration

```razor:path
src/Pennington.BlogSite/Components/SocialIcons.razor
```

The declaration exposes four `static readonly RenderFragment` fields; there is no class body and no constructor.

## Icons

All four fragments share `viewBox="0 0 24 24"`, `stroke="currentColor"`, and `fill="none"`; color and size are inherited from the surrounding anchor or container. Inner `<path>` elements use `stroke-width="1.5"`, `stroke-linecap="round"`, and `stroke-linejoin="round"` where present.

| Name | `viewBox` | `stroke` | `fill` | Notes |
|---|---|---|---|---|
| `GithubIcon` | `0 0 24 24` | `currentColor` | `none` | Single-path Octocat silhouette. |
| `LinkedInIcon` | `0 0 24 24` | `currentColor` | `none` | Four-path "in" glyph inside a rounded square. |
| `BlueskyIcon` | `0 0 24 24` | `currentColor` | `none` | Single-path butterfly silhouette using the same stroke-only, `currentColor` convention as the other icons. |
| `MastodonIcon` | `0 0 24 24` | `currentColor` | `none` | Two-path elephant-trunk mark using `stroke-width="1.5"` and rounded joins. |

## Reference from `SocialLink.Icon`

```csharp:xmldocid
T:Pennington.BlogSite.SocialLink
```

`SocialLink` is a `record SocialLink(RenderFragment Icon, string Url)`; pass the static field directly — `SocialIcons.GithubIcon` — not as a component tag `<SocialIcons.GithubIcon />`.

One-line syntax:

```csharp
new SocialLink(SocialIcons.GithubIcon, "https://github.com/example")
```

## Example

Excerpt from `BlogSiteHeroProjectsSocialsExample.Stage3.Run`, which populates `BlogSiteOptions.Socials` with all four built-in fragments.

```csharp:xmldocid,bodyonly
M:BlogSiteHeroProjectsSocialsExample.Stage3.Run(System.String[])
```

## See also

- Related reference: [`BlogSiteOptions`](xref:reference.api.blog-site-options) (see the `SocialLink` helper record and the `Socials` property)
- Related reference: [Built-in BlogSite routes](xref:reference.blogsite.routes)
- How-to: [Configure the BlogSite homepage](xref:how-to.feeds.blogsite-homepage)
- How-to: [Customize DocSite layouts and components](xref:how-to.response-pipeline.override-docsite-components)
