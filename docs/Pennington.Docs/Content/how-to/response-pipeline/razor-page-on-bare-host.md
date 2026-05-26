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

Write a Razor component whose `[Parameter]` surface is everything the page needs — there is no ambient `HttpContext`, layout, or cascading state from a parent. The component renders the entire document so it includes `<!DOCTYPE html>` and the `<link rel="stylesheet" href="/styles.css">` tag for MonorailCSS output.

```razor:symbol
examples/BareHostRazorPageExample/Components/StatusPage.razor
```

## Register the Blazor renderer services

`HtmlRenderer` needs Blazor's component services and an `IHttpContextAccessor` so cascading values can resolve. Register both alongside the Pennington and MonorailCSS hosts.

```csharp:symbol
examples/BareHostRazorPageExample/Program.cs
```

The `RenderRazorPageAsync<TComponent>` helper at the bottom of `Program.cs` is the only Blazor-specific code the host needs: it dispatches the render onto the renderer's dispatcher, materializes the output, and hands the HTML string to `Results.Content`. Reuse it for any other component-as-page route.

## Publish the routes through `IContentService`

A custom `IContentService` yields one `EndpointSource` per route so the build crawler discovers each URL and fetches it through the live pipeline — your `MapGet` produces the HTML the same way at build time as at request time. See <xref:how-to.content-services.custom-content-service> for the per-record discovery shape, including a worked `EndpointSource` example.

## Verify

- Run `dotnet run --project examples/BareHostRazorPageExample` and visit `http://localhost:5000/status/intro/` and `http://localhost:5000/status/verify/`. Each renders the `StatusPage` component as a full HTML page styled by `/styles.css`.
- Confirm the static build picks up both routes: `dotnet run --project examples/BareHostRazorPageExample -- build` writes `output/status/intro/index.html` and `output/status/verify/index.html`.

## Related

- How-to: [Source content from outside the file system](xref:how-to.content-services.custom-content-service)
- Background: [The content pipeline and union types](xref:explanation.core.content-pipeline)
