---
title: Beyond Tree-sitter
---

`Pennington.TreeSitter` pulls declarations into docs for **any**
tree-sitter-supported language, addressing a declaration by its **name path**
(`Type.Member`).

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

## Diff two members with `:symbol-diff`

Use `<lang>:symbol-diff` with exactly two references — before, then after — to
emit a unified diff between them. The `,bodyonly` suffix applies, so this
compares just the bodies:

```python:symbol-diff,bodyonly
calc.py > Calculator.add
calc.py > Calculator.subtract
```
