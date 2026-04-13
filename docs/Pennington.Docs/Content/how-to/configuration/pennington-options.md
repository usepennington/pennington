---
title: "Configure PenningtonOptions"
description: "Set SiteTitle, SiteDescription, CanonicalBaseUrl, ContentRootPath, and the nested option groups (Highlighting, Islands, Localization, SearchIndex, LlmsTxt)."
section: "configuration"
order: 10
tags: []
uid: how-to.configuration.pennington-options
isDraft: true
search: false
llms: false
---

> **In this page.** Setting `SiteTitle`, `SiteDescription`, `CanonicalBaseUrl`, `ContentRootPath`, and the nested option groups (`Highlighting`, `Islands`, `Localization`, `SearchIndex`, `LlmsTxt`).
>
> **Not in this page.** Each option's full schema (see Reference) or DocSite-specific options.

## When to use this

- You already have an `AddPennington(penn => ...)` call and need to tune the top-level engine knobs.
- You are wiring a bespoke site (not `AddDocSite` / `AddBlogSite`) and need to finish the common setup.
- You want a direct recipe for the options most custom hosts touch first.

## Assumptions

- You have an existing Pennington site calling `services.AddPennington(...)` (see the Getting Started tutorial if not).
- You can edit the `AddPennington` callback in `Program.cs`.
- For the full property schema (types, defaults, remarks), use the Reference: [`PenningtonOptions`](/reference/options/pennington-options).

To copy a working setup, see [`examples/MinimalExample`](https://github.com/phil-scott-78/Penn/tree/main/examples/MinimalExample). Do not walk through the whole example — this page is a recipe, not a tour.

---

## Steps

### 1. Set site metadata, content root, and routing assemblies

- Set `SiteTitle`, `SiteDescription`, and `ContentRootPath` first so the host has a clear identity and a clear content root.
- Set `CanonicalBaseUrl` if your custom site emits absolute URLs.
- Add `AdditionalRoutingAssemblies` when the host needs to discover your own Razor routes during build and runtime.
- Keep these site-wide settings together near the top of the callback.

```csharp raw-file="examples/MinimalExample/Program.cs"
```

### 2. Register markdown content sources

- Add one `AddMarkdownContent<TFrontMatter>` call per content tree you want Pennington to serve.
- Pick the front-matter type that matches the shape of that tree.
- If you need more than one source, add them one at a time and verify the URLs before moving on.

```csharp raw-file="examples/ForgePortalExample/Program.cs"
```

### 3. Add code highlighters via `penn.Highlighting`

- Add custom highlighters only if the built-in highlighting is not enough for your content.
- Register them under `penn.Highlighting` in the same callback so the setup stays readable.
- If you need Roslyn-backed C# snippets, wire the Roslyn package alongside the main Pennington setup.

### 4. Register islands via `penn.Islands`

- Register islands only if the site needs interactive SPA regions.
- Use one registration per slot name and keep the slot names aligned with your layout markup.
- Skip this step entirely for a content-only site.

```csharp raw-file="examples/SpaNavigationTutorialExample/Program.cs"
```

### 5. Configure localization via `penn.Localization`

- Set the default locale first, then add any additional locales.
- Add locale routing middleware in the app pipeline before your Razor endpoints.
- Use the dedicated localization how-to if you need the full translated-content flow.

```csharp raw-file="examples/YogaStudioExample/Program.cs"
```

- For deeper coverage (per-locale content folders, fallback, translations), see [Enable multiple locales](/how-to/configuration/localization).

### 6. Tune search indexing via `penn.SearchIndex`

- Change `penn.SearchIndex` only when the default indexing behavior is not enough.
- Start with `ContentSelector` and `DefaultPriority`, then stop unless you have a real ranking problem to solve.
- Verify the generated search JSON after each change.

### 7. Enable llms.txt via `penn.AddLlmsTxt`

- Call `penn.AddLlmsTxt` only if your custom host should emit `llms.txt`.
- Set the output location and content selector there if the defaults are not right.
- Stop after the endpoint works; deeper tuning belongs on the dedicated llms.txt how-to.

---

## Verify

- Run `dotnet run` and confirm the site title and content routes load as expected.
- Check `/search-index-<locale>.json` if you configured search.
- Check `/llms.txt` if you enabled llms.txt generation.

## Related

- Reference: [`PenningtonOptions`](/reference/options/pennington-options)
- Reference: [`HighlightingOptions`, `IslandsOptions`, `SearchIndexOptions`, `LlmsTxtOptions`, `OutputOptions`](/reference/options/auxiliary-options)
- Background: [The content pipeline and union types](/explanation/core/content-pipeline)
- Background: [The syntax-highlighting cascade](/explanation/rendering/highlighting)
