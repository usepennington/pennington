---
title: "Auto-generate an API reference tree for a class library"
description: "Walk the Roslyn workspace once on startup, emit one `/reference/api/{type}/` page per public type, and render member tables from xmldoc comments — the same pipeline Pennington's own docs site uses."
uid: how-to.extensibility.auto-api-reference
order: 203080
sectionLabel: Extensibility
tags: [extensibility, roslyn, xmldoc, api-reference, content-service]
---

To ship a DocSite whose reference section stays in sync with a class library's public surface — no hand-written type pages, no member lists to keep current — pair `Pennington.Roslyn`'s documentation primitives (`IMemberEnumerator`, `IXmlDocParser`, `IXmlDocHtmlRenderer`) with a custom `IContentService` that enumerates types from the Roslyn workspace. The three pieces to write are an *index* (walks the workspace once, publishes slug → type entries), a *content service* (yields one route per entry), and a *Razor page* (reads the xmldocid from its route parameter and renders members via the injected primitives).

This is the same plumbing Pennington's own docs site uses. The source lives at `docs/Pennington.Docs/ApiReference/` and `docs/Pennington.Docs/Components/Reference/` — fenced below as the worked example.

## Before you begin

- Completed <xref:tutorials.beyond-basics.connect-roslyn>, so `AddPenningtonRoslyn` is wired and `SolutionPath` points at a solution containing the library you want to document.
- The target library has `<GenerateDocumentationFile>true</GenerateDocumentationFile>` — without that, `ISymbol.GetDocumentationCommentXml()` returns empty strings and the generated pages have no prose.
- Familiarity with <xref:how-to.extensibility.custom-content-service> — this how-to layers an auto-generated source on top of the `IContentService` contract covered there.

## 1. Build the index

Walk the workspace once on first request, filter to the types you want to document, and publish a `slug → entry` map. Every downstream component (the content service, the Razor page, xref resolution) keys off one of those slugs, so the index is the single place where discovery rules live.

The snippet below collects every public type across the matching projects, skips attributes and `ComponentBase` subclasses, and falls back to a namespace-qualified slug when two types share a name. `ISolutionWorkspaceService` is the interface `AddPenningtonRoslyn` registers for low-level workspace access.

```csharp:path
docs/Pennington.Docs/ApiReference/ApiReferenceIndex.cs
```

The `ApiReferenceWorkspace` helper narrows the project set (replace the `Pennington`-prefixed check with a predicate that matches your library) and flattens nested types:

```csharp:path
docs/Pennington.Docs/ApiReference/ApiReferenceWorkspace.cs
```

## 2. Emit one route per entry

The content service is thin — its entire job is to project index entries into `DiscoveredItem`s and cross-references. No sidebar entries (the generated API pages live under `/reference/api/` but are reached from type-name search and xref, not from the TOC), no content-to-copy, no content-to-create.

```csharp:path
docs/Pennington.Docs/ApiReference/ApiReferenceContentService.cs
```

Two details worth noting:

- `DiscoverAsync` yields a `RazorPageSource` with the component's `AssemblyQualifiedName`. Combined with the `@page "/reference/api/{Key}"` route on the Razor page below, that's how one Razor file renders thousands of URLs.
- `GetIndexableEntriesAsync` is the opt-in channel for "this content should be searchable and llms.txt-indexed, but not sidebar-listed". Returning entries there without also returning them from `GetContentTocEntriesAsync` is exactly the pattern for auto-generated reference surfaces.

## 3. Render a page per type

The Razor page is parameterised on a route key, looks the entry up in the index, and delegates all the member rendering to the Pennington.UI reference components (`<ApiSummary>`, `<ApiMemberTable>`) which in turn inject `IMemberEnumerator` and `IXmlDocHtmlRenderer`.

```razor:path
docs/Pennington.Docs/Components/Reference/ApiReferencePage.razor
```

`<ApiMemberTable XmlDocId="@_entry.XmlDocId" Kind="All" />` pulls the full member set in one call; narrow it to `Kind="Properties"` or `Kind="Methods"` if the layout calls for separate tables per kind.

### Available primitives

`AddPenningtonRoslyn` registers these three services; inject them directly into Razor components or plain C# services as needed:

| Interface | Purpose |
|---|---|
| `IMemberEnumerator` | `EnumerateAsync(xmldocid, kind, access, order)` returns a list of `MemberDescriptor` records with parsed xmldoc and a formatted type signature. |
| `IXmlDocParser` | Turns the raw XML string from `ISymbol.GetDocumentationCommentXml()` into a `ParsedXmlDoc` tree. Use when authoring a custom renderer. |
| `IXmlDocHtmlRenderer` | Renders `ParsedXmlDoc` nodes to HTML (`RenderHtml` for block, `RenderInlineHtml` for table-cell contexts). |

## 4. Wire it into the host

Register the index as a singleton, the content service both concretely and via `IContentService`, and make sure `AddPenningtonRoslyn` runs first so the workspace is available.

```csharp
builder.Services.AddPenningtonRoslyn(opts =>
    opts.SolutionPath = "../MyLibrary.slnx");

builder.Services.AddSingleton<ApiReferenceIndex>();
builder.Services.AddSingleton<ApiReferenceContentService>();
builder.Services.AddSingleton<IContentService>(sp =>
    sp.GetRequiredService<ApiReferenceContentService>());
```

## Result

One page per public type appears at `/reference/api/{slug}/`, with summary, member table, and xref ids auto-published:

```text
/reference/api/calculator/        -> uid: reference.api.calculator
/reference/api/greeter/           -> uid: reference.api.greeter
/reference/api/options-record/    -> uid: reference.api.options-record
```

`<xref:reference.api.calculator>` in any markdown page now links to the generated page, and the pages flow through search and llms.txt the same as hand-authored content.

## Related

- Tutorial: <xref:tutorials.beyond-basics.connect-roslyn> — the `AddPenningtonRoslyn` / `SolutionPath` wire-up this how-to extends.
- How-to: <xref:how-to.extensibility.custom-content-service> — the generic `IContentService` recipe; this page is a specialization.
- Reference: <xref:reference.api.i-member-enumerator>, <xref:reference.api.i-xml-doc-parser>, <xref:reference.api.i-xml-doc-html-renderer> — the three primitives.
