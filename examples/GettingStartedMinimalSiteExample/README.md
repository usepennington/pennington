# GettingStartedMinimalSiteExample

The smallest viable Pennington host: `AddPennington` plus `AddMarkdownContent<DocFrontMatter>`, a single catch-all `MapGet` that resolves a URL through `IPageResolver`, and `RunOrBuildAsync` to switch between live serve and static build.

## Concepts

- `AddPennington` / `UsePennington` minimum wiring
- One markdown source rooted at `Content/` mapped to `/`
- Catch-all rendering via `IPageResolver` (the shape DocSite later replaces — the tutorial's "From `MapGet` to `UseDocSite`" section shows the two side-by-side)
- `RunOrBuildAsync` — `dotnet run` serves; `dotnet run -- build` writes static HTML
- Output is intentionally unstyled — no MonorailCSS, plain browser defaults. The next tutorial (`GettingStartedStylingExample`) adds the CSS layer.

## Tutorial stages

`Stage1_BareHost.cs` → `Stage2_AddPennington.cs` → `Stage3_UsePennington.cs` — pulled by xmldocid into the tutorial.

## Referenced from

- `docs/.../tutorials/getting-started/first-site.md`
