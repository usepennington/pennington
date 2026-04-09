---
title: "Advanced Topics"
description: "Custom island renderers and performance"
order: 40
---

## Custom Island Renderers

Each island has a renderer that fetches data and produces HTML. Renderers extend `RazorIslandRenderer<T>`:

```csharp
public class ArticleIslandRenderer(
    ContentHelper contentHelper,
    ComponentRenderer renderer)
    : RazorIslandRenderer<ArticleContent>(renderer)
{
    public override string IslandName => "article";

    protected override async Task<IDictionary<string, object?>?>
        BuildParametersAsync(ContentRoute route)
    {
        var result = await contentHelper.GetPageByUrlAsync(
            route.CanonicalPath.Value);
        if (result is null) return null;

        return new Dictionary<string, object?>
        {
            ["Title"] = result.Value.FrontMatter.Title,
            ["HtmlContent"] = result.Value.Html,
        };
    }
}
```

## Performance Benefits

SPA navigation payloads (`/_spa-data/*.json`) are typically ~2KB compared to ~15KB for full HTML pages. This means faster navigation and less bandwidth usage.
