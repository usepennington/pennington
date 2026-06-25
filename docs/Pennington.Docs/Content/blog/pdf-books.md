---
title: Turn your docs into a PDF
description: An optional package builds a self-contained PDF book from your content. Carve the table of contents into books, and the static build emits them next to the HTML.
author: Phil Scott
date: 2026-06-12
isDraft: false
tags:
  - pdf
  - books
---

You write your docs once as Markdown. `Pennington.Book` turns the same content
into a downloadable PDF, without a second source of truth or an extra toolchain
in your own project.

## Defining books

You carve the navigation tree into books by route prefix. One line per area gives
you a PDF whose slug lines up with that area; configure none and you get a single
whole-site book:

```csharp
builder.Services.AddPenningtonBook(book =>
{
    book.Books.Add(new BookDefinition("Guides", "/how-to/"));
    book.Books.Add(new BookDefinition("Reference", "/reference/"));
});
```

There's no `Use` call. Registration is all of it. Each book reuses the render
pipeline the live site uses: every page's HTML is converted back to Markdown and
re-rendered (the same round trip [llms.txt](xref:how-to.feeds.llms-txt) makes),
then composed with a cover, a page-numbered table of contents, and a colophon.
Print CSS and a [paged.js](https://pagedjs.org/) polyfill are inlined, and images
are embedded as data URIs, so the file stands on its own.

The dev server serves `/pdf/{slug}.pdf` on demand and a live
`/book-preview/{slug}/` for tuning the print CSS; the static build writes the
PDFs alongside the HTML. On a DocSite, each area's sidebar grows a "Download as
PDF" link automatically.

## It needs a browser

The honest cost: the PDF is rendered by headless Chromium through PuppeteerSharp.
On first run PuppeteerSharp downloads a private Chromium (around 150 MB) unless
you point `ChromiumExecutablePath` at one you already have, and renders run one at
a time. `diag books` prints each book's chapters and pages without launching
Chromium at all, so you can check the structure cheaply. The [generated-artifacts
how-to](xref:how-to.content-services.emit-generated-artifacts) explains how books
fit the artifact tier.
