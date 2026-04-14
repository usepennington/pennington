---
title: DocSite Chrome Overrides
description: Running DocSite that demonstrates every override seam.
sectionLabel: Guides
order: 10
---

# DocSite Chrome Overrides

This page renders through the stock DocSite template, but the
surrounding chrome is customised end-to-end by
`SiteChromeOverrides.BuildDocSiteOptions`:

- The `<head>` carries an extra `<meta>` and `<link>` tag from
  `AdditionalHtmlHeadContent`.
- `/styles.css` ends with the rules appended via `ExtraStyles`.
- The header and footer strings come straight from `HeaderContent` /
  `FooterContent`.
- `/extra` resolves because `AdditionalRoutingAssemblies` widens the
  router to this project's assembly, where `Components/ExtraPage.razor`
  lives.
