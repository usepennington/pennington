---
title: "Add social links and header navigation"
description: "Adding SocialLink entries with the built-in icons (BlueskyIcon, GithubIcon, etc.), populating MainSiteLinks with HeaderLink entries, and where each surfaces in the rendered chrome."
section: "configuration"
order: 120
tags: []
uid: how-to.configuration.blogsite-socials
isDraft: true
search: false
llms: false
---

> **In this page.** Adding `SocialLink` entries with the built-in icons (`BlueskyIcon`, `GithubIcon`, etc.), populating `MainSiteLinks` with `HeaderLink` entries, and where each surfaces in the rendered chrome.
>
> **Not in this page.** Custom icon components — see the Razor-component how-to in Extensibility.

## When to use this

- Outline bullet: You have a working `BlogSite` wired with `AddBlogSite` / `UseBlogSite` / `RunBlogSiteAsync` and need to populate the header navigation and the social-icon strip that ships with the default blog chrome.
- Outline bullet: You are using the built-in social icons (`GithubIcon`, `LinkedInIcon`, `BlueskyIcon`, `MastodonIcon`) shipped from `Pennington.BlogSite.Components.SocialIcons` — custom icons belong in the Extensibility how-to.

## Assumptions

- Outline bullet: You have an existing `BlogSite` (see `/tutorials/getting-started/first-site` if not).
- Outline bullet: You can edit the `new BlogSiteOptions { ... }` factory passed to `AddBlogSite` in `Program.cs`.
- Outline bullet: `BlogSiteOptions.Socials` (`SocialLink[]`) and `BlogSiteOptions.MainSiteLinks` (`HeaderLink[]`) are verified in `src/Pennington.BlogSite/BlogSiteOptions.cs`; the four built-in icon `RenderFragment`s are verified in `src/Pennington.BlogSite/Components/SocialIcons.razor`.
- Outline bullet: To copy a working setup, see [`examples/BlogExample`](https://github.com/Phil-Scott-Thomas/Pennington/tree/main/examples/BlogExample) — it is the only inventory example that sets both `Socials` and `MainSiteLinks`. Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

### 1. Import `SocialIcons` alongside `BlogSiteOptions`

- Outline bullet: Add `using Pennington.BlogSite.Components;` to `Program.cs` so the four static `RenderFragment` fields on `SocialIcons` are in scope alongside `BlogSiteOptions`.
- Outline bullet: The icons are `public static readonly RenderFragment` fields, not components — you reference them directly (`SocialIcons.GithubIcon`), no angle-bracket syntax.
- Outline bullet: Available built-ins are `GithubIcon`, `LinkedInIcon`, `BlueskyIcon`, `MastodonIcon`. Anything else requires a custom `RenderFragment` (see the Extensibility how-to).

### 2. Populate `Socials` with `SocialLink(Icon, Url)` entries

- Outline bullet: Assign a `SocialLink[]` to `BlogSiteOptions.Socials`; each `SocialLink` is `record SocialLink(RenderFragment Icon, string Url)` — first positional is the icon fragment, second is the destination URL.
- Outline bullet: Order matters: entries render left-to-right in the home-page sidebar strip in the order you list them.
- Outline bullet: `Url` is passed through verbatim into an `<a href>` — use a full `https://` URL for external profiles; `#` is tolerated as a placeholder during development.
- Outline bullet: Leaving `Socials` as the default empty array collapses the sidebar's social strip entirely (rendered inside an `@if (BlogOptions.Socials.Length > 0)` gate in `Home.razor`).

### 3. Populate `MainSiteLinks` with `HeaderLink(Title, Url)` entries

- Outline bullet: Assign a `HeaderLink[]` to `BlogSiteOptions.MainSiteLinks`; each `HeaderLink` is `record HeaderLink(string Title, string Url)` — `Title` becomes the visible anchor text (uppercased by the layout's utility classes), `Url` becomes the `href`.
- Outline bullet: These entries surface in three places in `MainLayout.razor`: the desktop header nav, the mobile collapsible menu (toggled by the hamburger button), and the footer nav.
- Outline bullet: Leaving `MainSiteLinks` as the default empty array hides the desktop nav, the mobile menu toggle, and the footer nav — all three are gated on `Options.MainSiteLinks.Length > 0`.
- Outline bullet: Mix internal paths (`/about`) and external URLs (`https://github.com/sponsors/...`) freely — nothing on the layout differentiates them.

### 4. Apply both on `BlogSiteOptions` inside `AddBlogSite`

- Outline bullet: Set `Socials = [...]` and `MainSiteLinks = [...]` as collection-expression initializers on the `BlogSiteOptions` record returned from the factory passed to `AddBlogSite`.
- Outline bullet: Both properties are `init`-only on a `record`; you set them once inside the object initializer, not via later mutation.
- Outline bullet: No service registration or middleware call is required beyond the existing `AddBlogSite` / `UseBlogSite` pair — the layout reads the options directly via `@inject BlogSiteOptions Options`.

```csharp:path
examples/BlogExample/Program.cs
```

- Outline bullet: Snippet source — `BlogExample/Program.cs` shows both `MainSiteLinks` (two `HeaderLink` entries) and `Socials` (four `SocialLink` entries using all four built-in icons) set inside one `BlogSiteOptions` initializer. (Raw-file fence: `Program.cs` is top-level statements with no xmldocid-addressable symbol.)

---

## Verify

- Outline bullet: Run `dotnet run` and visit `/` — the header shows each `HeaderLink.Title` (uppercased) on desktop, and a hamburger toggle reveals the same list on mobile.
- Outline bullet: Scroll to the footer — the same `MainSiteLinks` list repeats as the footer nav.
- Outline bullet: On the home page sidebar (visible at `lg` breakpoint and up), the social icons strip renders each `SocialLink` in declared order, each wrapping its `Icon` fragment in an `<a href="@Url">`.

## Related

- Reference: [BlogSiteOptions](/reference/blogsite-options) — full property listing including `Socials`, `MainSiteLinks`, `MyWork`, and `HeroContent`.
- Reference: [SocialIcons built-ins](/reference/blogsite-social-icons) — the four shipped `RenderFragment` icons and their SVG viewBox sizing.
- Background: [Blog chrome composition](/explanation/blogsite-chrome) — why socials and header links live on the options record instead of being component parameters.
