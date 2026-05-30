---
title: Lowercase rewriter demo
description: Anchor text with data-lowercase is normalized by AnchorLowercaseRewriter.
---

`AnchorLowercaseRewriter` demonstrates both halves of the
`IHtmlResponseRewriter` contract. The `PreParseAsync` pass strips a
synthetic sentinel (`<!--LOWERCASE-SENTINEL-->`) from the raw HTML
before AngleSharp parses it. The `ApplyAsync` pass walks every
`<a data-lowercase>` element and lowercases its text content.

- <a href="/" data-lowercase>HOME</a>
- <a href="/pipeline-demo/" data-lowercase>Pipeline DEMO</a>
- <a href="/line-count-demo/" data-lowercase>Line-Count DEMO</a>
- <a href="/" >Untouched (no data-lowercase)</a>

<!--LOWERCASE-SENTINEL-->

The sentinel comment above is removed by `PreParseAsync` — view source
after the page renders; it is gone.
