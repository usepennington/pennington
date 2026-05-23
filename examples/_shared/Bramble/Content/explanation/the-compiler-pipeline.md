---
title: "Inside the compiler pipeline"
description: "A conceptual walkthrough of the stages Bramble source code passes through on its way to bytecode, and why each stage exists where it does."
uid: bramble.explanation.the-compiler-pipeline
order: 50
sectionLabel: "Explanation"
tags: [compiler, pipeline, bytecode, type-checking, stages]
---

Compiling a Bramble program is a sequential process with clearly separated stages. Each stage takes the output of the previous one and produces a richer representation. The ordering is not arbitrary—each stage depends on information that earlier stages have established, and running them out of order would require either guessing or repeating work.

## Lexer

The lexer reads raw source text and produces a flat sequence of tokens: identifiers, keywords, literals, operators, and punctuation. It has no understanding of structure; it only knows how to recognize the atomic units of the language. Errors at this stage are character-level: an unrecognized byte sequence, an unterminated string literal.

The Bramble lexer preserves whitespace and comment tokens in a separate channel rather than discarding them. Sprig, the formatter, uses this channel to reformat source without losing documentation comments. The compiler itself ignores the channel.

## Parser

The parser consumes the token stream and produces an abstract syntax tree (AST). It enforces syntactic rules—that expressions are well-formed, that blocks are properly closed, that the grammar is satisfied. It does not know what any name means or whether types are correct.

Bramble uses a recursive-descent parser, which makes error recovery easier to reason about. When the parser encounters something it does not expect, it tries to synchronize at the next statement boundary and continue, which allows a single compilation to report multiple syntax errors rather than stopping at the first.

## Resolver (name resolution)

The resolver walks the AST and attaches meaning to every identifier: which declaration does this name refer to? Module imports are resolved here, local variable scopes are checked for shadowing and use-before-definition, and public versus private visibility is enforced.

Name resolution must precede type checking because the type checker operates on resolved names. Attempting to type-check before resolving names would require the type checker to perform its own lookup, duplicating logic and making both passes harder to maintain.

## Type checker

The type checker verifies that every expression has a consistent type, infers missing type annotations, and instantiates generic definitions. It works on the resolved AST and produces a typed AST where every node carries its type.

This is the most complex stage. Type inference, trait resolution, and generic instantiation interact in ways that can produce cascading errors—one wrong type can cause many downstream mismatches. Bramble's type checker uses a constraint-based approach internally and attempts to report errors at the earliest point where the constraint cannot be satisfied, rather than at the point where the inconsistency becomes visible.

## Borrow checker

The borrow checker runs after type checking because it needs type information to determine the sizes and copy semantics of values. It performs a dataflow analysis over the typed AST, tracking the state of each value (owned, moved, borrowed) at every point in the control flow. Violations—use after move, conflicting borrows, dangling references—are reported here.

Running the borrow checker after the type checker means that borrow errors are always presented in a context where the types are already known and correct. This improves error messages considerably.

## Bytecode emitter

The final stage takes the verified typed AST and emits bytecode for the Bramble VM. Register allocation, instruction selection, and basic optimizations (constant folding, dead code elimination) happen here. The output is a `.brc` bytecode file that the VM can load directly.

The emitter is kept intentionally simple; it does not perform the aggressive optimizations that a native-code compiler might. The tradeoff is that peak throughput is lower than compiled-to-native languages, but startup time is fast and the bytecode is compact. See [the bytecode VM](xref:bramble.explanation.the-bytecode-vm) for how the output is executed, and [performance characteristics](xref:bramble.explanation.performance-characteristics) for what this means for real workloads.
