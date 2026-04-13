---
title: "Configure fonts and typography"
description: "Set DisplayFontFamily/BodyFontFamily on DocSiteOptions, declare FontPreloads, and serve font assets."
section: "configuration"
order: 60
tags: []
uid: how-to.configuration.fonts
isDraft: true
search: false
llms: false
---

> **In this page.** Setting `DisplayFontFamily`/`BodyFontFamily` on `DocSiteOptions`, declaring `FontPreloads`, and serving font assets.
>
> **Not in this page.** Self-hosting vs. Google Fonts trade-offs (out of scope).

## When to use this

- Outline bullet: You already have a DocSite wired with `AddDocSite` / `UseDocSite` / `RunDocSiteAsync` and want to override the default font stack with a custom display face (headings) and body face (prose).
- Outline bullet: You have already decided where font files come from (Google Fonts CDN, self-hosted `.woff2` under `wwwroot/`, or another host); this page wires the chosen source into the DocSite, it does not help you pick one.

## Assumptions

- Outline bullet: You have a working DocSite (see `/tutorials/getting-started/first-site` if not).
- Outline bullet: You can find `Program.cs` and already have a `new DocSiteOptions { SiteTitle = ..., Description = ... }` factory in place.
- Outline bullet: `DisplayFontFamily`, `BodyFontFamily`, and `FontPreloads` are verified on `DocSiteOptions` in `src/Pennington.DocSite/DocSiteOptions.cs`; `FontPreload` is `record FontPreload(string Href, string Type = "font/woff2")` in `src/Pennington/Infrastructure/FontPreload.cs`.
- Outline bullet: To copy a working setup, see [`examples/SearchExample`](https://github.com/Phil-Scott-Thomas/Pennington/tree/main/examples/SearchExample) — a DocSite that sets both font families plus a Google Fonts stylesheet via `AdditionalHtmlHeadContent`. Do not walk through it end-to-end — this is a recipe, not a tour.

---

## Steps

### 1. Pick the two font families

- Outline bullet: `DisplayFontFamily` is applied to headings and other display-weight type; `BodyFontFamily` is applied to prose.
- Outline bullet: Both are `string?` CSS `font-family` values — pass the full stack including fallbacks, e.g. `"Manrope, sans-serif"` or `"\"Noto Sans Display\", sans-serif"` (escape internal quotes).
- Outline bullet: Leave either null to accept the DocSite default stack — you do not need to set both to set one.
- Outline bullet: Pennington only wires the CSS variables that MonorailCSS consumes; it does not fetch font files. The next step handles that.

### 2. Set `DisplayFontFamily` and `BodyFontFamily` on `DocSiteOptions`

- Outline bullet: Assign both properties inside the `DocSiteOptions` factory passed to `AddDocSite(...)`.
- Outline bullet: Order inside the record literal does not matter — the DocSite reads these after the full record is constructed.

```csharp raw-file="examples/SearchExample/Program.cs"
```

- Outline bullet: Snippet source — `SearchExample/Program.cs` shows `BodyFontFamily = "Manrope, sans-serif"` and `DisplayFontFamily = "Petrona, serif"` set inside the `DocSiteOptions` factory. (Raw-file fence: `Program.cs` is top-level statements with no xmldocid-addressable symbol.)

### 3. Deliver the font CSS to the browser

- Outline bullet: Setting the family properties alone does nothing if the browser has no font file — you must also ship the `@font-face` declarations or a stylesheet that contains them.
- Outline bullet: For a hosted service (Google Fonts, Bunny Fonts, etc.), inject the provider's `<link>` tags via `AdditionalHtmlHeadContent` on `DocSiteOptions`; the same example sets `AdditionalHtmlHeadContent` with `preconnect` hints plus the `fonts.googleapis.com` stylesheet URL.
- Outline bullet: For self-hosted files, drop `.woff2` assets under the project's `wwwroot/` (e.g. `wwwroot/fonts/manrope.woff2`) so ASP.NET static-file middleware serves them at `/fonts/manrope.woff2`, then emit your own `@font-face` CSS via `ExtraStyles` (or a stylesheet linked through `AdditionalHtmlHeadContent`).
- Outline bullet: Either way the family names in step 2 must match the `font-family` names declared in the CSS the browser loads.

### 4. (Optional) Declare `FontPreloads` for self-hosted fonts

- Outline bullet: `FontPreloads` is a `FontPreload[]` on `DocSiteOptions`; each entry emits a `<link rel="preload" href="..." as="font" type="..." crossorigin>` into `<head>` (rendered by `App.razor` in `Pennington.DocSite`).
- Outline bullet: Construct entries with `new FontPreload(Href, Type)` — `Type` defaults to `"font/woff2"`, so you can omit it for `.woff2` assets.
- Outline bullet: Only preload the one or two font files that appear in above-the-fold text; preloading every weight hurts more than it helps.
- Outline bullet: Preloads are unnecessary when the font comes from a third-party CDN that already sends its own resource hints — use this for self-hosted assets.

```csharp
FontPreloads =
[
    new FontPreload("/fonts/manrope-variable.woff2"),
    new FontPreload("/fonts/petrona-variable.woff2"),
],
```

### 5. Rebuild the MonorailCSS stylesheet

- Outline bullet: Fonts take effect after the next MonorailCSS regeneration — restart `dotnet run` (or save a file to trigger live reload) so `/styles.css` picks up the new `font-family` values.
- Outline bullet: No extra configuration is needed in `MonorailCssOptions`; the DocSite already threads the two family properties into the generated CSS.

---

## Verify

- Outline bullet: Run `dotnet run` and visit `/`; view page source and confirm `<link rel="preload" as="font" ...>` tags appear for each `FontPreloads` entry, and any `AdditionalHtmlHeadContent` `<link>` tags are present.
- Outline bullet: Open DevTools → Network, reload, and confirm the `.woff2` files return `200` (not `404`) and are served from the expected origin.
- Outline bullet: DevTools → Elements → pick a heading and a paragraph, inspect computed `font-family` — heading should resolve to `DisplayFontFamily`, body text to `BodyFontFamily`.

## Related

- Reference: [DocSiteOptions](/reference/options/docsite-options) — full property list including `DisplayFontFamily`, `BodyFontFamily`, `FontPreloads`, `AdditionalHtmlHeadContent`, `ExtraStyles`.
- Reference: [MonorailCssOptions](/reference/options/monorailcss-options) — how generated CSS consumes the family values.
- Background: [DocSite architecture](/explanation/docsite-architecture) — why font wiring lives on `DocSiteOptions` rather than a separate typography record.
