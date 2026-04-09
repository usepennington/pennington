---
title: "Code Block Directives"
description: "Complete syntax reference for all line-level directives (highlight, diff-add, diff-remove, focus, error, warning, word), snippet regions (include/exclude), comment marker support, generated CSS classes, and tabbed code block syntax"
uid: "penn.reference.code-block-directives"
order: 10
---

Exhaustive syntax reference for all code block features. Use a consistent format for each directive: syntax, what it does, the CSS class(es) it generates, and any variants. Line-level directives: `[!code highlight]`/`[!code hl]` (yellow bg), `[!code ++]` (green + diff-add), `[!code --]` (red + diff-remove), `[!code focus]` (high vis, others blurred), `[!code error]` (red), `[!code warning]` (amber), `[!code word:term]` (border around word), `[!code word:term|message]` (border + tooltip). Snippet regions: `[!code include-start:name]`/`[!code include-end:name]`, `[!code exclude-start:name]`/`[!code exclude-end:name]`. Document the 10 supported comment markers (`//`, `#`, `<!-- -->`, `--`, etc.). Document the fence info string key-value syntax: ` ```csharp title="Example" highlight="1,3-5" hide="10-12" `. Document tabbed code block syntax (`tabs=true`). This page is the reference companion to the how-to guide — no narrative explanation of when to use each feature.
