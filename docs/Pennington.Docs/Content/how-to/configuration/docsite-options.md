---
title: "Configure DocSite"
description: "Set the common DocSite options for branding, links, styling, Roslyn support, and advanced site structure."
section: "configuration"
order: 20
tags: []
uid: how-to.configuration.docsite-options
isDraft: true
search: false
llms: false
---

> **In this page.** Filling out `DocSiteOptions` (fonts, colors, header/footer content, GitHub URL, social image, solution path, areas).
>
> **Not in this page.** Runtime Razor overrides of DocSite layouts — see the customization how-to.

## When to use this

- You already have a DocSite running and want to finish the common site-level setup in one place.
- You want to brand the shell, set the main links, and optionally connect Roslyn-backed snippets.

## Assumptions

- You have a working DocSite (see [/tutorials/docsite/scaffold](/tutorials/docsite/scaffold) if not).
- You can edit the `new DocSiteOptions { ... }` block in `Program.cs`.
- For a full property list, use the reference page for [`DocSiteOptions`](/reference/options/docsite-options).

To copy a working setup, see [`examples/SearchExample`](https://github.com/Phil-Scott-Thomas/Pennington/tree/main/examples/SearchExample) or [`examples/BeaconDocsExample`](https://github.com/Phil-Scott-Thomas/Pennington/tree/main/examples/BeaconDocsExample).

---

## Steps

### 1. Set the color scheme

- Pick the color scheme you want the docs site to ship with.
- Leave this alone if the default look is already close to what you want.
- Verify the result on the home page before you move on to typography and header polish.

```csharp raw-file="examples/TempoDocsExample/Program.cs"
```

_This example shows the color scheme configured inline in `Program.cs`._

### 2. Set fonts (body + display)

- Set `BodyFontFamily` and `DisplayFontFamily` if the defaults are not the look you want.
- Load the fonts through `AdditionalHtmlHeadContent` or self-host them and add `FontPreloads`.
- Check one article page and one navigation-heavy page before you lock the typography in.

### 3. Set header icon, header content, and footer content

- Add `HeaderIcon` if you want a branded symbol next to the site title.
- Add `HeaderContent` for a short badge or status label.
- Add `FooterContent` for copyright, support links, or other footer-level text.

```csharp raw-file="examples/BeaconDocsExample/Program.cs"
```

_This example shows the icon, header badge, and footer content set together._

### 4. Set GitHub URL, canonical base URL, and social image

- Set `GitHubUrl` if the header should link back to the repository.
- Set `CanonicalBaseUrl` to the production URL you will publish at.
- Set `SocialImageUrl` if you already have an Open Graph image ready for the site.

### 5. (Optional) Set `SolutionPath` for Roslyn-backed code fences

- Set `SolutionPath` only if you are also wiring the `Pennington.Roslyn` package.
- Keep the path in the same edit as the Roslyn service registration so the setup stays easy to understand.
- Skip this step entirely if your site does not use xmldocid-backed snippets.

```csharp raw-file="examples/PrismDocsExample/Program.cs"
```

_This example shows `AddDocSite`, `AddPenningtonRoslyn`, and `SolutionPath` together._

### 6. (Advanced knobs) ExtraStyles, AdditionalHtmlHeadContent, AdditionalRoutingAssemblies, FontPreloads, Areas

- Add `ExtraStyles` only when the main theme options are not enough.
- Add `AdditionalHtmlHeadContent` for font links, analytics, or other head-only markup.
- Add `AdditionalRoutingAssemblies` only if the site needs to discover extra Razor routes.
- Add `FontPreloads` when you self-host fonts and want the browser to fetch them early.
- Leave `ConfigureLocalization` and `Areas` to their dedicated how-to or reference pages unless you already know you need them.

---

## Verify

- Run `dotnet run` and confirm the header, footer, colors, and fonts reflect the values you set.
- View page source to confirm the canonical URL and social image are present when configured.
- If you enabled Roslyn, load a page with an xmldocid fence and confirm it resolves.

## Related

- Reference: [DocSiteOptions](/reference/docsite-options) — full property-by-property listing including `Areas`, `FontPreloads`, and `ConfigureLocalization`.
- Reference: [MonorailCssOptions](/reference/monorailcss-options) — color scheme and content path details.
- Background: [DocSite architecture](/explanation/docsite-architecture) — why options are flat on the record rather than composed.
