---
title: "Embed focused code samples"
description: "Scope xmldocid fences to one member, strip declaration noise with bodyonly, and refactor long methods into named helpers so each section of a walkthrough shows one idea."
uid: how-to.content-authoring.focused-code-samples
order: 201130
sectionLabel: Content Authoring
tags: [authoring, xmldocid, code-samples, roslyn]
---

To limit a code fence to the one member a walkthrough discusses — rather than dumping the whole enclosing type with its xmldoc and every sibling property — use the xmldocid preprocessor's member-scoped forms. This page works through four techniques in order of reach: fence one member with `M:`, strip declaration noise with `,bodyonly`, refactor long methods into named helpers, and compare versions with `xmldocid-diff`. For the fence grammar itself, see <xref:reference.markdown.code-block-args>.

## Assumptions

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

```csharp:xmldocid
M:FocusedCodeSamplesExample.MonolithWordCounter.CountWords(System.String,System.Int32)
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

```csharp:xmldocid,bodyonly
M:FocusedCodeSamplesExample.MonolithWordCounter.CountWords(System.String,System.Int32)
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

```csharp:xmldocid,bodyonly,usings
M:FocusedCodeSamplesExample.MonolithWordCounter.CountWords(System.String,System.Int32)
```

The body uses `StringBuilder`, so `using System.Text;` lands above the snippet. `List<>`, `Dictionary<>`, and the `OrderByDescending` chain resolve through implicit/global usings — those are skipped by design, on the assumption that a reader who has `<ImplicitUsings>enable</ImplicitUsings>` already has them.

`,usings` and `,bodyonly` compose in any order, and the flag works on full-declaration fences too. For multi-symbol fences (one XmlDocId per line), the required usings are unioned and rendered in a single block at the top.

## Break a long method into named helpers

When the target method runs 25+ lines across distinct phases, no fence form will make it short and intelligible — the source itself is too large. Fix the source, not the fence: extract each phase into a named helper with its own xmldoc summary, then fence the helpers one at a time.

`ModularWordCounter` is the same logic as `MonolithWordCounter` split into three helpers — `Tokenize`, `Tally`, and `Format` — orchestrated by a short `CountWords`. A `T:` fence on the whole class gives the reader the full picture in one place:

````markdown
```csharp:xmldocid
T:FocusedCodeSamplesExample.ModularWordCounter
```
````

Which renders as:

```csharp:xmldocid
T:FocusedCodeSamplesExample.ModularWordCounter
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

```csharp:xmldocid,bodyonly
M:FocusedCodeSamplesExample.ModularWordCounter.CountWords(System.String,System.Int32)
```

`Tokenize`:

```csharp:xmldocid,bodyonly
M:FocusedCodeSamplesExample.ModularWordCounter.Tokenize(System.String)
```

`Tally`:

```csharp:xmldocid,bodyonly
M:FocusedCodeSamplesExample.ModularWordCounter.Tally(System.Collections.Generic.List{System.String},System.Int32)
```

`Format`:

```csharp:xmldocid,bodyonly
M:FocusedCodeSamplesExample.ModularWordCounter.Format(System.Collections.Generic.List{System.Collections.Generic.KeyValuePair{System.String,System.Int32}})
```

Keep the helpers `public`. `internal` methods do not surface xmldoc and do not participate in the symbol table the preprocessor walks, so fences against them fail to resolve. For doc-facing fixtures, `public` is the right visibility even when idiomatic application code would keep them internal.

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

```csharp:xmldocid-diff,bodyonly
M:FocusedCodeSamplesExample.ModularWordCounter.Format(System.Collections.Generic.List{System.Collections.Generic.KeyValuePair{System.String,System.Int32}})
M:FocusedCodeSamplesExample.ModularWordCounter.FormatV2(System.Collections.Generic.List{System.Collections.Generic.KeyValuePair{System.String,System.Int32}})
```

The fence body must hold exactly two xmldocids, one per line, in before → after order. `,bodyonly` applies to both sides, so the diff compares implementations without xmldoc boilerplate drowning out the change.

## Reach for `:path` only when no xmldocid exists

Four shapes have no C# symbol for the preprocessor to target: top-level-statement `Program.cs` files, `.razor` components, markdown or YAML fixtures, and JSON / TOML / config files. For those, `<lang>:path` embeds the whole file by path relative to the solution directory:

````markdown
```csharp:path
examples/FocusedCodeSamplesExample/Program.cs
```
````

For anything with a namespace and a type, prefer `:xmldocid`. The build fails noisily on an unresolved symbol, so it survives the renames and line shifts that silently break `:path` fences.

---

## Verify

- Rebuild the site with `dotnet run --project docs/Pennington.Docs -- build` and reload the page — each fence renders at the scope its info string declares, with no carry-over of enclosing-type xmldoc.
- Grep `output/**/*.html` for `<pre>` elements taller than 25 lines — those are candidates for a `,bodyonly` or member-scoped follow-up pass.
- Rename `Tokenize` to `Split` in `examples/FocusedCodeSamplesExample/ModularWordCounter.cs` and rebuild — the build report surfaces an unresolved `M:…Tokenize(…)` rather than silently rendering nothing.

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the full fence grammar including `xmldocid`, `xmldocid,bodyonly`, `xmldocid-diff`, and `path`.
- Reference: [Code-block argument reference](xref:reference.markdown.code-block-args) — info-string parser details and the full list of suffix forms.
- How-to: [Annotate code blocks](xref:how-to.content-authoring.code-annotations) — per-line `[!code highlight]` / `[!code ++]` directives that compose with the fence forms on this page.
- Reference: [RoslynOptions](xref:reference.api.roslyn-options) — the `SolutionPath` setting that lets the preprocessor resolve `T:` / `M:` / `P:` targets.
