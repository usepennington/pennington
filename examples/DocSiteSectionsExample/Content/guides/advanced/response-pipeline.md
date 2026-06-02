---
title: Hook into the response pipeline
description: Intercept rendered HTML before it reaches the browser with IResponseProcessor.
sectionLabel: Advanced
order: 50
---

Every rendered page flows through a response pipeline before hitting the
wire. Register an `IResponseProcessor` to mutate the HTML — add a feedback
widget, rewrite anchor IDs, or inject analytics — without touching the
markdown.

## Register a processor

```csharp
builder.Services.AddSingleton<IResponseProcessor, MyProcessor>();
```

Processors run in `Order` ascending. Override `ShouldProcess` to scope the
work to particular routes, content types, or request metadata.

## Where this fits vs islands

Response processors mutate the already-rendered HTML server-side, before it
reaches the browser. Islands — the SPA engine's `data-spa-region` blocks —
mark which server-rendered regions swap in on in-site navigation; Pennington
renders them on the server with no client hydration. Reach for a processor to
change the HTML every page ships; for interactive client behavior, ship your
own client-side script that enhances the rendered HTML.
