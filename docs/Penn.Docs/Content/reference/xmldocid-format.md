---
title: "XmlDocId Format"
description: "Reference for XML Documentation ID strings used by Penn.Roslyn — format specification, prefix characters, encoding rules, and usage in code blocks"
uid: "penn.reference.xmldocid-format"
order: 4003
---

XML Documentation ID strings (XmlDocIds) uniquely identify every type, method, property, field, event, and namespace in a .NET assembly. The .NET compiler generates them automatically from source code. Penn.Roslyn uses XmlDocIds to locate symbols in a Roslyn workspace and render their source code in documentation pages.

This page documents the XmlDocId format, its encoding rules, and how Penn consumes these IDs.

## Format Specification

Every XmlDocId follows this structure:

```
prefix:fully.qualified.name
```

The prefix is a single uppercase letter. The colon separates it from the fully qualified name. There are no spaces.

## Prefix Characters

| Prefix | Element | Examples |
|--------|---------|----------|
| `N:` | Namespace | `N:Penn.FrontMatter` |
| `T:` | Type (class, struct, record, interface, enum, delegate) | `T:Penn.FrontMatter.IFrontMatter` |
| `M:` | Method (method, constructor, operator, finalizer) | `M:Penn.Routing.UrlPath.Combine(Penn.Routing.UrlPath)` |
| `P:` | Property (property, indexer) | `P:Penn.FrontMatter.IFrontMatter.Title` |
| `F:` | Field (field, constant, enum member) | `F:Penn.Generation.OutputOptions.DefaultOutputPath` |
| `E:` | Event | `E:MyApp.Services.FileWatcher.Changed` |

## Types

### Simple Types

Use the fully qualified name including namespace:

```
T:Penn.FrontMatter.IFrontMatter
T:Penn.FrontMatter.DocFrontMatter
T:Penn.Islands.IIslandRenderer
T:Penn.MonorailCss.MonorailCssOptions
```

### Generic Types

Append a backtick and the number of type parameters:

```
T:System.Collections.Generic.List`1
T:System.Collections.Generic.Dictionary`2
T:Penn.Content.MarkdownContentService`1
```

`List<T>` has one type parameter, so the suffix is `` `1 ``. `Dictionary<TKey, TValue>` has two, so the suffix is `` `2 ``.

### Nested Types

Separate the outer type from the inner type with `+`:

```
T:Penn.Islands.SpaEnvelopeSerializer+Options
T:MyApp.OuterClass+InnerClass
T:MyApp.OuterClass+InnerClass+DeeplyNested
```

## Methods

Method IDs include parameter types in parentheses, fully qualified and comma-separated. No spaces between parameters.

```
M:Penn.MonorailCss.ColorPaletteGenerator.GenerateFromHue(System.Double)
M:Penn.Infrastructure.PennExtensions.AddPenn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Penn.Infrastructure.PennOptions})
```

A method with no parameters has empty parentheses:

```
M:Penn.Roslyn.Symbols.ISymbolExtractionService.ClearCache
```

### Constructors

Constructors use `#ctor` in place of the method name:

```
M:Penn.Islands.RenderContext.#ctor(Penn.Routing.UrlPath,System.String,System.String)
```

Static constructors use `#cctor`:

```
M:MyApp.Constants.#cctor
```

### Generic Methods

A method's own type parameters use double backticks followed by the count:

```
M:Penn.Islands.ComponentRenderer.RenderComponentAsync``1(System.Collections.Generic.IDictionary{System.String,System.Object})
```

The ` ``1 ` after the method name means one method-level type parameter. References to that parameter in the signature appear as `` ``0 `` (zero-indexed).

Type-level generic parameters (from the containing class) use a single backtick: `` `0 ``, `` `1 ``.

### Generic Parameters in Signatures

Generic type arguments in parameter types are enclosed in curly braces, not angle brackets:

```
M:Type.Method(System.Collections.Generic.List{System.String})
M:Type.Method(System.Func{System.String,System.Boolean})
M:Type.Method(System.Collections.Generic.Dictionary{System.String,System.Collections.Generic.List{System.Int32}})
```

`List<string>` becomes `List{System.String}`. Nested generics nest the curly braces.

### Extension Methods

Extension methods are encoded as static methods on their declaring class. The `this` modifier does not appear in the ID:

```
M:Penn.Infrastructure.PennExtensions.AddPenn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Penn.Infrastructure.PennOptions})
M:Penn.MonorailCss.MonorailServiceExtensions.AddMonorailCss(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{System.IServiceProvider,Penn.MonorailCss.MonorailCssOptions})
```

## Properties

### Simple Properties

```
P:Penn.FrontMatter.IFrontMatter.Title
P:Penn.FrontMatter.IDraftable.IsDraft
P:Penn.FrontMatter.ITaggable.Tags
P:Penn.MonorailCss.MonorailCssOptions.ColorScheme
P:Penn.Islands.RenderContext.BaseUrl
```

### Indexers

Indexers use `Item` as the property name with the parameter type in parentheses:

```
P:System.Collections.Generic.List`1.Item(System.Int32)
P:System.Collections.Generic.Dictionary`2.Item(`0)
```

## Fields and Events

Fields and constants:

```
F:Penn.Generation.OutputOptions.DefaultOutputPath
F:MyApp.Settings.MaxRetryCount
```

Enum members are fields:

```
F:Penn.MonorailCss.AlgorithmicColorScheme.Complementary
F:Penn.MonorailCss.AlgorithmicColorScheme.Analogous
```

Events:

```
E:MyApp.Services.FileWatcher.Changed
E:MyApp.Services.FileWatcher.Error
```

## Arrays and Special Parameters

Array parameters use square brackets:

```
M:MyApp.Processor.Run(System.String[])
```

Multi-dimensional arrays use commas inside the brackets:

```
M:MyApp.Processor.Run(System.Int32[,])
M:MyApp.Processor.Run(System.Int32[,,])
```

Jagged arrays stack the brackets:

```
M:MyApp.Processor.Run(System.Int32[][])
```

`ref` and `out` parameters use `@`:

```
M:MyApp.Parser.TryParse(System.String,System.Int32@)
```

Pointer types use `*`:

```
M:MyApp.Interop.Process(System.Int32*)
```

Nullable value types use `System.Nullable{System.Int32}`. Nullable reference type annotations (`string?` vs `string`) do not change the XmlDocId.

## Usage in Penn

Penn.Roslyn provides four code block modifiers that accept XmlDocIds. Add the modifier after the language identifier in a fenced code block. For full setup instructions, see [Connecting to Roslyn](xref:penn.getting-started.connecting-to-roslyn).

### `:xmldocid`

Renders the full source declaration of one or more symbols. Place each XmlDocId on its own line:

````markdown
```csharp:xmldocid
T:Penn.FrontMatter.IFrontMatter
```
````

Multiple symbols in one block:

````markdown
```csharp:xmldocid
T:Penn.FrontMatter.IDraftable
T:Penn.FrontMatter.ITaggable
T:Penn.FrontMatter.IOrderable
```
````

### `:xmldocid,bodyonly`

Renders only the implementation body, stripping the declaration signature, braces, and attributes. For methods, this returns the method body. For types, this returns the content between the type's braces:

````markdown
```csharp:xmldocid,bodyonly
M:Penn.Infrastructure.PennExtensions.AddPenn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Penn.Infrastructure.PennOptions})
```
````

### `:xmldocid-diff`

Accepts exactly two XmlDocIds and renders a line-level diff. Lines from the first symbol that are absent in the second appear as removals. Lines in the second that are absent in the first appear as additions:

````markdown
```csharp:xmldocid-diff
T:Penn.FrontMatter.IFrontMatter
T:Penn.FrontMatter.DocFrontMatter
```
````

Also supports `,bodyonly`:

````markdown
```csharp:xmldocid-diff,bodyonly
M:MyApp.Services.OldService.Process
M:MyApp.Services.NewService.Process
```
````

### `:path`

Includes a file by its path relative to the solution root. This modifier uses the solution directory, not XmlDocIds:

````markdown
```csharp:path
src/Penn/FrontMatter/IFrontMatter.cs
```
````

Directory traversal (`..`) and absolute paths are rejected.

## Getting XmlDocId Values

### JetBrains Rider / ReSharper

Right-click any symbol and select **Copy Code Reference**. The XmlDocId is copied to your clipboard.

Keyboard shortcut: **Ctrl+Shift+Alt+C** (Windows/Linux), **Cmd+Shift+Alt+C** (macOS).

### Visual Studio XML Documentation File

Enable XML documentation output in your project file:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

Build the project. The generated XML file in `bin/` contains every XmlDocId for the assembly. Search the file for the member name.

### Manual Construction

1. Choose the prefix: `T:`, `M:`, `P:`, `F:`, `E:`, or `N:`.
2. Write the fully qualified name, including namespace.
3. For generic types, append `` `N `` where N is the type parameter count.
4. For generic methods, append `` ``N `` after the method name.
5. For methods, add parameter types in parentheses, comma-separated, fully qualified.
6. For generic arguments in parameters, use `{` and `}` instead of `<` and `>`.

## Penn Normalization

Penn normalizes XmlDocIds before lookup by stripping namespace prefixes from parameter types. Both of these resolve to the same symbol:

```
M:Type.Method(System.String,System.Int32)
M:Type.Method(String,Int32)
```

You can use either the fully qualified or the short form in `:xmldocid` code blocks. The normalizer handles generic parameters (`` `0 ``, `` ``0 ``), nested generics, arrays, and `ref`/`out` markers.

## Penn Type Quick Reference

Commonly referenced Penn XmlDocIds:

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
T:Penn.Infrastructure.PennOptions
T:Penn.Islands.IIslandRenderer
T:Penn.Islands.RenderContext
T:Penn.Islands.ComponentRenderer
T:Penn.MonorailCss.MonorailCssOptions
T:Penn.MonorailCss.AlgorithmicColorScheme
T:Penn.Roslyn.RoslynOptions
M:Penn.Infrastructure.PennExtensions.AddPenn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Penn.Infrastructure.PennOptions})
M:Penn.Roslyn.RoslynExtensions.AddPennRoslyn(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Action{Penn.Roslyn.RoslynOptions})
```
