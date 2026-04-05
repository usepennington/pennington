---
title: Settin' Sail
order: 2
---

Ahoy, new recruit! Let's get ye shipshape with the Multilingual Tavern in just a few steps, arrr!

## Before Ye Set Sail

Make sure ye got these provisions:

- .NET 10 SDK stowed aboard
- A text editor (any port in a storm)
- A thirst fer adventure on the high seas

## Step 1: Prepare Yer Cargo

Create a `Content` hold in yer project. Stow yer default language markdown files there like proper cargo.

## Step 2: Add Yer Translations

Fer each additional tongue, create a subfolder named with the locale code, like markin' treasure on a map:

```
Content/
  pi/
    index.md
    getting-started.md
```

## Step 3: Chart Yer Course

In yer `Program.cs`, add the `Localization` option to yer `DocSiteOptions`. 'Tis like settin' the coordinates fer yer voyage!

## Step 4: Weigh Anchor!

```bash
dotnet run
```

That be all there is to it, ye scurvy dog! Yer site now speaks many tongues across the seven seas! Arrr!
