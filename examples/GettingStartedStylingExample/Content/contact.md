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

When the response is observed by `CssClassCollectorProcessor`, the
`text-primary-700` and `font-semibold` tokens are registered and the next
request to `/styles.css` includes their rules.
