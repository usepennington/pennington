---
title: "Highlighting interfaces"
description: "The two highlighting extension contracts — ICodeHighlighter and ICodeBlockPreprocessor — plus the HighlightingService dispatcher and the TextMateLanguageRegistry grammar registry."
sectionLabel: "Extension Points"
order: 405050
tags: [highlighting, extension-points, code-blocks, textmate]
uid: reference.extension-points.highlighting
---

## `ICodeHighlighter`

<ApiSummary XmlDocId="T:Pennington.Highlighting.ICodeHighlighter" />

<ApiMemberTable XmlDocId="T:Pennington.Highlighting.ICodeHighlighter" Kind="All" />

## `ICodeBlockPreprocessor`

<ApiSummary XmlDocId="T:Pennington.Markdown.Extensions.ICodeBlockPreprocessor" />

<ApiMemberTable XmlDocId="T:Pennington.Markdown.Extensions.ICodeBlockPreprocessor" Kind="All" />

### `CodeBlockPreprocessResult`

<ApiSummary XmlDocId="T:Pennington.Markdown.Extensions.CodeBlockPreprocessResult" />

<ApiMemberTable XmlDocId="T:Pennington.Markdown.Extensions.CodeBlockPreprocessResult" />

## `HighlightingService`

<ApiSummary XmlDocId="T:Pennington.Highlighting.HighlightingService" />

<ApiMemberTable XmlDocId="T:Pennington.Highlighting.HighlightingService" Kind="Methods" />

## `TextMateLanguageRegistry`

<ApiSummary XmlDocId="T:Pennington.Highlighting.TextMateLanguageRegistry" />

<ApiMemberTable XmlDocId="T:Pennington.Highlighting.TextMateLanguageRegistry" Kind="Methods" />

## Example

See `examples/ExtensibilityLabExample/PipelineHighlighter.cs` for a complete custom `ICodeHighlighter`.

## See also

- How-to: [Add a custom syntax highlighter](xref:how-to.extensibility.custom-highlighter)
- How-to: [Register a code-block preprocessor](xref:how-to.extensibility.code-block-preprocessor)
- Related reference: [`HighlightingOptions`](xref:reference.options.auxiliary-options)
- Background: [The syntax-highlighting cascade](xref:explanation.rendering.highlighting)
