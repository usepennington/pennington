# Post-mortem — GettingStartedMinimalSiteExample

## What was built

`examples/GettingStartedMinimalSiteExample/` — the smallest possible Pennington
host that backs tutorial §1.1.10. A top-level `Program.cs` wires
`AddPennington` + `UsePennington`, plus a single `MapGet("/{*path}", ...)`
endpoint that walks `IEnumerable<IContentService>`, drives the discovered item
through `IContentParser`/`IContentRenderer`, and returns bare HTML. The host
references only `Pennington` (no `Pennington.DocSite`, `Pennington.UI`, or
`Pennington.MonorailCss`) so the tutorial stays at the engine level.

Three stage files (`Stage1_BareHost.cs`, `Stage2_AddPennington.cs`,
`Stage3_UsePennington.cs`) contain static methods whose bodies correspond to
the tutorial's intermediate states. They compile but are never invoked; the
tutorial pulls each body via `csharp:xmldocid,bodyonly`.

## Verification

- `dotnet build Pennington.slnx` — succeeds, 0 new warnings/errors.
- Dev server — Playwright confirmed `http://localhost:5455/` renders with page
  title "Welcome to your first Pennington site" from front matter plus the
  markdown body.
- Static build — `dotnet run -- build _testoutput` reports
  `Build Complete — 3 pages in 0.3s` (index, sitemap.xml, search-index-en.json)
  with no build-report errors.

## API surprises / follow-ups

- **`OutputOptions.FromArgs` positional layout** — FIXED (plan P2-3).
  `FromArgs` now accepts named flags alongside legacy positional: prefer
  `dotnet run -- build --base-url /sub --output dist` (or `=`-joined
  forms) over the old `build <baseUrl> <outputDir>` positional shape.
  Positional stays supported for back-compat; flags win when both appear
  and also let you fill one slot by flag and the other positional
  (`build --base-url=/sub dist`). Regression coverage: eight new cases
  in `tests/Pennington.Tests/Generation/OutputOptionsTests.cs`.
- **Core `AddPennington` does not wire a content-page route handler.** The
  DocSite and BlogSite templates add Razor routing on top. A bare host has to
  bring its own endpoint — this example's `MapGet` fallback is the minimum
  shape. Subsequent `GettingStarted*` apps (#2, #3) should mirror this exact
  endpoint shape so the tutorial progression reads cleanly.
- **Convention established for stage files.** Static class named `StageN`,
  single static method `Run(string[] args)` taking the web host's args. Keep
  this shape in later tutorial apps that need intermediate states.

No blockers.
