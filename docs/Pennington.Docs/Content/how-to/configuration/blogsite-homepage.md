---
title: "Configure the BlogSite homepage"
description: "Populate the four BlogSite homepage surfaces — HeroContent, MyWork, Socials, and MainSiteLinks — on BlogSiteOptions in one pass."
uid: how-to.configuration.blogsite-homepage
order: 90
sectionLabel: Configuration
tags: [blogsite, homepage, socials, hero]
---

> **In this page.** The four BlogSite homepage surfaces as a single recipe: `HeroContent`, `MyWork` (`Project` entries), `Socials` (`SocialLink` + the built-in icon fragments on `SocialIcons`), and `MainSiteLinks` (`HeaderLink` entries for the top nav).
>
> **Not in this page.** The full `BlogSiteOptions` catalog — see [_`BlogSiteOptions`_](xref:reference.options.blogsite-options). Writing custom icon `RenderFragment`s — see the Extensibility how-tos.

## When to use this

_Two sentences. Reader has a running BlogSite (post scaffold + first-post tutorials) and wants to wire all four homepage surfaces in one pass — the hero block, "My Work" card, social-icon row, and top-nav links. Point anyone who wants the hand-held walkthrough back to the tutorial [Add a hero, projects, and social links](xref:tutorials.blogsite.hero-projects-socials); this is the compact lookup form._

## Assumptions

_Three bullets. Keep prerequisites tight — this is a configuration recipe, not an onboarding ramp. Do not re-teach `AddBlogSite` wiring or markdown authoring; link out instead._

- You have a running BlogSite built with `AddBlogSite` / `UseBlogSite` (see [Scaffold a blog with BlogSite](xref:tutorials.blogsite.scaffold) if not).
- You have at least one post under `BlogContentPath` so the recent-posts slot is not empty (see [Author your first post with BlogSiteFrontMatter](xref:tutorials.blogsite.first-post)).
- You are editing the single `AddBlogSite(() => new BlogSiteOptions { ... })` call — the four surfaces are init-only properties on that same record literal.

To copy a working setup, see [`examples/BlogSiteHeroProjectsSocialsExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogSiteHeroProjectsSocialsExample). Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

_Four verb-first steps, one per homepage surface. Each step names the `BlogSiteOptions` property, the record shape it takes, and drops an `xmldocid` fence that shows the final state of that property populated._

### 1. Set `HeroContent` for the headline block

_Two sentences. `HeroContent` is a two-field positional record (`Title`, `Description`) rendered at the top of `/`. The `Description` is emitted as `MarkupString` in `Home.razor`, so light HTML is permitted — plain prose is fine for most sites._

```csharp:xmldocid
T:Pennington.BlogSite.HeroContent
```

```csharp:xmldocid,bodyonly
M:BlogSiteHeroProjectsSocialsExample.Stage1.Run(System.String[])
```

### 2. Fill `MyWork` with `Project` entries

_Two sentences. `MyWork` takes a `Project[]`, where each `Project(Title, Description, Url)` renders as a linked entry in the "My Work" sidebar card on the home page. Order matters — the array is rendered verbatim, top to bottom._

```csharp:xmldocid
T:Pennington.BlogSite.Project
```

```csharp:xmldocid,bodyonly
M:BlogSiteHeroProjectsSocialsExample.Stage2.Run(System.String[])
```

### 3. Wire `Socials` with the built-in icon fragments

_Two sentences. `Socials` takes a `SocialLink[]`, where `SocialLink(Icon, Url)` pairs a `RenderFragment` with an `<a href>` target. The four built-in fragments live as `static readonly` fields on `Pennington.BlogSite.Components.SocialIcons` — `GithubIcon`, `BlueskyIcon`, `LinkedInIcon`, `MastodonIcon` — and are passed directly (no wrapper type, no component registration)._

```csharp:xmldocid
T:Pennington.BlogSite.SocialLink
T:Pennington.BlogSite.Components.SocialIcons
```

### 4. Populate `MainSiteLinks` for the top nav

_Two sentences. `MainSiteLinks` takes a `HeaderLink[]`, where `HeaderLink(Title, Url)` is rendered in both the site header and footer by `MainLayout.razor`. Use relative URLs (`/`, `/archive`, `/tags`) so `BaseUrlHtmlRewriter` can prefix them on sub-path deployments._

```csharp:xmldocid
T:Pennington.BlogSite.HeaderLink
```

```csharp:xmldocid,bodyonly
M:BlogSiteHeroProjectsSocialsExample.Stage3.Run(System.String[])
```

---

## Verify

_Terse. Three bullets, one per surface cluster so each wiring mistake is individually diagnosable._

- Run `dotnet run` and open `/` — the hero title and description appear at the top, and the "My Work" card lists each `Project` entry with a working link.
- The social-icon row under the card renders one icon per `SocialLink`, each linking to its `Url`; the top-nav and footer list every `HeaderLink`.
- Run `dotnet run -- build` — the generated `index.html` contains every hero/project/socials/nav string; no 500s in the build report.

## Related

- Tutorial: [_Add a hero, projects, and social links_](xref:tutorials.blogsite.hero-projects-socials) — the hand-held walkthrough of the same four surfaces.
- Reference: [_`BlogSiteOptions`_](xref:reference.options.blogsite-options) — the full property catalog (site metadata, content paths, feeds, fonts).
- Background: [_When is DocSite the right starting point?_](xref:explanation.core.docsite-positioning) — for the DocSite-vs-BlogSite trade-off behind these templates.
