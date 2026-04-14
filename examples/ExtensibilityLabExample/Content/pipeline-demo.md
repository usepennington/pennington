---
title: Pipeline highlighter demo
description: Fenced code block in the fictional pipeline DSL handled by PipelineHighlighter.
---

# Pipeline highlighter demo

The fence below is tagged `pipeline` — it is routed to
`PipelineHighlighter` because that highlighter declares the `pipeline`
language in its `SupportedLanguages`. Arrows (`->`, `|`) and known
keywords (`source`, `filter`, `sink`) are colored with CSS classes the
stylesheet can theme.

```pipeline
source "orders" -> filter where=paid | transform total=sum | sink "warehouse"
```

Compare with a plain-text fence that no highlighter claims:

```text
source "orders" -> filter where=paid | transform total=sum | sink "warehouse"
```

The first renders with `<span class="pipeline-arrow">` tokens; the second
is HTML-encoded by `PlainTextHighlighter` without spans.
