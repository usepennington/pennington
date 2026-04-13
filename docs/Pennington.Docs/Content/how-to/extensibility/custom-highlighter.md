---
title: Add a custom syntax highlighter
description: Implement `ICodeHighlighter`, declare `SupportedLanguages`, set priority, and register via `HighlightingOptions.AddHighlighter`.
section: extensibility
order: 30
tags: []
uid: how-to.extensibility.custom-highlighter
isDraft: true
search: false
llms: false
---

> **In this page.** Implement `ICodeHighlighter`, declare `SupportedLanguages`, set priority, and register via `HighlightingOptions.AddHighlighter`.
>
> **Not in this page.** Authoring TextMate grammars from scratch — see the upstream TextMateSharp docs.

## When to use this

- You have a domain-specific language (DSL), a configuration dialect, or a token scheme that the built-in `TextMateHighlighter` cannot satisfy.
- You want to override highlighting for a language already handled by a built-in (e.g., replace `TextMateHighlighter` output for `bash` the way `ShellHighlighter` does).
- You need deterministic, dependency-free highlighting (regex-driven) for a small fenced language.

## Assumptions

- You have an existing Pennington site wired with `AddPennington(...)` and `UsePennington()` (any template: docs, blog, or bare).
- You know which language identifier(s) the fenced code blocks declare (e.g., ```` ```pipeline ````).
- You are comfortable emitting `<pre><code>...</code></pre>` with `hljs-*` class spans.

To copy a working setup, see [`examples/ForgePortalExample`](https://github.com/scottsauber/Penn/tree/main/examples/ForgePortalExample) — a `PipelineHighlighter` registered for the `pipeline`/`pipe` fences.

---

## Steps

### 1. Implement `ICodeHighlighter`

- Create a class implementing `Pennington.Highlighting.ICodeHighlighter`.
- Populate `SupportedLanguages` with the fenced language identifiers you want to claim (e.g., `"pipeline"`, `"pipe"`).
- Return highlighted HTML from `Highlight(string code, string language)` — wrap in `<pre><code>...</code></pre>` and emit `<span class="hljs-*">` tokens so the site stylesheet picks them up.

```csharp:xmldocid
T:ForgePortalExample.PipelineHighlighter
```

### 2. Declare the claimed languages

- `SupportedLanguages` is an `IReadOnlySet<string>`; use a `HashSet<string>` literal populated in the field initializer.
- Use `"*"` only when the highlighter is a universal fallback — `TextMateHighlighter` and `PlainTextHighlighter` already claim `"*"`.
- Match the casing of the fence identifier as authors will write it; `HighlightingService` compares with `IReadOnlySet<string>.Contains`.

```csharp:xmldocid
P:ForgePortalExample.PipelineHighlighter.SupportedLanguages
```

### 3. Set `Priority` above any competing highlighter

- `HighlightingService` orders highlighters by `Priority` descending and picks the first whose `SupportedLanguages` contains the requested language (or `"*"`).
- Reference priorities of the built-ins: `ShellHighlighter` = 75, `TextMateHighlighter` = 50, `PlainTextHighlighter` = 0.
- Pick a value that slots your highlighter correctly; e.g., `60` to sit between TextMate and Shell.

```csharp:xmldocid
P:ForgePortalExample.PipelineHighlighter.Priority
```

### 4. Register via `HighlightingOptions.AddHighlighter`

- Inside `AddPennington(penn => { ... })`, call `penn.Highlighting.AddHighlighter<T>()` for parameterless types, or `AddHighlighter(instance)` to pass a pre-built instance (e.g., one that takes DI dependencies).
- The type/instance lands in `HighlightingOptions.Highlighters`, which `HighlightingService` composes with the built-ins at construction.

```csharp
services.AddPennington(penn =>
{
    penn.Highlighting.AddHighlighter<PipelineHighlighter>();
});
```

### 5. Verify the fence resolves to your highlighter

- Author a fenced block in any Markdown content file using the language identifier you claimed.
- Rebuild and view the rendered page.

````markdown
```pipeline
source http://example.com
transform select .body
```
````

---

## Verify

- Run `dotnet run` and open a page whose markdown contains a fenced block tagged with your language identifier.
- View source: the `<pre><code>` block should contain your `<span class="hljs-*">` tokens, not the TextMate default output or HTML-encoded plain text.
- Swap the fence's language identifier to one you did **not** claim and confirm that `TextMateHighlighter` (or `PlainTextHighlighter`) takes over — your `Priority` only wins within `SupportedLanguages`.

## Related

- Reference: [`Pennington.Highlighting`](xref:reference.namespaces.highlighting) — `ICodeHighlighter`, `HighlightingService`, built-ins.
- Reference: [`HighlightingOptions`](xref:reference.options.highlighting) — `AddHighlighter<T>()` / `AddHighlighter(instance)`.
- Background: [How Pennington dispatches highlighters](xref:explanation.highlighting-dispatch) — priority ordering and the `"*"` fallback.
