---
title: Line-count preprocessor demo
description: Fenced code block intercepted by LineCountPreprocessor before highlighting.
---

The preprocessor runs before the highlighter picks a language. When the
fence info string is `linecount`, `LineCountPreprocessor.TryProcess`
returns a `CodeBlockPreprocessResult` that wraps the code in a
`<figure class="linecount">` element showing how many lines the snippet
spans. `SkipTransform = true` skips `CodeTransformer` so our wrapper
reaches the page verbatim.

```linecount
one
two
three
four
five
```

A normal fence (no `linecount`) passes through the preprocessor chain
untouched and is highlighted the usual way:

```text
one
two
three
```
