---
title: Hook into the response pipeline
description: Intercept rendered HTML before it reaches the browser with IResponseProcessor.
sectionLabel: Advanced
order: 50
---

# Hook into the response pipeline

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

Response processors mutate the already-rendered page; islands let you hand
a region of the DOM off to a Razor component that rehydrates on the
client. Reach for a processor when the change is purely HTML; reach for an
island when you need interactive state.
