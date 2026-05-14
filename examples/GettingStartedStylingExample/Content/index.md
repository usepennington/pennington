---
title: Welcome
description: The home page of a styled three-page Pennington site.
---

# Welcome to the styled site

This is the home page. It lives at `Content/index.md`, so Pennington maps it to
the site root `/`. The same three-page shape as the previous tutorial picks up
a MonorailCSS stylesheet and a handful of utility classes.

Open the page source and you will see a `<link rel="stylesheet" href="/styles.css">`
tag. That endpoint is served by `UseMonorailCss` and regenerates whenever a new
utility class appears in rendered HTML.

## Brand palette vs. syntax theme

Brand colors (the `primary`, `accent`, and `base` utility prefixes used in the
layout) come from `NamedColorScheme`. Syntax-highlight colors are a separate
concern — `SyntaxTheme` paints code fences without touching the brand palette.
The fence below renders with the syntax theme; the surrounding chrome with the
brand palette:

```csharp
public static class Greeter
{
    public static string Hello(string name)
        => $"Hello, {name}! The brand and syntax palettes ship independently.";
}
```

Look at the rendered tokens (`public`, `static`, `class`, the string interpolation):
those colors belong to `SyntaxTheme`. The page background, headings, and links
belong to `NamedColorScheme`. Swap one without affecting the other.
