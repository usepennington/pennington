---
title: "Configure the BlogSite homepage"
description: "Set site metadata, hero content, project list, socials, and header navigation in one BlogSiteOptions block."
section: "configuration"
order: 90
tags: []
uid: how-to.configuration.blogsite-homepage
isDraft: true
search: false
llms: false
---

> **In this page.** The four BlogSite homepage surfaces — `HeroContent`, `MyWork`, `Socials`, and `MainSiteLinks` — set in one pass on `BlogSiteOptions`.
>
> **Not in this page.** The end-to-end build of these surfaces (see the tutorial) or the full options catalog (see Reference).

## When to use this

You already have a BlogSite running and want to finish the homepage — headline, project list, socials, header nav — in one edit without flipping between four pages.

## Assumptions

- You have a working BlogSite (see [_Scaffold a blog with BlogSite_](/tutorials/blogsite/scaffold) if not).
- You are editing the `BlogSiteOptions` initializer passed to `AddBlogSite` in `Program.cs`.
- For the full property schema, use the Reference: [`BlogSiteOptions`](/reference/options/blogsite-options).

To copy a working setup, see [`examples/BlogExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogExample) (every homepage surface wired) or [`examples/AlexBlogExample`](https://github.com/usepennington/pennington/tree/main/examples/AlexBlogExample) (minimal single-project case). Do not walk through either example — this page is a recipe, not a tour.

---

## Steps

### 1. Set `SiteTitle`, `Description`, and `CanonicalBaseUrl`

Name the site and pin the production URL so chrome, metadata, and absolute links all agree. Keep these three fields together at the top of the initializer.

```csharp:path
examples/AlexBlogExample/Program.cs
```

### 2. Add `AuthorName` and `AuthorBio`

Posts, feeds, and the sidebar use these for bylines and the author blurb. Leave them unset to hide those surfaces.

### 3. Fill in `HeroContent`

`HeroContent` is `record HeroContent(string Title, string Description)`. `Title` renders as an `<h1>`; `Description` is injected as raw HTML, so inline `<a>` tags work. Leave the whole property null to hide the hero block.

```csharp:path
examples/AlexBlogExample/Program.cs
```

### 4. Populate `MyWork` with `Project` entries

`Project` is a positional record `Project(string Title, string Description, string Url)`. Order is preserved in the sidebar card, which is hidden below the `lg:` breakpoint and hidden entirely when the array is empty.

```csharp:path
examples/BlogExample/Program.cs
```

### 5. Wire `Socials` with the built-in icons

Add `using Pennington.BlogSite.Components;` so `SocialIcons.GithubIcon`, `LinkedInIcon`, `BlueskyIcon`, and `MastodonIcon` are in scope. `SocialLink` is `record SocialLink(RenderFragment Icon, string Url)`; reference the icon fields directly (no angle-bracket syntax).

```csharp:path
examples/BlogExample/Program.cs
```

### 6. Populate `MainSiteLinks` with `HeaderLink` entries

`HeaderLink` is `record HeaderLink(string Title, string Url)`. Entries surface in the desktop header, the mobile menu, and the footer nav. Mix internal paths and external URLs freely. An empty array hides all three.

```csharp:path
examples/BlogExample/Program.cs
```

---

## Verify

- Run `dotnet run` and visit `/` — expect the hero, project list, and socials strip to render with the values you set.
- Expect each `HeaderLink.Title` to appear in the header (desktop and mobile) and footer nav.
- Expect each `SocialLink` to render its icon inside an `<a href>` wrapping `Url`.

## Related

- Reference: [`BlogSiteOptions`](/reference/options/blogsite-options)
- Reference: [Built-in `SocialIcons` render fragments](/reference/blogsite/social-icons)
- Reference: [Built-in BlogSite routes](/reference/blogsite/routes)
- Tutorial: [Add a hero, projects, and social links](/tutorials/blogsite/hero-projects-socials) — the teach-it-end-to-end version.
