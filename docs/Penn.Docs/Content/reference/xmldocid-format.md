---
title: "XmlDocId Format"
description: "Reference for XML Documentation ID strings — format specification, prefix characters, and Penn-specific examples"
uid: "penn.reference.xmldocid-format"
order: 4003
---

XML Documentation ID strings (XmlDocId) are the .NET ecosystem's way of giving every type, method, property, and field a stable, unique name. They're not pretty — nobody has ever called `M:System.Linq.Enumerable.Where``1(System.Collections.Generic.IEnumerable{``0},System.Func{``0,System.Boolean})` elegant — but they're unambiguous, and Penn uses them for API documentation generation and code block references.

## Format Specification

Every XmlDocId is a prefix character, a colon, and a fully qualified name. The prefix tells you what kind of thing it is.

### Prefix Characters

| Prefix | Element Type | Description |
|--------|-------------|-------------|
| `T:` | Type | Classes, interfaces, structs, enums, delegates, records |
| `F:` | Field | Fields, constants |
| `P:` | Property | Properties, indexers |
| `M:` | Method | Methods, constructors, operators, extension methods |
| `E:` | Event | Events |
| `N:` | Namespace | Namespaces |

## Types

The most common usage. Fully qualified name, namespace and all:

```
T:Penn.FrontMatter.IFrontMatter
T:Penn.FrontMatter.DocFrontMatter
T:Penn.FrontMatter.BlogFrontMatter
T:Penn.FrontMatter.IDraftable
T:Penn.Islands.IIslandRenderer
T:Penn.Islands.RenderContext
T:Penn.MonorailCss.MonorailCssOptions
T:Penn.MonorailCss.AlgorithmicColorScheme
```

### Generic Types

Generic types use a backtick followed by the number of type parameters:

```
T:System.Collections.Generic.List`1
T:System.Collections.Generic.Dictionary`2
T:Penn.Content.MarkdownContentService`1
```

### Nested Types

Nested types use `+` to separate the containing type from the nested type:

```
T:Penn.Islands.SpaEnvelopeSerializer+Options
T:OuterClass+InnerClass
```

## Methods

Method IDs include parameter types in parentheses, fully qualified:

```
M:Penn.Infrastructure.PennExtensions.AddPenn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Penn.Infrastructure.PennOptions})
M:Penn.Islands.ComponentRenderer.RenderComponentAsync``1(System.Collections.Generic.IDictionary{System.String,System.Object})
M:Penn.MonorailCss.ColorPaletteGenerator.GenerateFromHue(System.Double)
```

### Constructors

Constructors use `#ctor` instead of the type name:

```
M:Penn.Islands.RenderContext.#ctor(Penn.Routing.UrlPath,System.String,System.String)
```

### Generic Method Parameters

Generic method type parameters use double backticks:

```
M:Penn.Islands.ComponentRenderer.RenderComponentAsync``1(System.Collections.Generic.IDictionary{System.String,System.Object})
```

The ``1`` means the method has one type parameter. References to that parameter in the signature use `{``0}`.

### Extension Methods

Extension methods are documented as static methods on their containing class. The `this` parameter is just a regular parameter:

```
M:Penn.Infrastructure.PennExtensions.AddPenn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Penn.Infrastructure.PennOptions})
M:Penn.MonorailCss.MonorailServiceExtensions.AddMonorailCss(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{System.IServiceProvider,Penn.MonorailCss.MonorailCssOptions})
```

## Properties

```
P:Penn.FrontMatter.IFrontMatter.Title
P:Penn.FrontMatter.IDraftable.IsDraft
P:Penn.FrontMatter.ITaggable.Tags
P:Penn.MonorailCss.MonorailCssOptions.ColorScheme
P:Penn.Islands.RenderContext.BaseUrl
```

## Array and Special Parameters

Array parameters use square brackets:

```
M:MyMethod(System.String[])
M:MyMethod(System.Int32[,])    // Multi-dimensional array
M:MyMethod(System.Int32[][])   // Jagged array
```

Nullable reference types don't change the XmlDocId — `string?` and `string` produce the same ID.

## Usage in Penn

### Code Block References

Penn's markdown renderer supports `xmldocid` code blocks that expand to formatted API documentation:

````markdown
```csharp:xmldocid
T:Penn.FrontMatter.IFrontMatter
```
````

This renders the source code of `IFrontMatter` with syntax highlighting and a link to the source file.

### Source Path References

You can also reference source files directly with `:path`:

````markdown
```csharp:path
src/Penn/FrontMatter/IFrontMatter.cs
```
````

### Cross-References

The `xref:` link format uses the `Uid` from `ICrossReferenceable`, not XmlDocId strings. Don't mix them up — they serve different purposes.

## Getting XmlDocId Values

### JetBrains Rider / ReSharper

1. Right-click the type, method, or member
2. Select **Copy Code Reference**
3. The XmlDocId is now on your clipboard

Keyboard shortcut: **Ctrl+Shift+Alt+C** (Windows/Linux) or **Cmd+Shift+Alt+C** (macOS).

### Visual Studio

The **XML Documentation Comments** feature generates these IDs in the XML doc file during build. Check the `bin/` output for `YourAssembly.xml`.

### Manual Construction

For simple cases:

1. Pick the prefix (`T:`, `M:`, `P:`, `F:`, `E:`, `N:`)
2. Write the fully qualified name including namespace
3. For generics, add `` `N `` where N is the type parameter count
4. For methods, add parameter types in parentheses, comma-separated

## Penn Type Reference

A quick reference of commonly used Penn XmlDocId strings:

```
T:Penn.FrontMatter.IFrontMatter
T:Penn.FrontMatter.IDraftable
T:Penn.FrontMatter.ITaggable
T:Penn.FrontMatter.IOrderable
T:Penn.FrontMatter.IDescribable
T:Penn.FrontMatter.IDateable
T:Penn.FrontMatter.ICrossReferenceable
T:Penn.FrontMatter.ISectionable
T:Penn.FrontMatter.IRedirectable
T:Penn.FrontMatter.DocFrontMatter
T:Penn.FrontMatter.BlogFrontMatter
T:Penn.Islands.IIslandRenderer
T:Penn.Islands.RenderContext
T:Penn.Islands.ComponentRenderer
T:Penn.MonorailCss.MonorailCssOptions
T:Penn.MonorailCss.AlgorithmicColorScheme
```
