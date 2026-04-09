---
title: "Generating llms.txt for AI Access"
description: "Enable llms.txt generation so AI tools and LLMs can discover and consume site content in structured plain text"
uid: "penn.how-to.generating-llms-txt"
order: 30
---

You want AI tools and LLMs to discover and read your documentation in a structured format. Pennington generates llms.txt index files and optionally a concatenated full-content file.

## Beat 1: Enable and configure llms.txt

How to enable llms.txt generation and configure its options.

### What to show
- Show calling `M:Pennington.Infrastructure.PenningtonOptions.AddLlmsTxt(System.Action{Pennington.LlmsTxt.LlmsTxtOptions})` in `Program.cs` inside the `AddPennington` callback. Demonstrate both the simple call (`penn.AddLlmsTxt()`) and with configuration (`penn.AddLlmsTxt(opts => opts.GenerateFullFile = true)`). Note: DocSite calls `AddLlmsTxt()` automatically inside `AddDocSite`. BlogSite does not -- BlogSite users must call `AddPennington` separately to add llms.txt support.
- Show the `T:Pennington.LlmsTxt.LlmsTxtOptions` class: `P:Pennington.LlmsTxt.LlmsTxtOptions.OutputDirectory` (default `"_llms"`, the directory where individual markdown files are placed) and `P:Pennington.LlmsTxt.LlmsTxtOptions.GenerateFullFile` (boolean, whether to also generate `llms-full.txt` with all content concatenated).

### Key points
- `AddLlmsTxt()` registers the llms.txt services -- without calling it, no llms.txt endpoints or files are generated
- `P:Pennington.LlmsTxt.LlmsTxtOptions.OutputDirectory` controls the URL prefix for individual markdown files (e.g., `_llms/guides/getting-started.md`)
- `P:Pennington.LlmsTxt.LlmsTxtOptions.GenerateFullFile` defaults to false -- enable it to produce a single concatenated file for LLMs that prefer one large document

## Beat 2: How llms.txt is generated

The internals of llms.txt generation: content querying, navigation tree building, index formatting, and optional full-content concatenation.

### What to show
- Walk through `T:Pennington.LlmsTxt.LlmsTxtService`: it queries all `T:Pennington.Content.IContentService` instances (skipping `T:Pennington.LlmsTxt.LlmsTxtContentService` to avoid circular queries), parses markdown sources, builds the navigation tree via `T:Pennington.Navigation.NavigationBuilder`, and generates the index. The index uses section headers (`## Section Name`) from the navigation tree and lists content items as `- [Title](path): description`.
- Show that individual markdown files are stripped of front matter and placed in the `_llms/` directory as raw markdown, and the optional `llms-full.txt` concatenates all content with `---` separators between pages.
- Show the endpoint mapping: `app.MapGet("/llms.txt", async (LlmsTxtService service) => Results.Content(await service.GetLlmsTxtAsync(), "text/plain"))`. Note that only `/llms.txt` has a dev-mode endpoint. The `llms-full.txt` file and individual `_llms/*.md` files are generated only during static build by `T:Pennington.LlmsTxt.LlmsTxtContentService` (which implements `T:Pennington.Content.IContentService` and produces files via `GetContentToCreateAsync`).
- Mention that the llms.txt header can be customized by placing a `llms.txt` file in the content root directory -- `LlmsTxtService` reads it via `ReadUserHeaderAsync` and uses it instead of the auto-generated header.

### Key points
- `LlmsTxtService` is registered via `services.AddFileWatched<LlmsTxtService>()` so it rebuilds when content changes
- `T:Pennington.LlmsTxt.LlmsTxtContentService` is registered as `IContentService` so that the static site builder discovers and outputs `llms.txt`, `llms-full.txt`, and the individual `_llms/*.md` files
- Content implementing `T:Pennington.FrontMatter.IRedirectable` with a non-null `RedirectUrl` is excluded from llms.txt
- `T:Pennington.FrontMatter.IDescribable.Description` populates the `: description` suffix on each entry in the index

## Beat 3: Verify llms.txt

How to confirm that llms.txt files are generated correctly during development and in the static build output.

### What to show
- During development with `dotnet run`: navigate to `/llms.txt` (LLM index). Inspect the response for correct content -- the index should list sections and content items with titles, paths, and descriptions. Note: `/llms-full.txt` and individual `_llms/*.md` files are only available in the static build output, not during dev mode.
- Run `dotnet run -- build` to generate the static site. Check the output directory for `llms.txt`, `llms-full.txt` (if `GenerateFullFile` is enabled), and the `_llms/` directory containing individual markdown files.
- Show how the static site builder discovers these files: `T:Pennington.LlmsTxt.LlmsTxtContentService` generates files via `M:Pennington.LlmsTxt.LlmsTxtContentService.GetContentToCreateAsync` which returns `ContentToCreate` items for `llms.txt`, individual markdown files, and optionally `llms-full.txt`.

### Key points
- llms.txt files are generated as static assets during build via the `IContentService` pipeline, not just as endpoint responses
- The `_llms/` directory mirrors the site's content structure, so individual markdown files retain their logical paths
- If no content items are found (all filtered out or no content services registered), llms.txt will contain only the header
