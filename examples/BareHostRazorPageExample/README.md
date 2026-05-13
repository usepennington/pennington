# BareHostRazorPageExample

Rendering a Razor component as the entire response body of a `MapGet` route — no DocSite, no markdown pipeline. `HtmlRenderer` runs the component on the request thread via `Dispatcher.InvokeAsync` and returns the HTML string.

## Concepts

- `AddRazorComponents` + `AddHttpContextAccessor` wiring Blazor's `HtmlRenderer` on a bare host
- `IContentService` returning `EndpointSource` discoveries so the build crawler picks up sibling `MapGet` routes
- `GetContentTocEntriesAsync` (one `ContentTocItem` per published route — feeds the navigation builder + search index) and `GetCrossReferencesAsync` (per-route `uid`/`xref` entries — feeds the xref resolver) on a hand-rolled service

## Referenced from

- `docs/.../how-to/response-pipeline/razor-page-on-bare-host.md`
