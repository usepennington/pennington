# Post-mortem — BlogSiteHeroProjectsSocialsExample

## What was built

`examples/BlogSiteHeroProjectsSocialsExample/` — the third BlogSite
tutorial app. `Program.cs` extends the tutorial-8 shape by populating the
four homepage surfaces: `HeroContent`, `MyWork` (3 projects), `Socials`
(all four built-in icon kinds), and `MainSiteLinks` (Home / Archive /
Tags). One post (`weekend-content-engine.md`) keeps the recent-posts slot
non-empty. Three stage classes — `Stage1_HeroOnly`, `Stage2_AddProjects`,
`Stage3_AddSocialsAndHeader` — each expose a static `Run(string[])` the
tutorial extracts via `csharp:xmldocid,bodyonly`.

## Record shapes — locked for app #14

All four live in `src/Pennington.BlogSite/BlogSiteOptions.cs` as
positional `record`s. None have defaults; every parameter is required at
construction:

- `HeroContent(string Title, string Description)` — `Description` is
  rendered as `MarkupString` in `Home.razor`, so inline HTML would pass
  through. The tutorial keeps it plain prose.
- `Project(string Title, string Description, string Url)` — renders as
  a `<dt>`/`<dd>` pair inside an `<a href="{Url}">` on the home page.
- `SocialLink(RenderFragment Icon, string Url)` — the important one.
  `Icon` is a `Microsoft.AspNetCore.Components.RenderFragment`, NOT a
  component type, NOT a generic parameter, NOT a string name. The home
  page deconstructs the record (`var (icon, link) in Options.Socials`)
  and renders `@icon` inside an `<a href="@link">`.
- `HeaderLink(string Title, string Url)` — rendered in both the header
  top-nav and the footer nav of `MainLayout.razor` (so every header
  link appears twice in the output).

## Built-in social icons — complete set

All four ship as `public static readonly RenderFragment` fields on the
`SocialIcons` Razor component at
`src/Pennington.BlogSite/Components/SocialIcons.razor` (namespace
`Pennington.BlogSite.Components`, assembly `Pennington.BlogSite.dll`):

- `SocialIcons.GithubIcon` (1 SVG path)
- `SocialIcons.BlueskyIcon` (1 SVG path)
- `SocialIcons.LinkedInIcon` (4 SVG paths)
- `SocialIcons.MastodonIcon` (2 SVG paths)

Each is a 24×24 viewBox SVG that inherits `currentColor` from the
surrounding `<a>` tag (the Home.razor anchor sets the text color class).
App #14 should use the exact field syntax `SocialIcons.GithubIcon`, not
a component tag — these are fragments, not components.

## Rendering quirks

- The "My Work" card + socials row is hidden on mobile (`hidden
  lg:block`) — manual visual checks must use desktop viewport.
- `HeaderLink` items appear both in the top-nav and the footer nav, so
  grepping rendered HTML for a header title yields two hits per link
  (three with the mobile menu duplicate).
- `HeroContent.Description` goes through `MarkupString`.

## Verification

`dotnet build Pennington.slnx` clean, 0 errors. Dev server on
`http://localhost:5530/` — Playwright confirmed hero heading + prose,
Site nav with Home/Archive/Tags, "My Work" card listing all three
projects with correct URLs, four social anchors each containing a real
`<svg viewBox="0 0 24 24">` with the expected path counts, and
`/archive` resolving 200. Static build produced 11 pages; `index.html`
carried the hero headline, "My Work", all three project names, github
and bluesky URLs, and the header links. `output/` cleaned.

No blockers.
