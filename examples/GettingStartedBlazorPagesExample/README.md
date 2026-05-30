# GettingStartedBlazorPagesExample

A Pennington host whose markdown is served through a Blazor Server `@page` catch-all. The natural shape for a real Pennington app: same content pipeline as the minimal-site tutorial, but routed through a Blazor router with `App.razor` owning the document shell.

## Concepts

- `AddPennington()` + `AddMarkdownContent<DocFrontMatter>()` register the content pipeline (same as the minimal-site tutorial).
- `AddRazorComponents()` registers Blazor's static server-side rendering.
- `app.UsePennington()` runs **before** `app.MapRazorComponents<App>()` so the catch-all `@page "/{*Path}"` doesn't swallow Pennington's redirect / sitemap / llms.txt routes.
- `app.UseAntiforgery()` is required because Blazor's routed components opt into antiforgery metadata.
- `Components/App.razor` owns the entire document — no MainLayout. The styling tutorial introduces a layout.
- `Components/Pages/MarkdownPage.razor` is the `@page "/{*Path}"` catch-all. It resolves the URL through `IPageResolver.ResolveAsync` and renders the result via `(MarkupString)`.
- The file-path-to-URL convention from the markdown pipeline is unchanged: drop a new `.md` file under `Content/` and the catch-all serves it.

## Referenced from

- `docs/.../tutorials/getting-started/first-page.md`
