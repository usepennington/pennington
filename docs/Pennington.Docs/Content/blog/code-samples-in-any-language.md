---
title: Code samples in any language
description: Code samples now come from tree-sitter, addressed by name path, so a fence can pull a Python function or a Rust impl, not just C#. The Roslyn integration is gone.
author: Phil Scott
date: 2026-05-23
isDraft: false
tags:
  - code-samples
  - tree-sitter
---

Pennington pulls code samples from your source files instead of copy-paste, so a
sample can't drift from the code it documents. That used to run on Roslyn, which
meant the samples had to be C#. They don't anymore.

## Referencing a member by name

A `:symbol` fence names a file and a member, and the current source renders at
build time:

`````markdown
```csharp:symbol
src/Pennington/Pipeline/ContentPipeline.cs > ContentPipeline.ParseAsync
```
`````

The part after `>` is a name path (`Type`, `Type.Method`, a nested
`Type.Inner.Member`) matched in the file's syntax tree, with no line numbers to
go stale. Drop the `>` and the member name, and the fence embeds the whole file.

## Nine languages

The parser is [tree-sitter](https://tree-sitter.github.io/tree-sitter/), so
member addressing works across C#, Python, TypeScript, JavaScript, Java, Rust,
Go, Ruby, and PHP. The language before the colon picks the grammar:

`````markdown
```python:symbol
samples/pipeline.py > Pipeline.parse
```
`````

Whole-file embeds work for any language at all, since they don't need a grammar.

A few flags refine the output: `,bodyonly` drops the declaration, `,imports`
prepends the file's import lines, `,signatures` collapses bodies to an outline,
and `:symbol-diff` renders a before/after between two members. The [code-block
argument reference](xref:reference.markdown.code-block-args) lists them all.

## What it doesn't do

The matching is syntactic, not semantic. It finds a member by name; it doesn't
resolve types. Overloads resolve to the first declaration of that name, so point
at something unambiguous or write the snippet by hand. And `,imports` is
unfiltered: it prepends every import in the file, not just the ones the snippet
uses. Wiring it up takes `AddTreeSitter` with a content root; the [focused
code-samples how-to](xref:how-to.code-samples.focused-code-samples) has the
setup.
