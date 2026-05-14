# FocusedCodeSamplesExample

Console app — not a website. Two implementations of the same word-counter (`MonolithWordCounter` and `ModularWordCounter`) so the docs can fence small focused methods (`Tokenize`, `Tally`, `Format`) by xmldocid and prove that a build report surfaces unresolved IDs when a method is renamed.

## Concepts

- Pulling focused code samples by `csharp:xmldocid` / `csharp:xmldocid,bodyonly`. Each method's xmldoc summary names its role so a reader copying a fence into prose has the one-line description ready.
- Side-by-side "monolith vs modular" for narrating a refactor in docs. Both files carry class-level summaries explaining the variant's shape (`MonolithWordCounter` notes "parse, tally, and format phases all live inline inside one long method"; `ModularWordCounter` notes "parse, tally, and format phases are named public helpers rather than inline blocks"), so the file alone teaches the contrast.
- `ModularWordCounter.FormatV2` plus `StringBuilderPool` exists as a "small focused delta" target for `csharp:xmldocid-diff,bodyonly` fences — pair `Format` vs `FormatV2` to render exactly the StringBuilder-pool change.
- The build report's unresolved-xmldocid behaviour (rename → diagnostic).

## Rename → diagnostic loop

The example doesn't ship its own docs site, so the rename-loop teaching surface lives in the docs build that *consumes* this project:

1. The docs site fences `M:FocusedCodeSamplesExample.ModularWordCounter.Tally(...)` from one of its how-to pages.
2. Rename `Tally` → `Reduce` here (just the method name, not the file). Save.
3. `dotnet run --project docs/Pennington.Docs -- build` — the `Pennington.Roslyn` xmldocid preprocessor cannot resolve `M:...Tally(...)` and emits a build-report diagnostic (`Unresolved xmldocid: M:FocusedCodeSamplesExample.ModularWordCounter.Tally(...)`).
4. Either restore the rename or update the fence body to the new id (`Reduce`). The next build runs clean.

The loop is end-to-end with `Pennington.Roslyn`, not a standalone script — the diagnostic surfaces wherever the docs site that references this assembly is built.

## Referenced from

- `docs/.../how-to/code-samples/focused-code-samples.md`
