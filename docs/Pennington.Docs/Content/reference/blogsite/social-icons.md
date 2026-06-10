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

```razor:symbol,signatures
src/Pennington.BlogSite/Components/SocialIcons.razor
```

There is no render body, class body, or constructor — just the four fields.

## Icons

All four fragments share `viewBox="0 0 24 24"`, `stroke="currentColor"`, and `fill="none"`; color and size are inherited from the surrounding anchor or container. The available fields are:

- `GithubIcon`
- `LinkedInIcon`
- `BlueskyIcon`
- `MastodonIcon`

## `SocialLink.Icon` shape

```csharp:symbol
src/Pennington.BlogSite/BlogSiteOptions.cs > SocialLink
```

The `Icon` property accepts the static `RenderFragment` directly (`SocialIcons.GithubIcon`), not the Razor component tag form. For consumer wiring see <xref:how-to.theming.blogsite-homepage>.

## See also

- Related reference: [`BlogSiteOptions`](xref:reference.api.blog-site-options) (see the `SocialLink` helper record and the `Socials` property)
- Related reference: [Built-in BlogSite routes](xref:reference.blogsite.routes)
- How-to: [Configure the BlogSite homepage](xref:how-to.theming.blogsite-homepage)
- How-to: [Customize DocSite layouts and components](xref:how-to.response-pipeline.override-docsite-components)
