---
title: Extensibility Lab
description: Kitchen-sink host wiring the seven Pennington extension points.
---

# Extensibility Lab

This site demonstrates the seven extension points covered by the Pennington
how-to §2.3 Extensibility recipes. Each page below exercises one extension
so the effect is observable in the rendered HTML.

- [Pipeline highlighter demo](/pipeline-demo/) — `PipelineHighlighter`
  implements `ICodeHighlighter` for a fictional `pipeline` DSL.
- [Line-count preprocessor demo](/line-count-demo/) —
  `LineCountPreprocessor` implements `ICodeBlockPreprocessor` and annotates
  fences tagged `linecount`.
- [Releases index](/releases/) — `ReleaseNotesContentService` implements
  `IContentService` over `Content/releases/*.json`.

Every page also exercises two surfaces that run on every response:
`FeedbackWidgetProcessor` (`IResponseProcessor`) injects a "Was this helpful?"
footer, and `AnchorLowercaseRewriter` (`IHtmlResponseRewriter`) lowercases
the text of anchors marked `data-lowercase`.
