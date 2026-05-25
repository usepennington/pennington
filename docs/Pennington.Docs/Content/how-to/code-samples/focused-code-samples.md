---
title: "Embed focused code samples"
description: "Scope :symbol fences to one member, strip declaration noise with bodyonly, and refactor long methods into named helpers so each section of a walkthrough shows one idea."
uid: how-to.code-samples.focused-code-samples
order: 202030
sectionLabel: "Code Samples"
tags: [authoring, symbol, code-samples, tree-sitter]
---

To limit a code fence to the one member a walkthrough discusses — rather than dumping the whole enclosing file with every sibling member — use the `:symbol` preprocessor's member-scoped form. The recipes below scope a fence to a member, strip declaration noise, and diff two implementations. Address a member by its **name path** (`Type.Member`) rather than a hard-coded line range — a name path survives the line shifts that silently break a range. For the fence grammar itself, see <xref:reference.markdown.code-block-args>.

## Before you begin
- An existing Pennington site (see <xref:tutorials.getting-started.first-site> if not), with `Pennington.TreeSitter` wired through `AddPenningtonTreeSitter` and `ContentRoot` pointing at the root that holds the source to fence.
- Comfort authoring markdown code fences — the techniques on this page are all info-string changes on a `csharp:symbol` fence.

For a working setup, see [`examples/FocusedCodeSamplesExample`](https://github.com/usepennington/pennington/tree/main/examples/FocusedCodeSamplesExample). `MonolithWordCounter` carries one long `CountWords` method; `ModularWordCounter` splits the same logic into `Tokenize`, `Tally`, and `Format`. Both are referenced by the fences below.

---

## Fence one member, not the whole type

When the surrounding prose is about one method, reach for `Type.Method` instead of a bare `Type`. A member path shrinks the fence to the member the reader cares about; a `Type` reference (or a bare file path with no `>`) pulls the full type or file.

The wide form, which lands on a page that only discusses `CountWords`:

````markdown
```csharp:symbol
examples/FocusedCodeSamplesExample/MonolithWordCounter.cs > MonolithWordCounter
```
````

The narrow form, scoped to the method under discussion:

````markdown
```csharp:symbol
examples/FocusedCodeSamplesExample/MonolithWordCounter.cs > MonolithWordCounter.CountWords
```
````

Which renders as:

```csharp:symbol
examples/FocusedCodeSamplesExample/MonolithWordCounter.cs > MonolithWordCounter.CountWords
```

The name-path grammar — `Type`, `Type.Member`, nested `Type.Inner.Member` — is listed in <xref:reference.markdown.code-block-args>.

## Strip declaration noise with `,bodyonly`

Even a member-scoped fence still carries the signature and any leading doc comment. When the prose has already named the method and summarized what it does, both are redundant. Appending `,bodyonly` renders only the body between the braces.

````markdown
```csharp:symbol,bodyonly
examples/FocusedCodeSamplesExample/MonolithWordCounter.cs > MonolithWordCounter.CountWords
```
````

Which renders as:

```csharp:symbol,bodyonly
examples/FocusedCodeSamplesExample/MonolithWordCounter.cs > MonolithWordCounter.CountWords
```

`,bodyonly` also works on types (members between the braces, skipping the type header) and properties (the `get`/`set` accessors).

## Walk a multi-phase method through named helpers

When the target method runs 25+ lines across distinct phases, fence each phase as its own helper instead of fencing the monolith. `ModularWordCounter` is the same logic as `MonolithWordCounter` split into three helpers — `Tokenize`, `Tally`, and `Format` — orchestrated by a short `CountWords`. A whole-type fence gives the reader the full picture in one place:

````markdown
```csharp:symbol
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter
```
````

Which renders as:

```csharp:symbol
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter
```

In a walkthrough, fence each helper separately so each section carries one idea:

````markdown
```csharp:symbol,bodyonly
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter.CountWords
```

```csharp:symbol,bodyonly
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter.Tokenize
```

```csharp:symbol,bodyonly
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter.Tally
```

```csharp:symbol,bodyonly
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter.Format
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

`:symbol` resolves a member by name path within the file, so give each helper a distinct name — overloads resolve to the first declaration and can't be told apart.

## Show a delta with `symbol-diff`

When the article's point is that one version replaces another — a small refactor, a migration, a perf tweak — fence both versions with `symbol-diff`. The preprocessor emits a unified diff so the reader sees the two or three lines that moved rather than comparing two fences by eye. The form works best when the delta is small; whole-method rewrites render every line as changed and bury the point.

`ModularWordCounter.FormatV2` is deliberately a one-change variant of `Format`. It rents its `StringBuilder` from a pool instead of constructing a fresh one, and returns the builder at the end. Everything else is identical, so the diff collapses to those lines.

````markdown
```csharp:symbol-diff,bodyonly
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter.Format
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter.FormatV2
```
````

Which renders as:

```csharp:symbol-diff,bodyonly
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter.Format
examples/FocusedCodeSamplesExample/ModularWordCounter.cs > ModularWordCounter.FormatV2
```

The fence body must hold exactly two references, one per line, in before → after order. `,bodyonly` applies to both sides, so the diff compares implementations without declaration boilerplate drowning out the change.

## Embed a whole file with a bare path

Top-level-statement `Program.cs` files, `.razor` components, markdown or YAML fixtures, and JSON / TOML / config files have no member to scope to. A bare `<file>` reference with no `> member` embeds the entire file:

````markdown
```csharp:symbol
examples/FocusedCodeSamplesExample/Program.cs
```
````

---

## Verify

- Rebuild the site with `dotnet run --project docs/Pennington.Docs -- build` and reload the page — each fence renders at the scope its info string declares, with no carry-over of enclosing-type members.
- Grep `output/**/*.html` for `<pre>` elements taller than 25 lines — those are candidates for a `,bodyonly` or member-scoped follow-up pass.
- Rename `Tokenize` to `Split` in `examples/FocusedCodeSamplesExample/ModularWordCounter.cs` and rebuild — the build report surfaces an unresolved `ModularWordCounter.Tokenize` reference rather than silently rendering nothing.

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the full fence grammar including `symbol`, `symbol,bodyonly`, and `symbol-diff`.
- Reference: [Code-block argument reference](xref:reference.markdown.code-block-args) — info-string parser details and the full list of suffix forms.
- How-to: [Annotate code blocks](xref:how-to.code-samples.code-annotations) — per-line `[!code highlight]` / `[!code ++]` directives that compose with the fence forms on this page.
