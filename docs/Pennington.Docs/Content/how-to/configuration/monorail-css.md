---
title: Customize MonorailCSS
description: Swap between NamedColorScheme and AlgorithmicColorScheme, inject CustomCssFrameworkSettings, add ExtraStyles, and configure ContentPaths for class collection.
section: configuration
order: 50
tags: []
uid: how-to.configuration.monorail-css
isDraft: true
search: false
llms: false
---

> **In this page.** Swap between `NamedColorScheme` and `AlgorithmicColorScheme`, inject `CustomCssFrameworkSettings`, add `ExtraStyles`, and configure `ContentPaths` for class collection.
>
> **Not in this page.** The `CssClassCollectorProcessor` internals (Explanation) or writing a standalone color scheme (advanced customization).

## When to use this

- You have a Pennington site running with the default palette and want a different primary/accent color, custom component styles, or need MonorailCSS to see classes that only appear in JS or static assets.
- You are comfortable with `AddMonorailCss` / `UseMonorailCss` wiring and want to edit the `MonorailCssOptions` you pass in.

## Assumptions

- You have an existing Pennington site with `AddMonorailCss(...)` in `Program.cs` and `app.UseMonorailCss()` already mapped.
- You are using the core Pennington package directly (or willing to drop the factory argument into `AddMonorailCss` even when the site template — `AddDocSite` / `AddBlogSite` — configures it for you).
- You know which `MonorailCss.Theme.ColorNames` values you want (e.g. `Sky`, `Slate`, `Zinc`) or have a primary hue in mind (0–360).

To copy a working setup, see [`examples/YogaStudioExample`](https://github.com/Pennington/Pennington/tree/main/examples/YogaStudioExample) (full customization — custom `IColorScheme`, `ExtraStyles`, `CustomCssFrameworkSettings`) or [`examples/SpectreConsoleExample`](https://github.com/Pennington/Pennington/tree/main/examples/SpectreConsoleExample) (minimal `NamedColorScheme` swap). Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

### 1. Pick a color scheme

Pass a `MonorailCssOptions` instance into `AddMonorailCss` and set `ColorScheme`. Use `NamedColorScheme` when you want the exact Tailwind palette names; use `AlgorithmicColorScheme` when you want a palette generated from a single hue (0–360).

Named-palette swap (from `examples/SpectreConsoleExample/Program.cs`):

```csharp raw="examples/SpectreConsoleExample/Program.cs" lines="44-54"
```

Algorithmic hue (from `examples/BeaconDocsExample/Program.cs`, reached through `DocSiteOptions.ColorScheme`):

```csharp raw="examples/BeaconDocsExample/Program.cs" lines="17-22"
```

### 2. Add extra global styles

Use `ExtraStyles` for raw CSS that is prepended to every generated stylesheet — font imports, `:root` custom properties, one-off selectors.

```csharp raw="examples/YogaStudioExample/Program.cs" lines="45-55"
```

### 3. Inject CustomCssFrameworkSettings

Use the `CustomCssFrameworkSettings` callback to mutate the `CssFrameworkSettings` record (add component `Applies`, change `Theme`, tweak `ProseCustomization`). The callback receives the defaults Pennington builds (code-block, tabs, alerts, `hljs`, search-modal applies) — compose with `with` instead of replacing the whole record.

```csharp raw="examples/YogaStudioExample/Program.cs" lines="56-59"
```

### 4. Scan non-HTML files with ContentPaths

Set `ContentPaths` to filenames relative to `wwwroot/` that contain classes MonorailCSS would otherwise miss — JS bundles, hand-written HTML fragments, data files. Paths are read once at `UseMonorailCss` startup through `WebRootFileProvider`; the scanner extracts `class="..."` attributes and delimiter-split tokens.

```csharp
builder.Services.AddMonorailCss(_ => new MonorailCssOptions
{
    ContentPaths = ["js/theme-toggle.js", "js/search-widget.js"],
});
```

### 5. Wire it up

Keep middleware order as-is: `AddMonorailCss` before `Build()`, `app.UseMonorailCss()` before `app.UsePennington()` (or after `MapRazorComponents<App>` in template-based sites). The stylesheet is served at `/styles.css` by default; pass a different path to `UseMonorailCss("/css/site.css")` if you need one.

---

## Verify

- Run `dotnet run` and request `/styles.css`; confirm colors reflect your scheme (search for `--color-primary-500` in the response).
- Confirm any `ExtraStyles` block appears at the very top of `/styles.css`.
- For `ContentPaths`: confirm a class that only exists in the scanned file (e.g. `data-theme-toggle` utility) is emitted in `/styles.css`.

## Related

- Reference: [`MonorailCssOptions`](/reference/options/monorail-css-options)
- Reference: [DI and middleware extension methods](/reference/host/extensions)
- Background: [MonorailCSS integration](/explanation/rendering/monorail-css)
