---
title: "Render a Razor component as a page on a bare host"
description: "Use HtmlRenderer.RenderComponentAsync inside a MapGet to make a Razor component the entire response body, no DocSite layout pipeline required."
uid: how-to.response-pipeline.razor-page-on-bare-host
order: 4
sectionLabel: "Response Pipeline"
tags: [extensibility, razor-components, bare-host, html-renderer]
---

To render a Razor component as the whole response body for a custom route on a bare `AddPennington` host, render it through Blazor's server-side `HtmlRenderer` from inside a `MapGet`. The component owns the document — `<html>`, `<head>`, `<body>` — so the response is a complete HTML page without DocSite or BlogSite layout machinery. Use this pattern when a custom `IContentService` discovers per-record routes (`/instructors/{slug}/`, `/status/{slug}/`) and the rendered output is too complex for inline HTML strings.

## Before you begin

- A working Pennington site on bare `AddPennington` (see <xref:tutorials.getting-started.first-site> if not).
- A reference to `Microsoft.AspNetCore.Components.Web` — already transitive through `Pennington`.
- Familiarity with `IContentService` for publishing the routes you'll render against (<xref:how-to.content-services.custom-content-service>).

A working reference: `examples/BareHostRazorPageExample` — one Razor component plus a single `MapGet` that renders it.

## Author the page component

Write a Razor component whose `[Parameter]` properties are everything the page needs — there is no ambient `HttpContext`, layout, or cascading state from a parent. The component renders the entire document so it includes `<!DOCTYPE html>` and the `<link rel="stylesheet" href="/styles.css">` tag for [MonorailCSS](https://monorailcss.github.io/MonorailCss.Framework/) output.

```razor:symbol
examples/BareHostRazorPageExample/Components/StatusPage.razor
```

## Register the Blazor renderer services

`HtmlRenderer` needs Blazor's component services and an `IHttpContextAccessor` so cascading values can resolve. Register both alongside the `AddPennington` and `AddMonorailCss` hosts:

```csharp
builder.Services.AddRazorComponents();
builder.Services.AddHttpContextAccessor();
```

`AddRazorComponents` registers `HtmlRenderer` and its dispatcher; `AddHttpContextAccessor` lets a rendered component resolve cascading values. There is no `MapRazorComponents`, no `App.razor`, and no `_Host` page — the bare host never starts the Blazor router. Components reach the response only through the `MapGet` below.

## Render the component inside a `MapGet`

The route handler turns a slug into the component's `[Parameter]` values and hands them to a render helper. A missing record returns null parameters, which the helper turns into a 404:

```csharp:symbol
examples/BareHostRazorPageExample/Program.cs > BareHostRenderer.RenderRazorPageAsync
```

`RenderRazorPageAsync<TComponent>` is the only Blazor-specific code the host needs: it dispatches the render onto the renderer's dispatcher, materializes the output with `ToHtmlString`, and hands the complete HTML string to `Results.Content`. Reuse it for any other component-as-page route. The route wiring itself is a plain minimal-API endpoint:

```csharp
app.MapGet("/status/{slug}/", (string slug, StatusPagesContentService statuses, HtmlRenderer renderer)
    => BareHostRenderer.RenderRazorPageAsync<StatusPage>(renderer, statuses.TryGet(slug) is { } entry
        ? new Dictionary<string, object?>
        {
            [nameof(StatusPage.Slug)] = entry.Slug,
            [nameof(StatusPage.Title)] = entry.Title,
            [nameof(StatusPage.Summary)] = entry.Summary,
            [nameof(StatusPage.Facts)] = entry.Facts,
        }
        : null));
```

### Why not a Blazor `@page`?

A routed `@page` component needs the Blazor router, an `App.razor`, and `MapRazorComponents` — the machinery [Serve markdown through Blazor Pages](xref:tutorials.getting-started.first-page) stands up. A bare `AddPennington` host runs none of that, so a `@page` directive would never be routed. Rendering through `HtmlRenderer` inside a `MapGet` keeps the host minimal: the component is a render target, not a routed endpoint, and your `IContentService` owns route discovery.

## Publish the routes through `IContentService`

A custom `IContentService` yields one `EndpointSource` per route so the build crawler discovers each URL and fetches it through the live pipeline — your `MapGet` produces the HTML the same way at build time as at request time. See <xref:how-to.content-services.custom-content-service> for the per-record discovery pattern, including a worked `EndpointSource` example.

## Verify

- Run `dotnet run --project examples/BareHostRazorPageExample` and open `/status/intro/` and `/status/verify/` at the URL the console prints (the `Now listening on:` line). Each renders the `StatusPage` component as a full HTML page styled by `/styles.css`.
- Confirm the static build picks up both routes: `dotnet run --project examples/BareHostRazorPageExample -- build` writes `output/status/intro/index.html` and `output/status/verify/index.html`.

## Related

- How-to: [Source content from outside the file system](xref:how-to.content-services.custom-content-service)
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)
