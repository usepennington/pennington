# BeyondTreeSitterExample

Demonstrates `Pennington.TreeSitter` — multi-language code-fragment extraction in
doc markdown via the `:symbol` fence modifier.

## What it teaches

- `AddTreeSitter(o => o.ContentRoot = "Samples")` lights up the
  `<lang>:symbol` fence for every tree-sitter-supported language.
- Addressing a declaration by **name path** (`Type.Member`), which works
  uniformly across languages.
- The fence body format: `<file> > <Member.Path>` (one per line); a bare
  `<file>` embeds the whole file; `,bodyonly` returns just the body.
- `<lang>:symbol-diff` over two references emits a before/after unified diff.
- Cross-language resolution quirks: Rust methods resolve through their
  `impl` block; Go/TypeScript/Python all work from the same generic resolver.

## Layout

- `Program.cs` — `AddDocSite` + `AddTreeSitter`.
- `Samples/` — the source files (`calc.py`, `calc.rs`, `calc.go`, `calc.ts`)
  the fences read. Excluded from the host's compile globs via `DefaultItemExcludes`.
- `Content/index.md` — the page exercising each fence form.

## Run

```
dotnet run --project examples/BeyondTreeSitterExample
```

Where it is referenced from the docs site: _(reference for `Pennington.TreeSitter`)_.

> The `:symbol` modifier is multi-language and syntactic — it matches
> declarations by name path, without semantic or type resolution.
