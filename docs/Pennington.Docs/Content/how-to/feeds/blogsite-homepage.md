---
title: "Populate the blog homepage surfaces"
description: "Populate the four BlogSite homepage surfaces — HeroContent, MyWork, Socials, and MainSiteLinks — on BlogSiteOptions in one pass."
uid: how-to.feeds.blogsite-homepage
order: 207040
sectionLabel: "Theming"
tags: [blogsite, homepage, socials, hero]
---

When a BlogSite homepage needs its hero block, "My Work" card, social-icon row, and top-nav links populated in one pass, the four init-only properties on `BlogSiteOptions` cover it. For the hand-held walkthrough, see [Add a hero, projects, and social links](xref:tutorials.blogsite.hero-projects-socials).

## Before you begin
- A running BlogSite built with `AddBlogSite` / `UseBlogSite` (see [Scaffold a blog with BlogSite](xref:tutorials.blogsite.scaffold) if not).
- At least one post under `BlogContentPath` so the recent-posts slot is not empty (see [Author your first post with BlogSiteFrontMatter](xref:tutorials.blogsite.first-post)).
- A single `AddBlogSite(() => new BlogSiteOptions { ... })` call to edit — the four surfaces are init-only properties on that same record literal.

For a working setup, see [`examples/BlogSiteHeroProjectsSocialsExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogSiteHeroProjectsSocialsExample).

---

## Options

### Set `HeroContent` for the headline block

`HeroContent` is a two-field positional record (`Title`, `Description`) rendered at the top of `/`. `Description` is emitted as a `MarkupString` in `Home.razor`, so light HTML is permitted; plain prose works for most sites.

```csharp:symbol
src/Pennington.BlogSite/BlogSiteOptions.cs > HeroContent
```

```csharp:symbol,bodyonly
examples/BlogSiteHeroProjectsSocialsExample/Stage1_HeroOnly.cs > Stage1.Run
```

### Fill `MyWork` with `Project` entries

`MyWork` takes a `Project[]`, where each `Project(Title, Description, Url)` renders as a linked entry in the "My Work" sidebar card. The array is rendered verbatim, so ordering entries in the initializer controls their display order.

```csharp:symbol
src/Pennington.BlogSite/BlogSiteOptions.cs > Project
```

```csharp:symbol,bodyonly
examples/BlogSiteHeroProjectsSocialsExample/Stage2_AddProjects.cs > Stage2.Run
```

### Wire `Socials` with the built-in icon fragments

`Socials` takes a `SocialLink[]`, where `SocialLink(Icon, Url)` pairs a `RenderFragment` with an `<a href>` target. The four built-in fragments — `GithubIcon`, `BlueskyIcon`, `LinkedInIcon`, `MastodonIcon` — are `static readonly` fields on `Pennington.BlogSite.Components.SocialIcons` and are passed directly without any wrapper type or component registration.

```csharp:symbol
src/Pennington.BlogSite/BlogSiteOptions.cs > SocialLink
```

```razor:symbol
src/Pennington.BlogSite/Components/SocialIcons.razor
```

### Populate `MainSiteLinks` for the top nav

`MainSiteLinks` takes a `HeaderLink[]`, where each `HeaderLink(Title, Url)` appears in both the site header and footer via `MainLayout.razor`. Use relative URLs (`/`, `/archive`, `/tags`) so `BaseUrlHtmlRewriter` can prefix them correctly on sub-path deployments.

```csharp:symbol
src/Pennington.BlogSite/BlogSiteOptions.cs > HeaderLink
```

```csharp:symbol,bodyonly
examples/BlogSiteHeroProjectsSocialsExample/Stage3_AddSocialsAndHeader.cs > Stage3.Run
```

---

## Result

The homepage at `/` renders the hero block above the post list, the "My Work" card and social-icon row in the right rail, and every `HeaderLink` in both the top nav and the footer. The four surfaces are independent; populating any one renders that surface and leaves the rest at their template defaults.

## Verify

- Run `dotnet run` and open `/`. The hero title and description appear at the top, and the "My Work" card lists each `Project` entry with a working link.
- The social-icon row under the card renders one icon per `SocialLink`, each linking to its `Url`. The top-nav and footer list every `HeaderLink`.
- Run `dotnet run -- build`. The generated `index.html` contains every hero/project/socials/nav string, and the build report shows no 500s.

## Related

- Tutorial: [Add a hero, projects, and social links](xref:tutorials.blogsite.hero-projects-socials) — the hand-held walkthrough of the same four surfaces.
- Reference: [`BlogSiteOptions`](xref:reference.api.blog-site-options) — the full property catalog (site metadata, content paths, feeds, fonts).
- Background: [What the DocSite and BlogSite templates wire for you](xref:explanation.positioning.docsite-positioning) — what each template assembles and where the wiring stops.
