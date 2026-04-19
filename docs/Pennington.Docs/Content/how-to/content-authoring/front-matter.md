---
title: "Work with front matter"
description: "Declare YAML front matter, pick a built-in record, or define your own for custom keys."
uid: how-to.content-authoring.front-matter
order: 201010
sectionLabel: Content Authoring
tags: [front-matter, authoring, yaml]
---

This guide covers declaring YAML front matter in a markdown file, selecting the built-in record that matches the host, and creating a custom record when the built-in types don't expose the keys needed. For the full key catalog, see <xref:reference.front-matter.keys>; for the design rationale, see <xref:explanation.core.front-matter-capabilities>.

## Assumptions

- An existing Pennington site with markdown content under a `Content/` folder (see the [Getting Started tutorial](xref:tutorials.getting-started.first-site) if not).
- The host template in use — `AddDocSite`, `AddBlogSite`, or bare `AddPennington` with `AddMarkdownContent<T>`.
- A markdown file open and ready to fill in (or extend) its YAML block.

To copy a working setup, see [`examples/DocSiteKitchenSinkExample`](https://github.com/usepennington/pennington/tree/main/examples/DocSiteKitchenSinkExample) — `Content/main/front-matter.md` is the page this how-to is fenced from, and `ApiFrontMatter.cs` is the custom-record demo.

---

## Steps

<Steps>
<Step StepNumber="1">

**Declare the YAML block at the top of the file**

Place the YAML between two `---` fences as the very first content in the markdown file — before any heading.

```markdown:path
examples/DocSiteKitchenSinkExample/Content/main/front-matter.md
```

</Step>
<Step StepNumber="2">

**Pick the built-in record that matches your host**

The record is determined by the host: `AddDocSite` binds `DocSiteFrontMatter`, `AddBlogSite` binds `BlogSiteFrontMatter`, and bare `AddPennington` accepts whichever type you pass to `AddMarkdownContent<T>`. The base doc-shaped record is `DocFrontMatter`:

```csharp:xmldocid
T:Pennington.FrontMatter.DocFrontMatter
```

The blog-shaped counterpart for posts:

```csharp:xmldocid
T:Pennington.FrontMatter.BlogFrontMatter
```

</Step>
<Step StepNumber="3">

**Fill in only the keys needed**

Every key on the built-in records has a default, so the YAML block can be as small as `title:` plus whatever the page needs — tags, order, description, uid. The DocSite template exposes the full superset via `DocSiteFrontMatter`:

```csharp:xmldocid
T:Pennington.DocSite.DocSiteFrontMatter
```

</Step>
<Step StepNumber="4">

**Define a custom record for extra keys**

Declare a `public record` implementing `IFrontMatter` and any relevant capability interfaces — `ITaggable`, `IOrderable`, `ISectionable`, `IRedirectable`. See <xref:reference.api.i-front-matter> for the full list of optional interfaces.

```csharp:xmldocid
T:DocSiteKitchenSinkExample.ApiFrontMatter
```

</Step>
<Step StepNumber="5">

**Register the custom record with a markdown source**

Pass the record type to `AddMarkdownContent<T>` so the pipeline deserializes the YAML into that type. `AddDocSite` and `AddBlogSite` each already register one source — chaining a second record requires bare `AddPennington` (see [Use multiple content sources](xref:how-to.configuration.multiple-sources)).

<!-- TODO: xmldocid needed -->

</Step>
</Steps>

---

## Verify

- Run `dotnet run` and visit the page — the rendered `<h1>` matches the `title:` value.
- The sidebar entry appears with the label from `title:` at the position set by `order:`.
- When a custom record is in use, pages under its content source build without `FrontMatterParseError` diagnostics in the build report.

## Related

- Reference: [Front matter key reference](xref:reference.front-matter.keys) — every built-in key, type, and default
- Reference: [Built-in front-matter types](xref:reference.api.doc-front-matter) — `DocFrontMatter`, `BlogFrontMatter`, `DocSiteFrontMatter`, `BlogSiteFrontMatter`
- Reference: [`IFrontMatter` and capability defaults](xref:reference.api.i-front-matter) — the capability interfaces you can add to a custom record
- Background: [The front-matter capability system](xref:explanation.core.front-matter-capabilities) — why the design collapsed ten interfaces into default members
