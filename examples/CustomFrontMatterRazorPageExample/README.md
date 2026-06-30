# CustomFrontMatterRazorPageExample

A bare `AddPennington` host whose catch-all `@page` reads a custom front-matter record from a regular Razor page — no Mdazor component, no context dictionary. The direct way to consume a custom front-matter type you have defined.

## Concepts

- `ApiFrontMatter` is a custom `record : IFrontMatter` (plus capability interfaces) adding `namespace` and `stability` keys.
- `AddMarkdownContent<ApiFrontMatter>()` registers it inside the `AddPennington` lambda so pages under `Content/symbols` deserialize into the custom type.
- `Components/Pages/SymbolPage.razor` is the `@page "/{*Path}"` catch-all. It calls `IPageResolver.ResolveAsync<ApiFrontMatter>(url)`, which resolves the page and hands back its front matter already typed as `ApiFrontMatter` — `Stability` and `Namespace` are plain property reads.
- `ResolveAsync<TFrontMatter>` returns `null` when nothing matches or the page is served by a source registered against a different front-matter type.
- `app.UsePennington()` runs **before** `app.MapRazorComponents<App>()` so the catch-all doesn't swallow Pennington's redirect / sitemap / llms.txt routes.

## Referenced from

- `docs/.../how-to/pages/front-matter.md`
