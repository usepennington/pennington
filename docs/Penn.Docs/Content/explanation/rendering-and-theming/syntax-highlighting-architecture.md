---
title: "Syntax Highlighting Architecture"
description: "How the priority-based highlighter dispatch system works — covering server-side vs client-side trade-offs, TextMate grammars vs Roslyn semantic highlighting, ICodeBlockPreprocessor hooks, and the full transformation pipeline (highlighting → directive extraction → DOM manipulation → indent normalization)"
uid: "penn.explanation.syntax-highlighting-architecture"
order: 10
---

Explain the architecture of Penn's syntax highlighting system. Start with the design choice: server-side highlighting (no client JavaScript, works in static output, avoids layout shift, and produces consistent output across environments). Discuss the priority-based dispatch in `HighlightingService` — when multiple highlighters claim the same language, the highest-priority one wins. Walk through the highlighter stack: `RoslynHighlighter` (priority 100, semantic highlighting from a live Roslyn workspace), `ShellHighlighter` (75, regex-based tokenization for shell scripts), `TextMateHighlighter` (50, VS Code grammar files via TextMateSharp), `PlainTextHighlighter` (0, fallback). Explain the `ICodeBlockPreprocessor` hook — how Roslyn's preprocessor intercepts code blocks with `:xmldocid` modifiers before normal highlighting. Walk through the full transformation pipeline: highlighting → `CodeTransformer` extracts `[!code ...]` directives → AngleSharp DOM manipulation adds CSS classes and line annotations → indent normalization → final HTML output via `CodeBlockHtmlBuilder`.
