---
title: Generate an llms.txt
description: Enable `LlmsTxtOptions`, set `OutputDirectory` and `GenerateFullFile`, and opt pages out with `llms: false`.
section: configuration
order: 60
tags: []
uid: how-to.configuration.llms-txt
isDraft: true
search: false
llms: false
---

> **In this page.** Enabling `LlmsTxtOptions`, setting `OutputDirectory` and `GenerateFullFile`, and opting pages out with `llms: false`.
>
> **Not in this page.** The LLM-training implications of your output or MCP server generation.

## When to use this

- You want large-language-model agents to index your site via the `/llms.txt` convention plus per-page stripped-markdown sidecars.
- You need an `llms-full.txt` concatenation for single-file ingestion alongside the per-page files.
- You need specific pages (drafts, internal runbooks, template scaffolds) kept out of the llms.txt index and the `_llms/*.md` tree.

## Assumptions

- You have a working Pennington site (doc, blog, or custom) with `AddPennington` and `UsePennington` wired up.
- At least one `IContentService` (markdown or otherwise) is registered and surfaces a navigation tree.
- You understand that `LlmsTxtService` fetches post-pipeline HTML and converts it to markdown, so your Markdig extensions and rewriters run first.

---

## Steps

### 1. Enable `LlmsTxtOptions` in `AddPennington`

Call `penn.AddLlmsTxt()` to register `LlmsTxtService`, `LlmsTxtContentService`, and the `/llms.txt` endpoint. The `if (options.LlmsTxt is { } ...)` gate in `UsePennington` leaves everything off until you opt in.

```csharp
builder.Services.AddPennington(penn =>
{
    penn.AddLlmsTxt();
});
```

### 2. Override `OutputDirectory` for the stripped markdown

Per-page markdown sidecars are emitted under `OutputDirectory` (relative to site root, default `_llms`). Intra-site links in the converted markdown are rewritten to point at sibling files under this directory.

```csharp
builder.Services.AddPennington(penn =>
{
    penn.AddLlmsTxt(llms =>
    {
        llms.OutputDirectory = "_llms";
    });
});
```

### 3. Set `GenerateFullFile` to emit `llms-full.txt`

`GenerateFullFile = true` appends every converted page into a single concatenated `llms-full.txt` at the site root alongside `llms.txt`. Leave it `false` if you only want per-page files.

```csharp
builder.Services.AddPennington(penn =>
{
    penn.AddLlmsTxt(llms =>
    {
        llms.OutputDirectory = "_llms";
        llms.GenerateFullFile = true;
    });
});
```

### 4. Scope the converted body with `ContentSelector`

Set a CSS selector (for example `article` or `#main-content`) so only the main element is converted to markdown. When null the entire `<body>` is converted, which typically leaks navigation and footer content.

```csharp
builder.Services.AddPennington(penn =>
{
    penn.AddLlmsTxt(llms =>
    {
        llms.ContentSelector = "article";
    });
});
```

### 5. Opt a markdown page out with `llms: false`

`MarkdownContentService` reads `IFrontMatter.Llms` and sets `ContentTocItem.ExcludeFromLlms`. `LlmsTxtService` filters those entries before building the tree, so they appear neither in `llms.txt` nor under `OutputDirectory`.

```yaml
title: Internal runbook
llms: false
```

### 6. (Optional) Provide a custom `llms.txt` header

Drop an `llms.txt` file in the content root to replace the default `# {SiteTitle}` / description header; the rest of the tree is still auto-generated below your header.

```text
# My Site

> Short one-line summary used by agents.
```

---

## Verify

- Run `dotnet run` and fetch `/llms.txt`; confirm it lists every non-excluded page and uses your custom header if you wrote one.
- Run `dotnet run -- build` and confirm `llms.txt`, `_llms/**/*.md`, and (if enabled) `llms-full.txt` appear under the output directory.
- Open one `_llms/*.md` and confirm intra-site links have been rewritten to sibling `_llms/...md` paths.

## Related

- Reference: [`LlmsTxtOptions`](/reference/options/auxiliary-options)
- Reference: [`IFrontMatter` and capability defaults](/reference/front-matter/ifrontmatter)
