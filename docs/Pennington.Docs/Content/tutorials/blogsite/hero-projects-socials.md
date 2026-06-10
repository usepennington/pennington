---
title: "Add a hero, projects, and social links"
description: "Populate the four BlogSite homepage surfaces — hero block, My Work card, social-icon row, and top-nav links — on BlogSiteOptions."
sectionLabel: Getting Started with BlogSite
order: 3
tags: [blogsite, hero, socials, homepage]
uid: tutorials.blogsite.hero-projects-socials
---

By the end of this tutorial, the BlogSite host displays a hero headline on the home page, a "My Work" sidebar card listing three projects, a row of four social-media icons beneath it, and a top-nav bar populated from `MainSiteLinks` — all driven by `BlogSiteOptions` without a line of Razor.

## Prerequisites

- .NET 10 SDK installed
- Completed [Scaffold a blog with BlogSite](xref:tutorials.blogsite.scaffold)
- Completed [Publish your first post and light up the RSS feed](xref:tutorials.blogsite.first-post)

The finished code for this tutorial lives in [`examples/BlogSiteHeroProjectsSocialsExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogSiteHeroProjectsSocialsExample).

---

## 1. Populate the hero block

The BlogSite home page renders a headline block at the very top, driven entirely by `BlogSiteOptions.HeroContent`. `HeroContent` is a two-field positional record — `Title` and `Description` — so a single constructor call is all it takes.

<Steps>
<Step StepNumber="1">

**Add `HeroContent` to the `AddBlogSite` call**

Open the `AddBlogSite` call from the previous tutorial and add one property. The `HeroContent = new HeroContent(Title: …, Description: …)` assignment is the only addition — no new DI registrations, no new Razor files, no front matter changes. The rest of the options block carries forward unchanged from the scaffold tutorial.

```csharp:symbol,bodyonly
examples/BlogSiteHeroProjectsSocialsExample/Stage1_HeroOnly.cs > Stage1.Run
```

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/`
- The hero title "Field notes from a weekend content engine" and the description paragraph stack above the recent-posts list from the first-post tutorial

</Checkpoint>

---

## 2. Add a "My Work" projects section

`BlogSiteOptions.MyWork` accepts a `Project[]` that the home page renders as a sidebar card titled "My Work". `Project` is a three-field positional record — `Title`, `Description`, `Url`. The `Url` becomes the `<a href>` around each rendered entry, so it can point at a GitHub repo, a product page, or any other URL.

<Steps>
<Step StepNumber="1">

**Populate `MyWork` with a `Project[]` collection expression**

Add the `MyWork` property right below `HeroContent`, populated with a C# collection expression. The property is typed as `Project[]` on `BlogSiteOptions`; its default is an empty list, so the "My Work" card stays invisible in the UI until populated here.

```csharp:symbol,bodyonly
examples/BlogSiteHeroProjectsSocialsExample/Stage2_AddProjects.cs > Stage2.Run
```

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/`
- A "My Work" sidebar card appears with three linked entries — Pennington, MonorailCSS, Mdazor — each clickable

</Checkpoint>

---

## 3. Wire social links and top-nav header links

The final two surfaces land in the same `AddBlogSite` call. `Socials` is a `SocialLink(RenderFragment Icon, string Url)[]`; `MainSiteLinks` is a `HeaderLink(string Title, string Url)[]` that BlogSite renders in both the top-nav of `MainLayout.razor` and the footer. The listing below adds both.

<Steps>
<Step StepNumber="1">

**Reference the built-in social icons**

The four built-in icons ship as `static readonly RenderFragment` fields on [`SocialIcons`](xref:reference.blogsite.social-icons) — pass the field directly, not as a component tag (`SocialIcons.GithubIcon`, not `<SocialIcons.GithubIcon />`). Add a `using Pennington.BlogSite.Components;` directive at the top of `Program.cs` so `SocialIcons.GithubIcon` resolves.

</Step>
<Step StepNumber="2">

**Add the `Socials` and `MainSiteLinks` blocks**

Add a `Socials = [...]` block with four entries covering all four built-ins, then a `MainSiteLinks = [...]` block with three entries — `Home` pointing to `/`, `Archive` to `/archive`, and `Tags` to `/tags`. Those URLs match the routes BlogSite includes, so the top-nav populates with no additional code.

```csharp:symbol,bodyonly
examples/BlogSiteHeroProjectsSocialsExample/Stage3_AddSocialsAndHeader.cs > Stage3.Run
```

</Step>
</Steps>

<Checkpoint>

- Run `dotnet run` and visit `http://localhost:5000/`
- A horizontal row of four SVG icons — GitHub, Bluesky, LinkedIn, Mastodon — sits below the "My Work" sidebar card, each linking out to its `Url`
- A "Home / Archive / Tags" link row appears in the site header, and the same three links repeat in the footer nav
- Click **Archive** — the archive page lists the first post from the previous tutorial

</Checkpoint>

---

## Summary

- `HeroContent` now drives the home-page headline block.
- A `Project[]` on `MyWork` brought the "My Work" sidebar card to life with three linked entries.
- Four `SocialLink` entries wire the built-in `SocialIcons.GithubIcon`, `BlueskyIcon`, `LinkedInIcon`, and `MastodonIcon` `RenderFragment` fields — no Razor required.
- `MainSiteLinks` holds three `HeaderLink` entries, and they render in both the top-nav and the footer.
- All four homepage surfaces on `BlogSiteOptions` (hero, work, socials, header links) populate from one options block.

That completes the BlogSite getting-started arc — you have a scaffolded host, a fully-populated post, and a configured home page. From here, [populate the blog homepage](xref:how-to.theming.blogsite-homepage) is the how-to that revisits these same options when you need them on their own, [generate social card images](xref:how-to.feeds.social-cards) adds OpenGraph cards to every post, and <xref:reference.blogsite.routes> catalogs the full route surface the template serves.
