# DocSiteChromeOverridesExample

Live wiring behind "override DocSite components." `SiteChromeOverrides.cs` returns a populated `DocSiteOptions` exercising all four chrome seams; `Program.cs` stays one line so the helper is the teaching surface.

## Concepts

- **Head-slot fragment** via `Components/ExtraHeadFragment.razor` — wired with `DocSiteOptions.HeadFragment = typeof(ExtraHeadFragment)` in `SiteChromeOverrides.cs`.
- **Custom routed `@page`** via `Components/ExtraPage.razor` — see the `@page "/extra"` directive. Both `/extra` and `/extra/` return 200; the example uses the no-trailing-slash form to match Blazor's `@page` convention, but DocSite content routes elsewhere on the site keep the trailing slash. Pick one form and link consistently in your own content.
- **`AdditionalRoutingAssemblies`** widening the router to the host assembly — set in `SiteChromeOverrides.cs` as `AdditionalRoutingAssemblies = [typeof(Program).Assembly]`. Passing a non-routable assembly (one without `@page` directives) is a silent no-op — Blazor's `Router` simply finds zero routes in it and moves on. No throw, no log.
- **`DocSiteOptions` overrides** for header / footer / layout chrome — `HeaderContent`, `FooterContent`, layout component references on the options object in `SiteChromeOverrides.cs`.
- **`NotFound.razor` 404 body** via `Components/NotFound.razor` — a non-routed component named `NotFound`. With no `Content/404.md` present, DocSite's catch-all finds it by reflection and renders it for any unmatched URL. No `@page`, and no routing-assembly wiring needed (the scan walks every loaded assembly).

## Referenced from

- `docs/.../how-to/response-pipeline/override-docsite-components.md`
- `docs/.../how-to/pages/not-found-page.md` (`Components/NotFound.razor` fence)
