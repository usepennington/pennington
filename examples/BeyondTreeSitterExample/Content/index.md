---
title: Beyond Tree-sitter
---

# Multi-language snippets with `:symbol`

`Pennington.Roslyn` can pull C#/VB declarations into docs by their XmlDocId.
`Pennington.TreeSitter` does the same idea for **any** tree-sitter-supported
language, addressing a declaration by its **name path** (`Type.Member`).

Each fenced block uses the `<lang>:symbol` info-string. The body is one
`<file> > <Member.Path>` reference per line, resolved under the configured
`ContentRoot` (here, `Samples/`).

## Python

```python:symbol
calc.py > Calculator.add
```

## Rust

The `add` method lives in an `impl Calculator` block — tree-sitter resolves
`Calculator.add` by descending into the impl, not the struct.

```rust:symbol
calc.rs > Calculator.add
```

## Go

A free function needs no type prefix:

```go:symbol
calc.go > Add
```

## TypeScript

```typescript:symbol
calc.ts > Calculator.add
```

## Body only

Append `,bodyonly` to drop the signature and surrounding braces:

```python:symbol,bodyonly
calc.py > Calculator.add
```

## Whole file

A bare reference with no `>` embeds the entire file:

```python:symbol
calc.py
```
