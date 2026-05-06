---
title: Contact
description: How to reach the author.
order: 30
---

# Get in touch

Because this page has `order: 30` in its front matter, it sorts after
**About** (`order: 20`) in the nav strip. Ordering is still driven entirely
by front matter — picking up MonorailCSS did not move that logic into code.

<p class="text-primary-700 font-semibold">This paragraph uses an inline utility class.</p>

Because the literal `text-primary-700 font-semibold` string lives inside this
markdown file (and ends up baked into the rendered HTML at build time), the
MonorailCSS discovery pipeline picks both tokens up and the next request to
`/styles.css` includes their rules.
