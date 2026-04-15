---
title: "Add a hero, projects, and social links"
description: "Populate the four BlogSite homepage surfaces — hero block, My Work card, social-icon row, and top-nav links — on BlogSiteOptions."
sectionLabel: Getting Started with BlogSite
order: 103030
tags: [blogsite, hero, socials, homepage]
uid: tutorials.blogsite.hero-projects-socials
---

By the end of this tutorial your BlogSite host will display a hero headline on the home page, a "My Work" sidebar card listing three projects, a row of four social-media icons beneath it, and a top-nav bar populated from `MainSiteLinks`. You'll reach for `HeroContent`, `Project`, `SocialLink`, and `HeaderLink` on `BlogSiteOptions`, and wire the four built-in icon `RenderFragment` fields from `SocialIcons` — without writing a single line of Razor.

## Prerequisites

- .NET 11 SDK installed
- Completed [Scaffold a blog with BlogSite](xref:tutorials.blogsite.scaffold)
- Completed [Author your first post with BlogSiteFrontMatter](xref:tutorials.blogsite.first-post)

The finished code for this tutorial lives in [`examples/BlogSiteHeroProjectsSocialsExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogSiteHeroProjectsSocialsExample).

---

## 1. Populate the hero block

The BlogSite home page renders a headline block at the very top, driven entirely by `BlogSiteOptions.HeroContent`. Nothing outside the options call needs to change.

### Step 1.1 — Add `HeroContent` to the options

Open the `AddBlogSite` call you wrote in the previous tutorial and add one property. `HeroContent` is a two-field positional record — `Title` and `Description` — so a single constructor call is all it takes.

```csharp:xmldocid,bodyonly
M:BlogSiteHeroProjectsSocialsExample.Stage1.Run(System.String[])
```

The `HeroContent = new HeroContent(Title: …, Description: …)` assignment is the only addition — no new DI registrations, no new Razor files, no front matter changes. The rest of the options block carries forward unchanged from the scaffold tutorial.

### Checkpoint — The hero renders

- Run `dotnet run` and visit `http://localhost:5000/`
- You should see the hero title "Field notes from a weekend content engine" and the description paragraph stacked above the recent-posts list from the first-post tutorial

---

## 2. Add a "My Work" projects section

`BlogSiteOptions.MyWork` accepts a `Project[]` that the home page renders as a sidebar card titled "My Work". Each entry becomes an anchor wrapping a title-and-description pair.

### Step 2.1 — Build the project array

`Project` is a three-field positional record — `Title`, `Description`, `Url` — so you populate it with a C# collection expression right below `HeroContent`. The `Url` becomes the `<a href>` around each rendered entry, so it can point at a GitHub repo, a product page, or any other URL.

```csharp:xmldocid,bodyonly
M:BlogSiteHeroProjectsSocialsExample.Stage2.Run(System.String[])
```

The `MyWork` property is typed as `IReadOnlyList<Project>` on `BlogSiteOptions`. Its default is an empty list, so the "My Work" card stays invisible in the UI until you populate it here.

### Checkpoint — The sidebar card appears

- Run `dotnet run` and visit `http://localhost:5000/`
- You should see a "My Work" card in the home-page right rail with three linked entries — Pennington, MonorailCSS, Mdazor — each clickable

---

## 3. Wire social links with the built-in icons

Social links are `SocialLink(RenderFragment Icon, string Url)` records. The four built-in icons ship as `static readonly RenderFragment` fields on `Pennington.BlogSite.Components.SocialIcons`, so you reference them directly — no component instantiation needed.

### Step 3.1 — Add a `using` for `SocialIcons` and four `SocialLink`s

You'll add two things: a `using Pennington.BlogSite.Components;` directive at the top of `Program.cs` so `SocialIcons.GithubIcon` resolves, and a `Socials = [...]` block with four entries covering all four built-ins (`GithubIcon`, `BlueskyIcon`, `LinkedInIcon`, `MastodonIcon`). Each field is a `RenderFragment` value — you pass the field itself, not `typeof(...)` and not `<GithubIcon />`.

```csharp:xmldocid,bodyonly
M:BlogSiteHeroProjectsSocialsExample.Stage3.Run(System.String[])
```

Notice that `new SocialLink(SocialIcons.GithubIcon, "https://github.com/example")` passes `SocialIcons.GithubIcon` — the `RenderFragment` delegate itself — as the first positional argument. BlogSite invokes that delegate inside the `<a href>` at render time. For custom SVG icons, see the Extensibility how-tos.

### Checkpoint — The icon row renders under "My Work"

- Run `dotnet run` and visit `http://localhost:5000/`
- You should see a horizontal row of four SVG icons — GitHub, Bluesky, LinkedIn, Mastodon — below the "My Work" card, each linking out to its `Url`

---

## 4. Add header links for top-nav

The same `Stage3.Run` listing you added above includes the final surface: `MainSiteLinks`, a `HeaderLink[]` that BlogSite renders in both the top-nav of `MainLayout.razor` and the footer. Each entry is a `HeaderLink(string Title, string Url)` positional record.

### Step 4.1 — Confirm the three header links resolve

Look at the `MainSiteLinks = [...]` block you already pasted in Step 3.1. It contains three entries — `Home` pointing to `/`, `Archive` to `/archive`, and `Tags` to `/tags`. No additional code is needed here; this step exists so you can verify that the nav URLs line up with the routes BlogSite exposes out of the box.

### Checkpoint — The top-nav and footer populate

- Run `dotnet run` and visit `http://localhost:5000/`
- You should see a "Home / Archive / Tags" link row in the site header and the same three links repeated in the footer nav
- Click **Archive** — the archive page lists your first post from the previous tutorial

---

## Summary

- You populated `HeroContent` and watched it render as the home-page headline block.
- You added a `Project[]` to `MyWork` and saw the "My Work" sidebar card appear with three linked entries.
- You wired four `SocialLink` entries to the built-in `SocialIcons.GithubIcon`, `BlueskyIcon`, `LinkedInIcon`, and `MastodonIcon` `RenderFragment` fields — without writing Razor.
- You populated `MainSiteLinks` with three `HeaderLink` entries and saw them render in both the top-nav and the footer.
- You now know the four homepage surfaces on `BlogSiteOptions` — hero, work, socials, header links — and which record type drives each.
