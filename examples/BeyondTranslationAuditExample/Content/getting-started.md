---
title: Getting Started
description: Wire Pennington.TranslationAudit into your DocSite host.
order: 30
---

# Getting Started

There is no Spanish version of this page on disk. When the audit runs, it
records a `Missing` diagnostic for the `es` locale of `/getting-started/`. The
dev overlay shows it when you visit this page; the build report lists it after
`dotnet run -- build`.

To wire it into your own host:

```csharp
using Pennington.TranslationAudit;

builder.Services.AddDocSite(...);
builder.Services.AddPenningtonTranslationAudit();
```

That is the entire wiring. Repository path auto-discovers from cwd, severity
defaults to Warning, every non-default locale is reported.
