---
title: "Built-in SocialIcons RenderFragments"
description: "The four static RenderFragment fields shipped on Pennington.BlogSite.Components.SocialIcons — GithubIcon, LinkedInIcon, BlueskyIcon, MastodonIcon — and how to wire them into BlogSiteOptions.Socials."
section: "blogsite"
order: 20
tags: []
uid: reference.blogsite.social-icons
isDraft: true
search: false
llms: false
---

> **In this page.** The four static `RenderFragment` fields on `Pennington.BlogSite.Components.SocialIcons` — `GithubIcon`, `LinkedInIcon`, `BlueskyIcon`, `MastodonIcon` — their SVG viewBoxes, the `currentColor` / `stroke` conventions they follow, and how to reference them directly (not as components) when populating `SocialLink.Icon`.
>
> **Not in this page.** Authoring new icon fragments — that is a Razor-component authoring topic (see Extensibility).

## Summary

- Four pre-built SVG icon fragments shipped inside the `BlogSite` template so a default blog scaffold has a working social-icon strip with zero additional work.
- Namespace `Pennington.BlogSite.Components`; source file `src/Pennington.BlogSite/Components/SocialIcons.razor`.
- All four fields are `public static readonly RenderFragment` — reference them directly (e.g., `SocialIcons.GithubIcon`), do not write `<SocialIcons.GithubIcon />`.

## Declaration

- File: `src/Pennington.BlogSite/Components/SocialIcons.razor`.
- Each field is a `RenderFragment` built with the Razor inline-SVG syntax `@<svg …></svg>`.
- All four SVGs share:
  - `xmlns="http://www.w3.org/2000/svg"`
  - `viewBox="0 0 24 24"`
  - `stroke="currentColor"` so the icon inherits the surrounding text color.
  - `fill="none"` and `stroke-width="1.5"` as the default pen settings.
- Consuming code colors the icon by placing the fragment inside an element with a `text-*` utility class.

## Icon table

| Field | Purpose | Notes |
|---|---|---|
| `GithubIcon` | GitHub profile link. | Single-path Octicon-style silhouette. |
| `LinkedInIcon` | LinkedIn profile link. | Three-path rectangle-cornered "in" logo. |
| `BlueskyIcon` | Bluesky profile link. | Butterfly-shaped mark. |
| `MastodonIcon` | Mastodon profile link. | Two-path glyph. |

All four are emitted verbatim inside the `<a>` produced by the Home-page layout when you add a `SocialLink` to `BlogSiteOptions.Socials`.

## Usage contract

- `SocialLink` is declared as `public record SocialLink(RenderFragment Icon, string Url)` (source: `src/Pennington.BlogSite/BlogSiteOptions.cs`).
- `SocialIcons.<IconName>` is a `RenderFragment`, so you pass it directly as the `Icon` positional argument — no `@` prefix, no angle brackets.
- Adding a `using Pennington.BlogSite.Components;` at the top of `Program.cs` makes the four fields available unqualified.

```csharp
using Pennington.BlogSite.Components;

// …
new BlogSiteOptions
{
    // …
    Socials = [
        new SocialLink(SocialIcons.GithubIcon,   "https://github.com/your-handle"),
        new SocialLink(SocialIcons.LinkedInIcon, "https://www.linkedin.com/in/your-handle"),
        new SocialLink(SocialIcons.BlueskyIcon,  "https://bsky.app/profile/your-handle"),
        new SocialLink(SocialIcons.MastodonIcon, "https://mastodon.social/@your-handle"),
    ],
};
```

## Styling notes

- Because every icon uses `stroke="currentColor"`, you control the icon color by styling its containing element — the default `Home.razor` layout wraps each `<a>` in a text-color utility class.
- The SVGs carry no `width` / `height` attributes; size them with `w-*` / `h-*` utilities on the container (default layout uses `size-6`).
- The `viewBox` is uniform across the four icons, so sizing and alignment remain consistent when mixing them in the social strip.

## Extending

- To add a custom icon in the same style, declare a `public static readonly RenderFragment YourIcon = @<svg …></svg>;` in any static Razor-code block in your own project, then pass it to `new SocialLink(YourIcon, "https://…")`.
- Custom icons should follow the same conventions — `viewBox="0 0 24 24"`, `stroke="currentColor"`, `fill="none"` — so they render at the same size/color as the built-ins.

## See also

- Related reference: [`BlogSiteOptions`](/reference/options/blogsite-options) — the `Socials` property and the `SocialLink` record.
- Related reference: [Built-in BlogSite routes](/reference/blogsite/routes).
- How-to: [Add social links and header navigation](/how-to/configuration/blogsite-socials).
