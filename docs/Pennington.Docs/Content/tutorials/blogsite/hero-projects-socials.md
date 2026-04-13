---
title: "Add a hero, projects, and social links"
description: "Populate HeroContent, a list of Project entries, SocialLink entries with the built-in BlueskyIcon/GithubIcon, and HeaderLink entries for top-nav items."
section: "blogsite"
order: 30
tags: []
uid: tutorials.blogsite.hero-projects-socials
isDraft: true
search: false
llms: false
---

> **In this page.** Populating `HeroContent`, a list of `Project` entries for a "my work" section, `SocialLink` entries (with the built-in `BlueskyIcon`/`GithubIcon`), and `HeaderLink` entries for top-nav items.
>
> **Not in this page.** Custom icon components or deep homepage layout overrides — covered in the Extensibility how-tos.

## What you'll do

- Artifact: a BlogSite homepage that renders a personalized hero block above the post list, a "my work" grid of project cards, a row of social icons, and top-nav header links.
- Skill: you will know how to populate the four homepage-surface fields of `BlogSiteOptions` (`HeroContent`, `MyWork`, `Socials`, `MainSiteLinks`) and wire the built-in `SocialIcons` render fragments into `SocialLink` entries.

## Prerequisites

- .NET 11 SDK installed.
- Completed [Author your first post with BlogFrontMatter](/tutorials/blogsite/first-post) (or have a BlogSite project with at least one published post).
- A decision already made about which socials and links you want to list (URLs, project names) — the tutorial assumes you bring the content; it teaches the wiring.

The finished code for this tutorial lives in [`examples/AlexBlogExample`](https://github.com/PhilScott/Pennington/tree/main/examples/AlexBlogExample) (minimal, two socials, one project) with a fuller reference configuration in [`examples/BlogExample`](https://github.com/PhilScott/Pennington/tree/main/examples/BlogExample).

---

## 1. Add a hero block

- Bullets:
  - Introduce `HeroContent` as a two-field record: `Title` and `Description`, defined in `Pennington.BlogSite.BlogSiteOptions.cs`.
  - Explain that `Description` accepts inline HTML (see the `<strong>` usage in `BlogExample/Program.cs`), so readers can emphasize a name or role without reaching for a custom component.
  - Note that if `HeroContent` is left `null` (its default), the homepage falls back to just the recent-posts list — the hero is opt-in.

### Step 1.1 — Set `HeroContent` on `BlogSiteOptions`

- Bullets:
  - Inside the existing `AddBlogSite(() => new BlogSiteOptions { ... })` call, add a `HeroContent = new HeroContent("<headline>", "<intro>")` line.
  - Place it alongside `SiteTitle` / `Description` / `AuthorName` for readability.
  - Save — `dotnet watch` (or the dev-loop Pennington live reload) will refresh the browser.

```csharp raw-file="examples/AlexBlogExample/Program.cs"
```

- One-line bullet: snippet shows the minimal `AlexBlogExample` wiring — `HeroContent` appears right after `AuthorBio` with a plain headline + single-paragraph intro.

### Checkpoint — Hero renders above recent posts

- Run `dotnet run` and visit `http://localhost:5000/`.
- You should see the headline and description above the list of blog posts, styled with the site's display font.

---

## 2. Showcase your projects with `MyWork`

- Bullets:
  - Introduce `Project` as a three-field record: `Title`, `Description`, `Url`.
  - Explain that `MyWork` is a `Project[]` on `BlogSiteOptions` (default empty). Each entry renders as a card on the homepage.
  - Remind the learner that `Url` may point anywhere — a GitHub repo, a product page, a case study on another site.
  - Suggest keeping `Description` to one sentence so the grid stays visually even.

### Step 2.1 — Populate `MyWork` with `Project` entries

- Bullets:
  - Add a `MyWork = [ new Project(...) , new Project(...) ]` collection-expression to `BlogSiteOptions`.
  - Encourage listing projects the reader actually wants visible — don't pad.
  - Mention that ordering in the array is the rendered order.

```csharp raw-file="examples/BlogExample/Program.cs"
```

- One-line bullet: snippet from `BlogExample/Program.cs` (Calvin's Chewing Chronicles) shows five `Project` entries populating `MyWork` — a denser layout than the `AlexBlogExample` one-project form.

### Checkpoint — Projects appear on the homepage

- Refresh `http://localhost:5000/` and confirm a "my work" section renders each `Project` as a titled card with its description and a link to `Url`.

---

## 3. Add social links with the built-in icons

- Bullets:
  - Introduce `SocialLink(RenderFragment Icon, string Url)` — it takes a Blazor `RenderFragment` for the icon, paired with a destination URL.
  - Introduce the `Pennington.BlogSite.Components.SocialIcons` static class, which exposes ready-to-use render fragments: `GithubIcon`, `BlueskyIcon`, `LinkedInIcon`, `MastodonIcon`. These are stroke-styled SVGs that pick up the site's text color via `currentColor`.
  - Emphasize that because `Icon` is a `RenderFragment`, readers can supply their own Razor fragment later (covered in the Extensibility how-tos).

### Step 3.1 — Wire `Socials` to built-in icons

- Bullets:
  - Add a `Socials = [ new SocialLink(SocialIcons.GithubIcon, "https://github.com/you"), ... ]` array on `BlogSiteOptions`.
  - Pick the icons matching platforms the reader actually uses.
  - Add a `using Pennington.BlogSite.Components;` at the top of `Program.cs` so `SocialIcons` resolves without qualification.

```csharp raw-file="examples/AlexBlogExample/Program.cs"
```

- One-line bullet: same file as step 1.1; learners will see the `Socials` array with `SocialIcons.GithubIcon` and `SocialIcons.MastodonIcon` wired in directly below `MyWork`.

### Checkpoint — Icons render in the chrome

- Refresh the homepage and confirm that each `SocialLink` renders as an icon button linking to its `Url`; hover shows the underlying link.

---

## 4. Add top-nav entries with `MainSiteLinks`

- Bullets:
  - Introduce `HeaderLink(string Title, string Url)` — a plain text link shown in the site header.
  - Introduce `MainSiteLinks` as a `HeaderLink[]` on `BlogSiteOptions` (default empty). Populating it turns on the top-nav row.
  - Note that URLs can be relative (`/about`) or absolute (`https://github.com/sponsors/you`) — the blog template doesn't treat them differently.

### Step 4.1 — Populate `MainSiteLinks`

- Bullets:
  - Add `MainSiteLinks = [ new HeaderLink("About", "/about"), new HeaderLink("Sponsor Me", "https://...") ]` to the options.
  - Keep the list short (2–4 entries) so the header doesn't wrap on mobile.
  - If an internal URL points at a page that doesn't exist yet, Pennington's link verification will warn at build time — this is expected; create the page in a later tutorial or ignore for now.

```csharp raw-file="examples/BlogExample/Program.cs"
```

- One-line bullet: the `BlogExample/Program.cs` snippet shows `MainSiteLinks` with two entries ("About" and "Sponsor Me") plus the full `Socials` array covering all four built-in icons, giving learners a complete reference of the homepage surface populated at once.

### Checkpoint — Header row appears

- Refresh and confirm the top-nav row is visible across the site's pages (header is shared chrome, not homepage-only).

---

## Summary

- You have a BlogSite homepage with a personalized `HeroContent` block above the post list.
- You added a `MyWork` grid driven by `Project` records and can rearrange or extend it by editing the array.
- You wired `Socials` using the built-in `SocialIcons.GithubIcon`, `SocialIcons.BlueskyIcon`, `SocialIcons.LinkedInIcon`, and `SocialIcons.MastodonIcon` render fragments.
- You populated `MainSiteLinks` so every page shares a top-nav row of `HeaderLink` entries.
- You know where each of the four homepage-surface fields lives on `BlogSiteOptions` and can edit them without touching Razor.

> Navigation to the next tutorial is generated automatically from `order` — do not write a "what's next" section.
