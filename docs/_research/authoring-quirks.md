# Authoring quirks — reference for doc authors

Captured from the worked-example postmortems and plan P3. Each entry is a
non-obvious API detail that a reader writing their first site will hit. They
belong in the future `/explanation/` or `/reference/` pages; until those are
written, docs authors should draw from this sheet rather than re-discovering
each surface.

## MonorailCSS

### `ColorNames` lives in `MonorailCss.Theme`, not `MonorailCss`
**Source:** `postmortem-GettingStartedStylingExample.md`

`using MonorailCss;` on its own doesn't expose the `ColorNames.Blue`, `.Purple`
etc. constants used by `NamedColorScheme`. They live in
`MonorailCss.Theme`. Authoring the styling tutorial must show both usings:

```csharp
using MonorailCss;         // NamedColorScheme, AlgorithmicColorScheme
using MonorailCss.Theme;   // ColorNames
```

### `NamedColorScheme` has five required `init` properties
**Source:** `postmortem-GettingStartedStylingExample.md`

`PrimaryColorName`, `AccentColorName`, `TertiaryOneColorName`,
`TertiaryTwoColorName`, and `BaseColorName` are all `required init`. There is
no partial-configuration path — a tutorial showing the scheme must populate
all five. This is intentional (the palette is designed to be complete) but
worth calling out so readers don't try `new NamedColorScheme { PrimaryColorName = ... }`.

## BlogSite

### `SocialLink.Icon` is a `RenderFragment`, not a component type or string
**Source:** `postmortem-BlogSiteHeroProjectsSocialsExample.md`

Users write:

```csharp
new SocialLink("Bluesky", "https://bsky.app/...", Icon: SocialIcons.BlueskyIcon)
```

where `SocialIcons.BlueskyIcon` is a `RenderFragment` instance. Not a
`Type`, not an enum, not a string tag. The pre-baked fragments live on
`Pennington.BlogSite.SocialIcons`; custom icons pass any other `RenderFragment`.

### Blog post default template does not render `author` on the page body
**Source:** `postmortem-BlogSiteFirstPostExample.md`

The `author:` front-matter field flows through to RSS and footer copyright,
but the default post template has no byline surface. A tutorial teaching
front matter should note this so readers don't look for the author name on
the rendered page.

### Blog post `YYYY-MM-DD-` filename prefix is preserved in the URL
**Source:** `postmortem-BlogKitchenSinkExample.md`

`BlogContentResolver` does not strip date prefixes from filenames. A post at
`Content/Blog/2024-01-15-getting-started.md` produces the URL
`/blog/2024-01-15-getting-started/`. Authors wanting clean URLs should drop
the date from the filename and keep it in `date:` front matter only. By
design — the date prefix is a sorting aid for the authoring file system,
not a URL convention.

## Navigation

### `NavigationBuilder.BuildTree` requires caller to aggregate TOC entries
**Source:** `postmortem-GettingStartedFirstPageExample.md`

`BuildTree` does not pull from DI. Callers on bare `AddPennington` hosts
must loop every `IContentService.GetIndexableEntriesAsync()` and concatenate:

```csharp
var allEntries = new List<ContentTocItem>();
foreach (var service in services)
    allEntries.AddRange(await service.GetIndexableEntriesAsync());
var tree = navigationBuilder.BuildTree(allEntries);
```

DocSite and BlogSite templates do this aggregation internally, so the
quirk only surfaces on bare hosts.

### Root `index.md` does not appear in nav by default
**Source:** `postmortem-GettingStartedFirstPageExample.md`

`NavigationBuilder.BuildLevel` filters by path depth — entries with an
empty hierarchy (i.e., the root `index.md`) fall out. This is a design
choice: navigation is usually meant to complement a "Home" link rather
than include it. Tutorials that want a "Home" entry need a different nav
strategy (static header link, or explicit `ContentTocItem`).

## Authoring pages that reference these items

- Styling tutorial — `/tutorials/getting-started/styling` (§1.1.30): fold
  the two `MonorailCss` usings and the five-required-fields note into
  prose around `BuildColorScheme`.
- Blog socials tutorial — `/tutorials/blogsite/hero-projects-socials`
  (§1.3.30): show `SocialIcons.BlueskyIcon` explicitly as a
  `RenderFragment` — the fact that it's not a type avoids a common
  TypeScript-imported-mindset misstep.
- Blog first-post tutorial — `/tutorials/blogsite/first-post` (§1.3.20):
  add a sidebar that `author:` shows up in RSS / footer copyright, not
  the post body.
- Blog "customize URLs" how-to — call out the date-prefix convention at
  the natural place (likely `/how-to/content-authoring/front-matter` or
  a future blog-URL-shape reference).
- Bare-host navigation how-to or reference — document the TOC
  aggregation pattern and the root-index filtering.

Each of these items is already locked into its source postmortem; this
file is a single-page cheat sheet so the writer does not have to rediscover
them across seventeen postmortem files.
