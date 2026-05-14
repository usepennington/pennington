# GettingStartedStylingExample

Layers MonorailCSS onto the Blazor-pages site from the previous tutorial. Adds a styled `MainLayout.razor` that wraps every routed `@page`, registers `AddMonorailCss` with a `NamedColorScheme`, and mounts `/styles.css` via `UseMonorailCss`.

## Concepts

- `MainLayout.razor` (Blazor `LayoutComponentBase`) holds the utility-class scaffold the class collector reads from.
- `AddMonorailCss(...)` chooses the named palettes behind the `primary`, `accent`, and `base` utility prefixes.
- `UseMonorailCss()` mounts `/styles.css`. The class collector scans rendered HTML on every request, so a new utility class added to a markdown body shows up in the stylesheet on the next request — no restart.

## Tutorial stages

`Stage1_BeforeStyling.cs` (Blazor catch-all, no MonorailCSS) → `Stage2_AddMonorailCss.cs` (DI registered, endpoint not mounted) → `Stage3_UseMonorailCss.cs` (`/styles.css` live, page renders styled).

## Referenced from

- `docs/.../tutorials/getting-started/styling.md`
