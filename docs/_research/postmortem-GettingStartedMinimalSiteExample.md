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

- **`OutputOptions.FromArgs` positional layout.** `build <baseUrl> <outputDir>`
  with only two args sends `_testoutput` into `BaseUrl` and defaults the output
  to `output/`. The static build still works — the baked-in `<body data-base-url>`
  just ends up as `_testoutput`. Later-app agents running the same verification
  command should expect the output at `output/`, not at the name they pass, and
  clean both paths.
- **Core `AddPennington` does not wire a content-page route handler.** The
  DocSite and BlogSite templates add Razor routing on top. A bare host has to
  bring its own endpoint — this example's `MapGet` fallback is the minimum
  shape. Subsequent `GettingStarted*` apps (#2, #3) should mirror this exact
  endpoint shape so the tutorial progression reads cleanly.
- **Convention established for stage files.** Static class named `StageN`,
  single static method `Run(string[] args)` taking the web host's args. Keep
  this shape in later tutorial apps that need intermediate states.

No blockers.
