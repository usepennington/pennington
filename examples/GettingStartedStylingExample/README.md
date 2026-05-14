# GettingStartedStylingExample

Adds MonorailCSS to the minimal host. Shows how `AddMonorailCss` + `UseMonorailCss` light up utility classes in a shared layout without leaving the bare-host shape.

## Concepts

- `AddMonorailCss(_ => new MonorailCssOptions { ... })` with `NamedColorScheme`
- `app.UseMonorailCss()` mounting `/styles.css` as a live endpoint
- Where utility classes live (the layout, not the markdown body)
- Brand palette vs. syntax theme split — `Content/index.md` includes a C# fence so the rendered home page shows both palettes side by side
- Built-in `ColorName` values (`Red`, `Orange`, …, `Slate`, `Mauve`, `Olive`, `Mist`, `Taupe`, `Black`, `White`) ship as static properties on `Pennington.MonorailCss.ColorName`; the auto-generated type page at `<xref:reference.api.color-name>` lists them with summaries.

## Tutorial stages

`Stage1_WithoutStyling.cs` → `Stage2_AddMonorailCss.cs` → `Stage3_UseMonorailCss.cs`; shared chrome in `Layout.cs`.

## Referenced from

- `docs/.../tutorials/getting-started/styling.md`
