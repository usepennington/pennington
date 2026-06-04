# BeyondCookFormatExample

Registers **Cooklang** (`.cook`) recipe files as a first-class Pennington content format, served
alongside a markdown landing page on a single bare host. Recipes are discovered, parsed, and rendered
through the same `IPageResolver` pipeline that handles markdown — proving the multi-format dispatch
seam (`AddContentFormat`, a format-tagged `FileSource`, and the dispatching parser/renderer in core).

## Concepts

- **`AddContentFormat<CookFrontMatter>("cook", …)`** — registers a custom format: a content directory,
  a file glob (`*.cook`), a parser, and a renderer, keyed by the format string `"cook"`.
- **`CookFrontMatter`** — an `IFrontMatter` (+ `ITaggable`) front-matter type. Keys are camelCase
  (`prepTime`, `cookTime`, …) so they bind without attributes and pass the build's strict unknown-key check.
- **`CookContentParser : IContentParser`** — reads the file, splits front matter into `CookFrontMatter`,
  and yields the Cooklang body as the parsed body. The dispatcher stamps the `"cook"` format.
- **`CookContentRenderer : IContentRenderer`** — runs the body through the `CooklangSharp` package and
  emits a rudimentary semantic recipe page (title, meta, ingredient list, sectioned steps).

Markdown (`Content/index.md`) and cook (`recipes/*.cook`) coexist, demonstrating that the dispatcher
routes each URL to the parser/renderer for its format.

## Run

```
dotnet run --project examples/BeyondCookFormatExample
```

Then open `/` (markdown landing) and `/recipes/chicken-piccata/` (a Cooklang recipe). Build the static
site with `dotnet run --project examples/BeyondCookFormatExample -- build`.

## Docs

Backs `how-to/content-services/custom-content-format.md`, which embeds this example's code via
tree-sitter `:symbol` fences.
