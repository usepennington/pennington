---
title: Wiki-links and math markup
description: A custom [[wiki-link]] inline parser registered via ConfigureMarkdownPipeline, plus the math markup the built-in pipeline already emits.
---

The `WikiLinkExtension` registered through `penn.ConfigureMarkdownPipeline`
teaches Markdig one new inline token. A bare target links to its slug — see
the [[Glossary]] — and the piped form sets its own label, like
[[content-pipeline|how rendering works]]. Each renders as
`<a class="wikilink" href="/notes/<slug>/">`, so the response pipeline prefixes
the internal href the same way it prefixes any other link.

A single bracket is untouched: an ordinary [link](https://example.com) still
parses through the built-in CommonMark link parser.

## Math is already on

No extension registration is needed for math — `UseAdvancedExtensions` (part of
Pennington's default pipeline) already parses it. Inline math like $E = mc^2$
renders to a `<span class="math">`, and a display block to a `<div class="math">`:

$$
\int_0^1 x^2 \,\mathrm{d}x = \frac{1}{3}
$$

The markup ships in KaTeX/MathJax delimiters; rendering it is a client-side step,
not a Markdig one.
