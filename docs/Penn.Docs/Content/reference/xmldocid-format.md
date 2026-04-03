---
title: "XmlDocId Format"
description: "Complete reference for XML Documentation ID string format and usage in MyLittleContentEngine"
uid: "docs.reference.xmldocid-format"
order: 4003
---

XML Documentation ID strings (XmlDocId) are used to uniquely identify types, members, and other constructs in .NET assemblies. MyLittleContentEngine uses these identifiers for API documentation generation and cross-referencing.

## Format Specification

XML Documentation ID strings follow a specific format defined by the .NET documentation system. Each ID string consists of a prefix character followed by a fully qualified name.

### Prefix Characters

| Prefix | Element Type | Description                                    |
|--------|--------------|------------------------------------------------|
| `T:`   | Type         | Classes, interfaces, structs, enums, delegates |
| `F:`   | Field        | Fields, constants                              |
| `P:`   | Property     | Properties, indexers                           |
| `M:`   | Method       | Methods, constructors, operators               |
| `E:`   | Event        | Events                                         |
| `N:`   | Namespace    | Namespaces                                     |

### Basic Format Examples

#### Types

```
T:MyLittleContentEngine.Models.IFrontMatter
T:System.String
T:System.Collections.Generic.List`1
```

#### Methods

```
M:MyLittleContentEngine.Services.Content.ContentFilesService`1.GetContentFiles
M:System.String.Substring(System.Int32)
M:System.String.Substring(System.Int32,System.Int32)
```

#### Properties

```
P:MyLittleContentEngine.Models.IFrontMatter.Title
P:MyLittleContentEngine.ContentOptions.PostFilePattern
```

#### Fields

```
F:MyLittleContentEngine.Models.Tag.Name
F:System.String.Empty
```

## Advanced Format Rules

### Generic Types

Generic types use backticks followed by the number of type parameters:

```
T:System.Collections.Generic.List`1
T:System.Collections.Generic.Dictionary`2
T:MyLittleContentEngine.Services.Content.ContentFilesService`1
```

### Nested Types

Nested types use the `+` character to separate the containing type from the nested type:

```
T:OuterClass+InnerClass
T:MyLittleContentEngine.Models.ApiReference+TypeInfo
```

### Method Parameters

Method parameters are enclosed in parentheses and separated by commas:

```
M:System.String.Substring(System.Int32)
M:System.String.Substring(System.Int32,System.Int32)
M:MyLittleContentEngine.Services.Content.MarkdownContentService`1.ProcessContent(System.String,System.String)
```

### Generic Method Parameters

Generic method parameters use curly braces:

```
M:System.Linq.Enumerable.Where``1(System.Collections.Generic.IEnumerable{``0},System.Func{``0,System.Boolean})
```

### Array Parameters

Array parameters use square brackets:

```
M:MyMethod(System.String[])
M:MyMethod(System.Int32[,])  // Multi-dimensional array
M:MyMethod(System.Int32[][]) // Jagged array
```


## Usage in MyLittleContentEngine

### Code Block References

You'll commonly use XmlDocId strings in code blocks to reference specific API elements:

````markdown
```csharp:xmldocid
T:MyLittleContentEngine.Models.IFrontMatter
```
````

### API Documentation Generation

MyLittleContentEngine automatically generates API documentation using XmlDocId strings to organize and link content.

## Getting XmlDocId Values

### Using JetBrains Rider or ReSharper

1. **Right-click** on the type, method, or member you want to reference
2. Select **"Copy Code Reference"** from the context menu
3. The XmlDocId gets copied to your clipboard

**Alternative method:**

1. Place your cursor on the element
2. Use the keyboard shortcut **Ctrl+Shift+Alt+C** (Windows/Linux) or **Cmd+Shift+Alt+C** (macOS)

### Manual Construction

For simple cases, you can manually construct XmlDocId strings:

1. **Identify the prefix** (T:, M:, P:, F:, E:, N:)
2. **Add the fully qualified name** including namespace
3. **Include type parameters** for generics using backticks
4. **Add parameter types** for methods in parentheses

## Common Examples

### MyLittleContentEngine Types

```
T:MyLittleContentEngine.Models.IFrontMatter
T:MyLittleContentEngine.Models.MarkdownContentPage
T:MyLittleContentEngine.ContentEngineContentOptions`1
T:MyLittleContentEngine.Services.Content.IContentService`1
```

### MyLittleContentEngine Methods

```
M:MyLittleContentEngine.Services.Content.ContentFilesService`1.GetContentFiles
M:MyLittleContentEngine.Services.Content.MarkdownContentService`1.GetContent
M:MyLittleContentEngine.ContentEngineExtensions.AddContentEngineService(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Func{System.IServiceProvider,MyLittleContentEngine.ContentEngineOptions})
```

### MyLittleContentEngine Properties

```
P:MyLittleContentEngine.Models.IFrontMatter.Title
P:MyLittleContentEngine.ContentEngineContentOptions`1.PostFilePattern
P:MyLittleContentEngine.Models.MarkdownContentPage.FrontMatter
```
