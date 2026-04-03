---
title: "Syntax Highlighting System Architecture"
description: "Deep dive into MyLittleContentEngine's multi-layered syntax highlighting system with Roslyn, TextMateSharp, and custom highlighters"
uid: "docs.under-the-hood.syntax-highlighting-system"
order: 3020
---

MyLittleContentEngine features a sophisticated multi-layered syntax highlighting system that provides server-side highlighting for optimal performance while maintaining broad language compatibility. The system uses a cascading approach with four different highlighting engines, each optimized for specific use cases.

> [!NOTE]
> To enable Roslyn syntax highlighting for C# and VB.NET, see [Connecting to Roslyn](xref:docs.getting-started.connecting-to-roslyn). For information about styling syntax highlighting, see [Monorail CSS Configuration](xref:docs.reference.monorail-css-configuration).

## Architecture Overview

The syntax highlighting system follows a priority-based cascade that maximizes accuracy and performance:

```mermaid
graph TD
    A[Code Block] --> B{Language Detection}
    B --> C{Roslyn Available?}
    C -->|Yes + C#/VB| D[Roslyn Highlighter]
    C -->|No| E{Custom Highlighter?}
    E -->|GBNF/Shell/Text| F[Custom Highlighter]
    E -->|No| G{TextMate Grammar?}
    G -->|Available| H[TextMateSharp]
    G -->|Unavailable| I[Client-side Highlight.js]
    
    D --> J[Server-side HTML with CSS Classes]
    F --> J
    H --> J
    I --> K[Client-side Highlighting]
```

This layered approach ensures that:
- **C# and VB.NET** get the most accurate highlighting via Roslyn
- **Common languages** are highlighted server-side for better performance
- **Specialized languages** receive custom tokenization
- **Any language** can be handled via client-side fallback

## Layer 1: Roslyn Highlighter (Highest Priority)

The Roslyn highlighter provides the most accurate syntax highlighting for .NET languages by leveraging Microsoft's actual C# and VB.NET compilers.


### Special Roslyn Features

#### XML Documentation Integration
`````markdown
```csharp:xmldocid
T:System.Collections.Generic.List<T>.Add
```
`````

This extracts the actual implementation of `List<T>.Add` from the loaded assemblies and highlights it with full semantic information.

#### File Path Loading

`````markdown
```csharp:path
examples/MinimalExample/Program.cs
```
`````

Loads and highlights external files from your solution, ensuring documentation stays in sync with actual code.

> [!TIP]  
> This will work for non-C# files too, but they have to be part of the .NET solution.

#### Body-Only Documentation
`````markdown
```csharp:xmldocid,bodyonly
M:MyNamespace.MyClass.MyMethod
```
`````

Shows only the method body without class declaration context.

### Implementation Details

The Roslyn highlighter operates by:
1. **Loading Solutions**: Scans referenced assemblies and XML documentation
2. **Semantic Analysis**: Uses full compiler semantic information
3. **Classification**: Applies precise token classification (keywords, types, literals, etc.)
4. **HTML Generation**: Outputs semantic HTML with CSS classes matching Highlight.js conventions


## Layer 2: Custom Highlighters

Custom highlighters provide specialized tokenization for domain-specific languages that benefit from purpose-built parsers.

### GBNF (Grammar Backus-Naur Form)

**Language**: `gbnf`
**Purpose**: Highlighting grammar definitions for language parsers

```gbnf
root ::= expr
expr ::= term (("+" | "-") term)*
term ::= factor (("*" | "/") factor)*
factor ::= number | "(" expr ")"
number ::= [0-9]+
```

**Features:**
- **Rule Detection**: Identifies grammar rule definitions
- **Operator Highlighting**: Highlights GBNF operators (`::=`, `|`, `*`, `+`, `?`)
- **String Literals**: Recognizes quoted terminal symbols
- **Character Ranges**: Highlights bracket notation `[a-z]`
- **Comments**: Supports `//` line comments

### Shell/Bash Highlighter

**Languages**: `bash`, `shell`
**Purpose**: Command-line script highlighting with semantic understanding

```bash
#!/bin/bash
# Install dependencies
curl -sfL https://example.com/install.sh | tar -xzf -
dotnet build --configuration Release
```

**Features:**
- **Command Recognition**: Identifies shell commands and built-ins
- **Flag Highlighting**: Recognizes command-line flags (`-f`, `--verbose`)
- **String Detection**: Handles single and double-quoted strings
- **Comment Support**: Highlights `#` and `REM` comments
- **Shebang Support**: Recognizes script interpreters

### Plain Text Handler

**Languages**: `text`, `` (empty string)
**Purpose**: Renders code without syntax highlighting

Simply wraps content in `<pre><code>` tags with HTML escaping, providing a fallback for content that shouldn't be highlighted.

## Layer 3: TextMateSharp Integration

[TextMateSharp](https://github.com/danipen/TextMateSharp) provides broad language support using Visual Studio Code's grammar definitions, offering server-side highlighting for 49+ programming languages.

### Language Coverage

#### Web Technologies
- **Frontend**: HTML, CSS, SCSS, Less, JavaScript, TypeScript
- **Data**: JSON, XML, YAML
- **Templates**: Handlebars, Pug, Razor

#### Systems Programming
- **Native**: C, C++, Rust, Go, Swift, Objective-C
- **Managed**: Java, Kotlin, Dart, Pascal

#### Scripting & Dynamic
- **Python**: Full Python 3 syntax support
- **Ruby**: Including Rails-specific highlighting
- **PHP**: Web-focused syntax highlighting
- **PowerShell**: Windows scripting support
- **Lua**: Lightweight scripting language
- **R**: Statistical computing language

#### Functional Programming
- **F#**: .NET functional language
- **Clojure**: JVM-based Lisp dialect
- **Julia**: Scientific computing language

#### Documentation & Markup
- **Markdown**: Including extensions and frontmatter
- **LaTeX**: Mathematical typesetting
- **AsciiDoc**: Technical documentation
- **Typst**: Modern markup language

#### Specialized Languages
- **HLSL/ShaderLab**: Graphics programming
- **SQL**: Database queries
- **Groovy**: JVM scripting
- **Dockerfile**: Container definitions

### Fallback Behavior

When a requested language isn't found:
1. **Scope Name Generation**: Tries `source.{language}` pattern
2. **Alias Resolution**: Attempts common language aliases
3. **Plain Text Fallback**: Renders as plain code block if no grammar is found

## Layer 4: Client-Side Highlight.js (Fallback)

When server-side highlighting isn't available, the system falls back to client-side Highlight.js for maximum language compatibility.

### Pre-loaded Languages (23)

High-frequency languages loaded immediately:
- **Web**: `javascript`, `typescript`, `css`, `html`, `xml`, `json`, `yaml`
- **Systems**: `cpp`, `c`, `rust`, `go`, `java`, `kotlin`, `swift`
- **Scripting**: `python`, `php`, `ruby`, `bash`, `shell`
- **Data**: `sql`, `markdown`
- **.NET**: `csharp`

### Dynamic Loading

For uncommon languages, Highlight.js loads additional grammars from CDN:

```javascript
// Lazy load additional languages from CDN
const response = await fetch(`https://cdn.jsdelivr.net/npm/highlight.js@11/lib/languages/${language}.min.js`);
```

This provides access to 190+ languages without increasing initial bundle size.

## Summary

MyLittleContentEngine's syntax highlighting system provides comprehensive language support through a multi-layered architecture with 52+ server-side languages and 190+ client-side languages for maximum compatibility.