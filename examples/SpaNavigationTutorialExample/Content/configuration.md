---
title: "Configuration"
description: "Setting up SPA navigation"
order: 20
---

## Registering SPA Navigation

Add SPA navigation in your `Program.cs`:

```csharp
builder.Services.AddPennington(penn =>
{
    // ... other configuration ...
    penn.Islands.Register<ArticleIslandRenderer>("article");
    penn.Islands.Register<NavIslandRenderer>("nav");
});

builder.Services.AddSpaNavigation();
app.UseSpaNavigation();
```

## Island Attributes

Mark regions in your layout with `data-spa-island`:

- `data-spa-island="name"` — identifies the island
- `data-spa-loading="keep"` — don't touch during navigation (sidebar)
- `data-spa-loading="skeleton"` — show placeholder while loading
- `data-spa-loading="clear"` — empty the region while loading
