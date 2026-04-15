---
title: "Configure the BlogSite homepage"
description: "Populate the four BlogSite homepage surfaces — HeroContent, MyWork, Socials, and MainSiteLinks — on BlogSiteOptions in one pass."
uid: how-to.configuration.blogsite-homepage
order: 202090
sectionLabel: Configuration
tags: [blogsite, homepage, socials, hero]
---

When you have a running BlogSite and want to wire all four homepage surfaces — the hero block, "My Work" card, social-icon row, and top-nav links — in one pass, this is the recipe. For the hand-held walkthrough, see [Add a hero, projects, and social links](xref:tutorials.blogsite.hero-projects-socials).

## Assumptions


- You have a running BlogSite built with `AddBlogSite` / `UseBlogSite` (see [Scaffold a blog with BlogSite](xref:tutorials.blogsite.scaffold) if not).
- You have at least one post under `BlogContentPath` so the recent-posts slot is not empty (see [Author your first post with BlogSiteFrontMatter](xref:tutorials.blogsite.first-post)).
- You are editing the single `AddBlogSite(() => new BlogSiteOptions { ... })` call — the four surfaces are init-only properties on that same record literal.

To copy a working setup, see [`examples/BlogSiteHeroProjectsSocialsExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogSiteHeroProjectsSocialsExample). Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps


### 1. Set `HeroContent` for the headline block

`HeroContent` is a two-field positional record (`Title`, `Description`) rendered at the top of `/`. `Description` is emitted as a `MarkupString` in `Home.razor`, so light HTML is permitted — plain prose works for most sites.

```csharp:xmldocid
T:Pennington.BlogSite.HeroContent
```

```csharp:xmldocid,bodyonly
M:BlogSiteHeroProjectsSocialsExample.Stage1.Run(System.String[])
```

### 2. Fill `MyWork` with `Project` entries

`MyWork` takes a `Project[]`, where each `Project(Title, Description, Url)` renders as a linked entry in the "My Work" sidebar card. The array is rendered verbatim, so ordering entries in the initializer controls their display order.

```csharp:xmldocid
T:Pennington.BlogSite.Project
```

```csharp:xmldocid,bodyonly
M:BlogSiteHeroProjectsSocialsExample.Stage2.Run(System.String[])
```

### 3. Wire `Socials` with the built-in icon fragments

`Socials` takes a `SocialLink[]`, where `SocialLink(Icon, Url)` pairs a `RenderFragment` with an `<a href>` target. The four built-in fragments — `GithubIcon`, `BlueskyIcon`, `LinkedInIcon`, `MastodonIcon` — are `static readonly` fields on `Pennington.BlogSite.Components.SocialIcons` and are passed directly without any wrapper type or component registration.

```csharp:xmldocid
T:Pennington.BlogSite.SocialLink
```

```razor:path
src/Pennington.BlogSite/Components/SocialIcons.razor
```

### 4. Populate `MainSiteLinks` for the top nav

`MainSiteLinks` takes a `HeaderLink[]`, where each `HeaderLink(Title, Url)` appears in both the site header and footer via `MainLayout.razor`. Use relative URLs (`/`, `/archive`, `/tags`) so `BaseUrlHtmlRewriter` can prefix them correctly on sub-path deployments.

```csharp:xmldocid
T:Pennington.BlogSite.HeaderLink
```

```csharp:xmldocid,bodyonly
M:BlogSiteHeroProjectsSocialsExample.Stage3.Run(System.String[])
```

---

## Verify

- Run `dotnet run` and open `/` — the hero title and description appear at the top, and the "My Work" card lists each `Project` entry with a working link.
- The social-icon row under the card renders one icon per `SocialLink`, each linking to its `Url`; the top-nav and footer list every `HeaderLink`.
- Run `dotnet run -- build` — the generated `index.html` contains every hero/project/socials/nav string; no 500s in the build report.

## Related

- Tutorial: [_Add a hero, projects, and social links_](xref:tutorials.blogsite.hero-projects-socials) — the hand-held walkthrough of the same four surfaces.
- Reference: [_`BlogSiteOptions`_](xref:reference.options.blogsite-options) — the full property catalog (site metadata, content paths, feeds, fonts).
- Background: [_When is DocSite the right starting point?_](xref:explanation.core.docsite-positioning) — for the DocSite-vs-BlogSite trade-off behind these templates.
