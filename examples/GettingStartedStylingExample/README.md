# GettingStartedStylingExample

Layers MonorailCSS onto the Blazor-pages site from the previous tutorial. Adds a styled `MainLayout.razor` that wraps every routed `@page`, registers `AddMonorailCss` with a `NamedColorScheme`, and mounts `/styles.css` via `UseMonorailCss`.

## Concepts

- `MainLayout.razor` (Blazor `LayoutComponentBase`) holds the utility-class scaffold; Discovery's startup IL scan reads those classes out of the compiled Razor.
- `AddMonorailCss(...)` chooses the named palettes behind the `primary`, `accent`, and `base` utility prefixes and wires the Discovery pipeline (`AddMonorailClassDiscovery`) under the hood.
- `UseMonorailCss()` mounts `/styles.css`, which serves the class set from `IClassRegistry` on every request. Under `dotnet watch`, Discovery's source-file watcher re-extracts class strings from `.razor`/`.cs` edits — a new utility added to those files shows up in the stylesheet on the next request, no restart. Markdown edits do not participate (Discovery scans source, not rendered HTML).

## Tutorial stages

`Stage1_BeforeStyling.cs` (Blazor catch-all, no MonorailCSS) → `Stage2_AddMonorailCss.cs` (DI registered, endpoint not mounted) → `Stage3_UseMonorailCss.cs` (`/styles.css` live, page renders styled).

## Referenced from

- `docs/.../tutorials/getting-started/styling.md`
