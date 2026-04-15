---
title: "Embed focused code samples"
description: "Scope xmldocid fences to one member, strip declaration noise with bodyonly, and refactor long methods into named helpers so each step of a walkthrough shows one idea."
uid: how-to.content-authoring.focused-code-samples
order: 201130
sectionLabel: Content Authoring
tags: [authoring, xmldocid, code-samples, roslyn]
---

When a walkthrough step discusses a single member, the matching code fence should show only that member — not the whole enclosing type with its xmldoc and every sibling property. The xmldocid preprocessor supports the narrower forms directly; the trick is knowing which form applies where. This page covers the four moves that take a 100-line fence down to a handful of lines: fence one member, strip declarations with `,bodyonly`, split long methods into named helpers, and compare versions with `xmldocid-diff`. For the fence grammar itself, see <xref:reference.markdown.code-block-args>.

## Assumptions

- An existing Pennington site (see <xref:tutorials.getting-started.first-site> if not), with `Pennington.Roslyn` wired through `AddPenningtonRoslyn` and `SolutionPath` pointing at the solution that owns the source to fence.
- Comfort authoring markdown code fences — the techniques on this page are all info-string changes on a `csharp:xmldocid` fence.

For a working setup, see [`examples/FocusedCodeSamplesExample`](https://github.com/usepennington/pennington/tree/main/examples/FocusedCodeSamplesExample). `MonolithWordCounter` carries one long `CountWords` method; `ModularWordCounter` splits the same logic into `Tokenize`, `Tally`, and `Format`. Both are referenced by the fences below.

---

## Steps

<Steps>
<Step StepNumber="1">

**Fence one member, not the whole type**

`T:Type` pulls the full class declaration with xmldoc and every member. When the surrounding prose is about one method, reach for `M:Type.Method(...)` (or `P:` / `F:` / `E:`) instead — the fence shrinks to the member that matters.

Before, with the whole class body landing on a page that only discusses `CountWords`:

````markdown
```csharp:xmldocid
T:FocusedCodeSamplesExample.MonolithWordCounter
```
````

After, scoped to the method the prose is about:

```csharp:xmldocid
M:FocusedCodeSamplesExample.MonolithWordCounter.CountWords(System.String,System.Int32)
```

The xmldocid grammar for each member kind — `T:`, `M:`, `P:`, `F:`, `E:` — is listed in <xref:reference.markdown.code-block-args>.

</Step>
<Step StepNumber="2">

**Strip declaration noise with `,bodyonly`**

Even a member-scoped `M:` fence still carries the leading `/// <summary>` xmldoc and the method signature. When the prose has already named the method and summarized what it does, both are redundant. Appending `,bodyonly` renders only the body between the braces.

The same fence as step 1, now with `,bodyonly` dropping the xmldoc header and signature:

```csharp:xmldocid,bodyonly
M:FocusedCodeSamplesExample.MonolithWordCounter.CountWords(System.String,System.Int32)
```

`,bodyonly` also works on types (members between the braces, skipping the class header) and properties (the `get`/`set` accessors without the leading xmldoc).

</Step>
<Step StepNumber="3">

**Break a long method into named helpers**

When the target method is genuinely 25+ lines of distinct phases, no fence form will make it short and still intelligible — the source is too large. The fix is in the source, not the fence: extract each phase into a named helper with its own xmldoc summary, then fence the helpers one step at a time.

`ModularWordCounter` is the same logic as `MonolithWordCounter` split into three helpers. The orchestrator reads top-to-bottom as the outline for the walkthrough:

```csharp:xmldocid,bodyonly
M:FocusedCodeSamplesExample.ModularWordCounter.CountWords(System.String,System.Int32)
```

Each helper fences at roughly ten lines, so the walkthrough can spend one step on each:

```csharp:xmldocid,bodyonly
M:FocusedCodeSamplesExample.ModularWordCounter.Tokenize(System.String)
```

```csharp:xmldocid,bodyonly
M:FocusedCodeSamplesExample.ModularWordCounter.Tally(System.Collections.Generic.List{System.String},System.Int32)
```

```csharp:xmldocid,bodyonly
M:FocusedCodeSamplesExample.ModularWordCounter.Format(System.Collections.Generic.List{System.Collections.Generic.KeyValuePair{System.String,System.Int32}})
```

Helpers being `public` is part of the trade — `internal` methods do not surface xmldoc and do not participate in the symbol table the preprocessor walks. For doc-facing fixtures, `public` is the right visibility even when idiomatic application code would keep them internal.

</Step>
<Step StepNumber="4">

**Show a delta with `xmldocid-diff`**

When the article's point is that one version replaces another — a refactor, a migration, a before/after — fence both versions together with `xmldocid-diff`. The preprocessor renders a diff so the reader sees what changed without reading both fences independently and comparing by eye.

```csharp:xmldocid-diff,bodyonly
M:FocusedCodeSamplesExample.MonolithWordCounter.CountWords(System.String,System.Int32)
M:FocusedCodeSamplesExample.ModularWordCounter.CountWords(System.String,System.Int32)
```

The fence body must hold exactly two xmldocids, one per line, in before → after order. `,bodyonly` applies to both sides, so the diff compares implementations without the xmldoc boilerplate drowning out the change.

</Step>
<Step StepNumber="5">

**Reach for `:path` only when no xmldocid exists**

Four shapes have no C# symbol for the preprocessor to target: top-level-statement `Program.cs` files, `.razor` components, markdown or YAML fixtures, and JSON / TOML / config files. For those, `<lang>:path` embeds the whole file by path relative to the solution directory.

```markdown
```csharp:path
examples/FocusedCodeSamplesExample/Program.cs
```
```

For anything with a namespace and a type, prefer `:xmldocid` — it survives renames and line shifts that would silently break a `:path` fence, because the build fails noisily when a referenced xmldocid no longer resolves.

</Step>
</Steps>

---

## Verify

- Rebuild the site with `dotnet run --project docs/Pennington.Docs -- build` and reload the page — each fence renders at the scope its info string declares, with no carry-over of enclosing-type xmldoc.
- Grep `output/**/*.html` for `<pre>` elements taller than 25 lines — those are candidates for a `,bodyonly` or member-scoped follow-up pass.
- Rename `Tokenize` to `Split` in `examples/FocusedCodeSamplesExample/ModularWordCounter.cs` and rebuild — the build report surfaces an unresolved `M:…Tokenize(…)` rather than silently rendering nothing. That failure mode is the feedback loop that keeps fences honest as source moves.

## Related

- Reference: [Markdown extensions catalog](xref:reference.markdown.extensions) — the full fence grammar including `xmldocid`, `xmldocid,bodyonly`, `xmldocid-diff`, and `path`.
- Reference: [Code-block argument reference](xref:reference.markdown.code-block-args) — info-string parser details and the full list of suffix forms.
- How-to: [Annotate code blocks](xref:how-to.content-authoring.code-annotations) — per-line `[!code highlight]` / `[!code ++]` directives that compose with the fence forms on this page.
- Reference: [RoslynOptions](xref:reference.options.roslyn-options) — the `SolutionPath` setting that lets the preprocessor resolve `T:` / `M:` / `P:` targets.
