---
title: "Customize the BlogSite hero"
description: "Fill out HeroContent so the BlogSite homepage shows a headline and intro paragraph above the recent-posts list."
section: "configuration"
order: 100
tags: []
uid: how-to.configuration.blogsite-hero
isDraft: true
search: false
llms: false
---

> **In this page.** Filling out `HeroContent` (headline, intro paragraph, CTA) and how the homepage layout renders it above the recent-posts list.
>
> **Not in this page.** Replacing the hero component entirely — see Extensibility.

## When to use this

- Outline bullet: You have a working `BlogSite` (via `AddBlogSite` / `UseBlogSite`) and want a personal greeting block above the recent-posts list on `/`.
- Outline bullet: You want headline + intro text (and optionally a CTA link) without replacing the `Home.razor` page.
- Outline bullet: Link back to the getting-started tutorial if the reader has not yet wired `AddBlogSite`.

## Assumptions

- Bullet: You have a `BlogSite`-based project (`services.AddBlogSite(() => new BlogSiteOptions { ... })`).
- Bullet: Your homepage is the default `Pennington.BlogSite` `Home.razor` (you have not swapped it).
- Bullet: You are editing the `BlogSiteOptions` initializer in `Program.cs`.
- Bullet: To copy a working setup, point at [`examples/AlexBlogExample`](https://github.com/phil-scott-78/Pennington/tree/main/examples/AlexBlogExample).

---

## Steps

### 1. Set `HeroContent` on `BlogSiteOptions`

- Bullet: `HeroContent` is a two-field record: `public record HeroContent(string Title, string Description);` in `src/Pennington.BlogSite/BlogSiteOptions.cs`.
- Bullet: The property is nullable (`HeroContent? HeroContent { get; init; }`); leaving it null hides the hero block entirely.
- Bullet: Fence the canonical wiring from `AlexBlogExample/Program.cs` (raw-file fence of `examples/AlexBlogExample/Program.cs` showing the `HeroContent = new HeroContent(...)` assignment — this example has no xmldocid symbol; top-level statements only per examples-inventory).

```csharp
HeroContent = new HeroContent(
    "Hi, I'm Alex",
    "I write about .NET, developer tooling, and the occasional deep dive into something unexpected."),
```

### 2. Write the headline as plain text in `Title`

- Bullet: `Title` is rendered through an `<h1>` inside a `prose` wrapper (see `src/Pennington.BlogSite/Components/Pages/Home.razor`, lines 23–29).
- Bullet: Keep it a single short line — no markdown or HTML; Blazor escapes it via the standard `@` expression.
- Bullet: Typography comes from `prose-headings:font-display`, so the display font configured in `BlogSiteOptions.DisplayFontFamily` applies automatically.

### 3. Write the intro paragraph (and any CTA link) in `Description`

- Bullet: `Description` is injected as `@((MarkupString)BlogOptions.HeroContent.Description)` in `Home.razor` — the string is emitted as raw HTML, so inline tags survive.
- Bullet: This is the only place to put a call-to-action: include an `<a href="...">` inside the string, since `HeroContent` has no separate CTA field.
- Bullet: The string is wrapped in a single `<p>`, so block-level elements (additional `<p>`, `<ul>`, headings) will produce invalid nesting — keep the markup inline.
- Bullet: Example CTA-style description: `"I write about .NET tooling — <a href=\"/archive\">browse the archive</a>."`

### 4. Keep the hero brief

- Bullet: The hero sits in `mx-auto w-full max-w-7xl` with `mb-16 lg:mb-24` margin before the `BlogSummary` posts list — long content pushes the fold.
- Bullet: One headline line + one or two sentences is the shape `prose lg:prose-lg` is tuned for.

---

## Verify

- Bullet: Run `dotnet run` from the blog project and load `/`.
- Bullet: Expect the `<h1>` headline and the intro paragraph to appear above the recent-posts list; expect the sidebar (My Work / Socials) to be unaffected.
- Bullet: Remove `HeroContent` (set to `null` or delete the property) and reload — the entire hero `<div>` disappears (guarded by `@if (BlogOptions.HeroContent != null)` in `Home.razor`).

## Related

- Reference: `BlogSiteOptions` reference page (under `reference/blogsite/`) for the full option surface including `HeroContent`, `MyWork`, `Socials`, `MainSiteLinks`.
- Background: Explanation page on the BlogSite homepage layout (why the hero, recent posts, and sidebar are composed the way they are) under `explanation/blogsite/`.
- Extensibility: How-to on replacing the BlogSite homepage component entirely (out of scope here).
