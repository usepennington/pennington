---
title: "Embed focused code samples"
description: "Scope xmldocid fences to one member, strip declaration noise with bodyonly, and refactor long methods into named helpers so each section of a walkthrough shows one idea."
uid: how-to.code-samples.focused-code-samples
order: 202030
sectionLabel: "Code Samples"
tags: [authoring, xmldocid, code-samples, roslyn]
---

To limit a code fence to the one member a walkthrough discusses — rather than dumping the whole enclosing type with its xmldoc and every sibling property — use the xmldocid preprocessor's member-scoped forms. The recipes below scope a fence to a member, strip declaration noise, copy-paste the surrounding `using` directives, and diff two implementations. Prefer `:xmldocid` over `:path` wherever the source has a C# symbol — `:xmldocid` survives renames and line shifts that silently break `:path` fences. For the fence grammar itself, see <xref:reference.markdown.code-block-args>.

## Before you begin
- An existing Pennington site (see <xref:tutorials.getting-started.first-site> if not), with `Pennington.Roslyn` wired through `AddPenningtonRoslyn` and `SolutionPath` pointing at the solution that owns the source to fence.
- Comfort authoring markdown code fences — the techniques on this page are all info-string changes on a `csharp:xmldocid` fence.

For a working setup, see [`examples/FocusedCodeSamplesExample`](https://github.com/usepennington/pennington/tree/main/examples/FocusedCodeSamplesExample). `MonolithWordCounter` carries one long `CountWords` method; `ModularWordCounter` splits the same logic into `Tokenize`, `Tally`, and `Format`. Both are referenced by the fences below.

---

## Fence one member, not the whole type

When the surrounding prose is about one method, reach for `M:Type.Method(...)` instead of `T:Type`. Member-scoped forms (`M:` for methods, `P:` for properties, `F:` for fields, `E:` for events) shrink the fence to the member the reader cares about. A `T:` fence pulls the full class declaration, its xmldoc, and every sibling member.

The wide form, which lands on a page that only discusses `CountWords`:

````markdown
```csharp:xmldocid
T:FocusedCodeSamplesExample.MonolithWordCounter
```
````

The narrow form, scoped to the method under discussion:

````markdown
```csharp:xmldocid
M:FocusedCodeSamplesExample.MonolithWordCounter.CountWords(System.String,System.Int32)
```
````

Which renders as:

```csharp:symbol
examples/FocusedCodeSamplesExample/MonolithWordCounter.cs > MonolithWordCounter.CountWords
```

The xmldocid grammar for each member kind — `T:`, `M:`, `P:`, `F:`, `E:` — is listed in <xref:reference.markdown.code-block-args>.

## Strip declaration noise with `,bodyonly`

Even a member-scoped `M:` fence still carries the leading `/// <summary>` xmldoc and the method signature. When the prose has already named the method and summarized what it does, both are redundant. Appending `,bodyonly` renders only the body between the braces.

````markdown
```csharp:xmldocid,bodyonly
M:FocusedCodeSamplesExample.MonolithWordCounter.CountWords(System.String,System.Int32)
```
````

Which renders as:

```csharp:symbol,bodyonly
examples/FocusedCodeSamplesExample/MonolithWordCounter.cs > MonolithWordCounter.CountWords
```

`,bodyonly` also works on types (members between the braces, skipping the class header) and properties (the `get`/`set` accessors without the leading xmldoc).

## Make the snippet copy-pasteable with `,usings`

A `,bodyonly` fence shows the body, but a reader copying it into a fresh file still has to guess which `using` directives the body needs. Append `,usings` to prepend the file-local `using` directives the fragment actually references. Only the directives whose namespaces or aliases appear in the body are emitted — the rest of the file's using block is suppressed.

````markdown
```csharp:xmldocid,bodyonly,usings
M:FocusedCodeSamplesExample.MonolithWordCounter.CountWords(System.String,System.Int32)
```
````

Which renders as:

```csharp:symbol,bodyonly
examples/FocusedCodeSamplesExample/MonolithWordCounter.cs > MonolithWordCounter.CountWords
```

The body uses `StringBuilder`, so `using System.Text;` lands above the snippet. `List<>`, `Dictionary<>`, and the `OrderByDescending` chain resolve through implicit/global usings — those are skipped by design, on the assumption that a reader who has `<ImplicitUsings>enable</ImplicitUsings>` already has them.

`,usings` and `,bodyonly` compose in any order, and the flag works on full-declaration fences too. For multi-symbol fences (one XmlDocId per line), the required usings are unioned and rendered in a single block at the top.

## Walk a multi-phase method through named helpers

When the target method runs 25+ lines across distinct phases, fence each phase as its own helper instead of fencing the monolith. `ModularWordCounter` is the same logic as `MonolithWordCounter` split into three helpers — `Tokenize`, `Tally`, and `Format` — orchestrated by a short `CountWords`. A `T:` fence on the whole class gives the reader the full picture in one place:

````markdown
```csharp:xmldocid
T:FocusedCodeSamplesExample.ModularWordCounter
```
````

Which renders as:

```csharp:symbol
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter
```

In a walkthrough, fence each helper separately so each section carries one idea:

````markdown
```csharp:xmldocid,bodyonly
M:FocusedCodeSamplesExample.ModularWordCounter.CountWords(System.String,System.Int32)
```

```csharp:xmldocid,bodyonly
M:FocusedCodeSamplesExample.ModularWordCounter.Tokenize(System.String)
```

```csharp:xmldocid,bodyonly
M:FocusedCodeSamplesExample.ModularWordCounter.Tally(System.Collections.Generic.List{System.String},System.Int32)
```

```csharp:xmldocid,bodyonly
M:FocusedCodeSamplesExample.ModularWordCounter.Format(System.Collections.Generic.List{System.Collections.Generic.KeyValuePair{System.String,System.Int32}})
```
````

The orchestrator renders as a three-liner that reads top-to-bottom as the outline for the walkthrough:

```csharp:symbol,bodyonly
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter.CountWords
```

`Tokenize`:

```csharp:symbol,bodyonly
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter.Tokenize
```

`Tally`:

```csharp:symbol,bodyonly
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter.Tally
```

`Format`:

```csharp:symbol,bodyonly
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter.Format
```

Keep the helpers `public` — `internal` methods do not surface xmldoc and do not participate in the symbol table the preprocessor walks.

## Show a delta with `xmldocid-diff`

When the article's point is that one version replaces another — a small refactor, a migration, a perf tweak — fence both versions with `xmldocid-diff`. The preprocessor emits a unified diff so the reader sees the two or three lines that moved rather than comparing two fences by eye. The form works best when the delta is small; whole-method rewrites render every line as changed and bury the point.

`ModularWordCounter.FormatV2` is deliberately a one-change variant of `Format`. It rents its `StringBuilder` from a pool instead of constructing a fresh one, and returns the builder at the end. Everything else is identical, so the diff collapses to those lines.

````markdown
```csharp:xmldocid-diff,bodyonly
M:FocusedCodeSamplesExample.ModularWordCounter.Format(System.Collections.Generic.List{System.Collections.Generic.KeyValuePair{System.String,System.Int32}})
M:FocusedCodeSamplesExample.ModularWordCounter.FormatV2(System.Collections.Generic.List{System.Collections.Generic.KeyValuePair{System.String,System.Int32}})
```
````

Which renders as:

```csharp:symbol-diff,bodyonly
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter.Format
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter.FormatV2
```

The fence body must hold exactly two xmldocids, one per line, in before → after order. `,bodyonly` applies to both sides, so the diff compares implementations without xmldoc boilerplate drowning out the change.

## Embed files without a C# symbol via `:path`

Top-level-statement `Program.cs` files, `.razor` components, markdown or YAML fixtures, and JSON / TOML / config files have no symbol for `:xmldocid` to target. `<lang>:path` embeds the whole file by path relative to the solution directory:

````markdown
```csharp:path
examples/FocusedCodeSamplesExample/Program.cs
```
````

---

## Verify

- Rebuild the site with `dotnet run --project docs/Pennington.Docs -- build` and reload the page — each fence renders at the scope its info string declares, with no carry-over of enclosing-type xmldoc.
- Grep `output/**/*.html` for `<pre>` elements taller than 25 lines — those are candidates for a `,bodyonly` or member-scoped follow-up pass.
- Rename `Tokenize` to `Split` in `examples/FocusedCodeSamplesExample/ModularWordCounter.cs` and rebuild — the build report surfaces an unresolved `M:…Tokenize(…)` rather than silently rendering nothing.

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the full fence grammar including `xmldocid`, `xmldocid,bodyonly`, `xmldocid-diff`, and `path`.
- Reference: [Code-block argument reference](xref:reference.markdown.code-block-args) — info-string parser details and the full list of suffix forms.
- How-to: [Annotate code blocks](xref:how-to.code-samples.code-annotations) — per-line `[!code highlight]` / `[!code ++]` directives that compose with the fence forms on this page.
- Reference: [RoslynOptions](xref:reference.api.roslyn-options) — the `SolutionPath` setting that lets the preprocessor resolve `T:` / `M:` / `P:` targets.
