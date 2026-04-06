---
title: Getting Started
order: 2
---

Welcome aboard! Let's get you set up with the Multilingual Tavern in just a few steps.

## Prerequisites

Before you begin, make sure you have:

- .NET 10 SDK installed
- A text editor (any will do)
- A sense of adventure

## Step 1: Create Your Content

Create a `Content` folder in your project root. Add your default language markdown files here:

```
Content/
  index.md
  getting-started.md
  menu.md
```

## Step 2: Add Translations

For each additional language, create a subfolder named with the locale code:

```
Content/
  fr/
    index.md
    getting-started.md
```

## Step 3: Configure Localization

In your `Program.cs`, add the `Localization` option to your `DocSiteOptions`:

```csharp
builder.Services.AddDocSite(() => new DocSiteOptions
{
    SiteTitle = "My Site",
    Localization = new LocalizationOptions
    {
        DefaultLocale = "en",
        Locales = ImmutableDictionary<string, LocaleInfo>.Empty
            .Add("en", new LocaleInfo("English"))
            .Add("fr", new LocaleInfo("Fran\u00e7ais"))
    },
});
```

## Step 4: Run It

```bash
dotnet run
```

That's it! Your site now supports multiple languages with automatic navigation filtering, a language switcher, and fallback to the default language for untranslated pages.
