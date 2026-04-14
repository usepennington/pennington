---
title: "Add a hero, projects, and social links"
description: "Populate the four BlogSite homepage surfaces — hero block, My Work card, social-icon row, and top-nav links — on BlogSiteOptions."
sectionLabel: Getting Started with BlogSite
order: 103030
tags: [blogsite, hero, socials, homepage]
uid: tutorials.blogsite.hero-projects-socials
---

> **In this page.** Populate `HeroContent`, a list of `Project` entries for a "My Work" section, `SocialLink` entries (with the built-in `BlueskyIcon`/`GithubIcon`), and `HeaderLink` entries for top-nav items.
>
> **Not in this page.** Custom icon components — covered in the Extensibility how-tos. When you need a compact lookup version of the same options surface, see the [BlogSite homepage configuration how-to](xref:how-to.configuration.blogsite-homepage).

## What you'll do

_**Artifact** (one sentence): describe the concrete output — the `BlogSiteFirstPostExample` host, now with a hero headline, a "My Work" sidebar card with three projects, a row of four social icons under it, and a top-nav populated from `MainSiteLinks`._

_**Skill** (one sentence): describe what the reader walks away able to do — reach for `HeroContent`, `Project`, `SocialLink`, and `HeaderLink` on `BlogSiteOptions`, and wire the four built-in icon `RenderFragment`s from `SocialIcons` without writing any Razor._

## Prerequisites

_Keep this list to tools and prior tutorials only. The reader arrives with the BlogSite host from the previous two tutorials in section 1.3 — scaffold sets up `AddBlogSite`, first-post lands one markdown post in `Content/Blog/` so the recent-posts card isn't empty._

- .NET 11 SDK installed
- Completed [Scaffold a blog with BlogSite](xref:tutorials.blogsite.scaffold)
- Completed [Author your first post with BlogSiteFrontMatter](xref:tutorials.blogsite.first-post)

The finished code for this tutorial lives in [`examples/BlogSiteHeroProjectsSocialsExample`](https://github.com/usepennington/pennington/tree/main/examples/BlogSiteHeroProjectsSocialsExample).

---

## 1. Populate the hero block

_One sentence: orient the reader — the BlogSite home page (`/`) renders a headline block at the top driven entirely by `BlogSiteOptions.HeroContent`, and nothing else needs to change to light it up._

### Step 1.1 — Add `HeroContent` to the options

_One sentence of setup: the reader opens the `AddBlogSite` call from the first-post tutorial and adds one property. `HeroContent` is a two-field positional record — `Title` and `Description` — and the description is rendered as `MarkupString` inside `Home.razor`, so light HTML works but plain prose keeps this tutorial uncluttered._

```csharp:xmldocid,bodyonly
M:BlogSiteHeroProjectsSocialsExample.Stage1.Run(System.String[])
```

_Call out one non-obvious line: the `HeroContent = new HeroContent(Title: …, Description: …)` line is all that's needed — no new DI calls, no new Razor, no new front matter. The rest of the options block carries forward from the scaffold tutorial._

### Checkpoint — The hero renders

- Run `dotnet run` and visit `http://localhost:5000/`
- You should see the hero title "Field notes from a weekend content engine" and the description paragraph stacked above the recent-posts list from the first-post tutorial

---

## 2. Add a "My Work" projects section

_One sentence: `BlogSiteOptions.MyWork` takes a `Project[]` that the home page renders as a sidebar card titled "My Work" — each entry becomes an anchor wrapping a title/description pair, and the reader will add three._

### Step 2.1 — Build the project array

_One sentence of setup: `Project` is a three-field positional record — `Title`, `Description`, `Url` — and the reader adds a collection expression with three entries right below `HeroContent`. The `Url` becomes the `<a href>` that wraps the rendered `<dt>`/`<dd>` pair, so it can point at anything from a GitHub repo to a product page._

```csharp:xmldocid,bodyonly
M:BlogSiteHeroProjectsSocialsExample.Stage2.Run(System.String[])
```

_Explain one non-obvious line: `MyWork = [ new Project(...), new Project(...), new Project(...) ]` uses C# collection expressions; the property is typed as `IReadOnlyList<Project>` on `BlogSiteOptions`, and the empty default stays invisible until populated._

### Checkpoint — The sidebar card appears

- Run `dotnet run` and visit `http://localhost:5000/`
- You should see a "My Work" card in the home-page right rail with three linked entries — Pennington, MonorailCSS, Mdazor — each clickable

---

## 3. Wire social links with the built-in icons

_One sentence: social links are `SocialLink(RenderFragment Icon, string Url)` records, and the four built-in icons ship as `static readonly RenderFragment` fields on `Pennington.BlogSite.Components.SocialIcons` — the reader references them directly, no component instantiation or generic type argument required._

### Step 3.1 — Add a `using` for `SocialIcons` and four `SocialLink`s

_One sentence of setup: introduce the two pieces the reader is about to add — the `using Pennington.BlogSite.Components;` directive at the top of `Program.cs` so `SocialIcons.GithubIcon` resolves, and a `Socials = [...]` block with four entries covering all four built-ins (`GithubIcon`, `BlueskyIcon`, `LinkedInIcon`, `MastodonIcon`). Mention that each field is a `RenderFragment`, not a component type — you pass the field value itself, not `typeof(...)` and not `<GithubIcon />`._

```csharp:xmldocid,bodyonly
M:BlogSiteHeroProjectsSocialsExample.Stage3.Run(System.String[])
```

_Explain one non-obvious line: `new SocialLink(SocialIcons.GithubIcon, "https://github.com/example")` — `SocialIcons.GithubIcon` is the `RenderFragment` itself, so the positional constructor receives a delegate that BlogSite invokes inside the `<a href>` at render time. Custom icons are out of scope for this tutorial — link to the Extensibility how-tos for that._

### Checkpoint — The icon row renders under "My Work"

- Run `dotnet run` and visit `http://localhost:5000/`
- You should see a horizontal row of four SVG icons — GitHub, Bluesky, LinkedIn, Mastodon — below the "My Work" card, each linking out to its `Url`

---

## 4. Add header links for top-nav

_One sentence: the same `Stage3.Run` listing above also adds the final surface — `MainSiteLinks`, a `HeaderLink[]` rendered in both the top-nav of `MainLayout.razor` and the footer nav. Each entry is a `HeaderLink(string Title, string Url)` positional record._

### Step 4.1 — Confirm the three header links resolve

_One sentence of setup: direct the reader back to the same `Stage3` snippet — they already pasted the `MainSiteLinks = [...]` block with three entries (`Home` → `/`, `Archive` → `/archive`, `Tags` → `/tags`). No additional code goes in this step — this unit exists so the reader verifies the top-nav is populated and that internal link URLs line up with the routes BlogSite already exposes._

_No additional code fence: the `Stage3` body in Step 3.1 is the final state for this tutorial, and this unit just reads the `MainSiteLinks` block within it._

### Checkpoint — The top-nav and footer populate

- Run `dotnet run` and visit `http://localhost:5000/`
- You should see a "Home / Archive / Tags" link row in the site header and the same three links repeated in the footer nav
- Click **Archive** — the archive page lists your first post from the previous tutorial

---

## Summary

_Three to five bullets. Each bullet names a capability the reader now has, not a topic you covered._

- You populated `HeroContent` and watched it render as the home-page headline block.
- You added a `Project[]` to `MyWork` and saw the "My Work" sidebar card appear with three linked entries.
- You wired four `SocialLink` entries to the built-in `SocialIcons.GithubIcon`, `BlueskyIcon`, `LinkedInIcon`, and `MastodonIcon` `RenderFragment` fields — without writing Razor.
- You populated `MainSiteLinks` with three `HeaderLink` entries and saw them render in both the top-nav and the footer.
- You now know the four homepage surfaces on `BlogSiteOptions` — hero, work, socials, header links — and which record type drives each.
