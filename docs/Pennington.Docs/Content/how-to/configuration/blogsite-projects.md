---
title: "Showcase projects with \"my work\""
description: "Populating MyWork with Project entries (title, description, URL, color/icon) and how the list renders on the homepage."
section: "configuration"
order: 110
tags: []
uid: how-to.configuration.blogsite-projects
isDraft: true
search: false
llms: false
---

> **In this page.** Populating `MyWork` with `Project` entries (title, description, URL, color/icon) and how the list renders on the homepage.
>
> **Not in this page.** Per-project landing pages — use ordinary markdown content for those.

## When to use this

- Outline bullet: You already have a BlogSite wired with `AddBlogSite` / `UseBlogSite` / `RunBlogSiteAsync` and want the sidebar "My Work" card on the home page to link to your side projects, repos, or products.
- Outline bullet: You are not building per-project detail pages here — for those, author ordinary markdown under `Content/` and link out from the `Project.Url`.

## Assumptions

- Outline bullet: You have a working BlogSite (see `/tutorials/getting-started/first-blog` if not).
- Outline bullet: You can find `Program.cs` and already have a `new BlogSiteOptions { SiteTitle = ..., Description = ... }` factory in place.
- Outline bullet: `BlogSiteOptions.MyWork`, the `Project` record, and the homepage rendering live in `Pennington.BlogSite` and are verified in `src/Pennington.BlogSite/BlogSiteOptions.cs` and `src/Pennington.BlogSite/Components/Pages/Home.razor`.
- Outline bullet: The `Project` record is a three-property positional record: `Project(string Title, string Description, string Url)`. There is no `Color` or `Icon` property on `Project` today — the Covers line is aspirational on that front. Do not fabricate those fields in the sample; call the mismatch out to the reader in a short note.
- Outline bullet: To copy a working setup, see [`examples/AlexBlogExample`](https://github.com/Phil-Scott-Thomas/Pennington/tree/main/examples/AlexBlogExample) (single project, minimal wiring) or [`examples/BlogExample`](https://github.com/Phil-Scott-Thomas/Pennington/tree/main/examples/BlogExample) (multiple projects alongside hero + socials). Do not walk through either end-to-end — this is a recipe, not a tour.

---

## Steps

### 1. Construct `Project` entries

- Outline bullet: `Project` is a positional record with three ordered args: `Title`, `Description`, `Url`. All three are non-nullable `string`.
- Outline bullet: `Title` is the short bold label (rendered as `<dt>`); keep it tight (`"HotPath"`, `"Tempo"`) — long titles wrap awkwardly in the sidebar card.
- Outline bullet: `Description` is the one-line pitch (rendered as `<dd>` below the title); aim for under ~60 characters so it fits the sticky sidebar at `lg:` width.
- Outline bullet: `Url` is the link target the whole `<dt>/<dd>` block wraps — external (`https://github.com/...`) or internal (`/projects/tempo`).

### 2. Assign the array to `BlogSiteOptions.MyWork`

- Outline bullet: `MyWork` is typed `Project[]` and defaults to `[]`; an empty array hides the entire "My Work" card (the homepage guards on `BlogOptions.MyWork.Length > 0`).
- Outline bullet: Order is preserved — entries render top-to-bottom exactly as declared, so put flagship projects first.
- Outline bullet: Use a collection-expression literal inside the `BlogSiteOptions` initializer.

```csharp raw-file="examples/AlexBlogExample/Program.cs"
```

- Outline bullet: Snippet source — `AlexBlogExample/Program.cs` shows the minimal single-project case (`MyWork = [ new Project("Tempo", "...", "https://...") ]`). (Raw-file fence: `Program.cs` is top-level statements with no xmldocid-addressable symbol.)

### 3. (Optional) Add multiple projects

- Outline bullet: Add additional `new Project(...)` items separated by commas inside the `[ ]`.
- Outline bullet: There is no hard cap, but the card is sticky in the homepage sidebar — lists beyond ~6 entries start to dominate the viewport at `lg:` breakpoint.

```csharp raw-file="examples/BlogExample/Program.cs"
```

- Outline bullet: Snippet source — `BlogExample/Program.cs` shows a three-project `MyWork` array (`gum-performance-benchmark`, `mandible-trainer-pro`, plus a third) alongside `HeroContent` and `Socials`.

### 4. (Not supported today) Color / icon per project

- Outline bullet: The `Project` record has **no** `Color` or `Icon` property — despite the toc line, neither field exists on the type.
- Outline bullet: If you need per-project styling today, put the icon or color accent in the linked target page itself, not on the sidebar entry.
- Outline bullet: If/when those fields are added, this page will grow a step; link out to the reference page for the current shape rather than hand-rolling a workaround here.

---

## Verify

- Outline bullet: Run `dotnet run` and visit `/` at a `lg:` (desktop) viewport — the right sidebar shows a bordered card titled "My Work" with each `Title` in bold and `Description` underneath; clicking anywhere on an entry opens `Url`.
- Outline bullet: Narrow the viewport below `lg:` — the card is hidden by design (`hidden lg:block`); this is not a bug.
- Outline bullet: Set `MyWork = []` (or omit the property) — the card disappears entirely on the home page; confirm no empty-card chrome remains.

## Related

- Reference: [BlogSiteOptions](/reference/blogsite-options) — full property-by-property listing including `MyWork`, `Socials`, `MainSiteLinks`, `HeroContent`.
- Background: [BlogSite homepage layout](/explanation/blogsite-homepage) — why `MyWork`, `Socials`, and `AuthorBio` share one sticky sidebar card.
