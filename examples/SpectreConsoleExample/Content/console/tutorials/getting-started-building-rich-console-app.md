---
title: "Getting Started: Building a Rich Console App"
description: "A beginner-friendly tutorial that walks through creating a simple console application using Spectre.Console"
date: 2025-08-05
tags: ["tutorial", "getting-started", "console", "basics"]
section: "Console"
uid: "console-getting-started"
order: 1030
---

Welcome to Spectre.Console! This tutorial will guide you through creating your first rich console application. By the end, you'll have built a colorful, interactive console app that demonstrates the core features of Spectre.Console.

## What You'll Build

In this tutorial, you'll create a console application that showcases:
- Colorful text output using markup syntax
- Formatted data tables
- Advanced text styling with colors and decorations
- Interactive progress bars for long-running operations

## Prerequisites

- .NET 6.0 or later
- Basic C# knowledge
- A text editor or IDE (Visual Studio, VS Code, or JetBrains Rider)

## Installation

First, create a new console application and add the Spectre.Console NuGet package:

```bash
dotnet new console -n MySpectreApp
cd MySpectreApp
dotnet add package Spectre.Console
```

## Step 1: Hello World with Color

Let's start by replacing the default "Hello World" with something more colorful. Spectre.Console uses a simple markup syntax that makes it easy to add colors and styling to your text.

```csharp:xmldocid
M:Spectre.Console.Examples.Console.Tutorials.GettingStartedExample.ShowColoredHelloWorld
```

The markup syntax uses square brackets to define styling. For example:
- `[green]text[/]` makes text green
- `[red]text[/]` makes text red  
- `[bold]text[/]` makes text bold
- `[dim]text[/]` makes text dimmed

You can combine multiple styles and colors to create rich, readable console output without complex formatting code.

## Step 2: Displaying Data in Tables

Console applications often need to display structured data. Instead of manually formatting text into columns, Spectre.Console provides a powerful `Table` class that handles all the formatting for you.

```csharp:xmldocid
M:Spectre.Console.Examples.Console.Tutorials.GettingStartedExample.ShowDataTable
```

Tables automatically:
- Handle column alignment and spacing
- Draw borders using Unicode box-drawing characters
- Support colored headers and content
- Adapt to different console widths

The fluent API makes it easy to build tables step by step - add columns first, then populate with rows of data.

## Step 3: Advanced Text Styling

Beyond basic colors, Spectre.Console supports a wide range of text styling options. You can combine colors, backgrounds, and text decorations to create visually appealing output.

```csharp:xmldocid
M:Spectre.Console.Examples.Console.Tutorials.GettingStartedExample.ShowTextStyling
```

Key styling features include:

**Basic Colors**: red, green, blue, yellow, cyan, magenta, white, black
**Text Decorations**: bold, italic, underline, strikethrough, dim
**Background Colors**: Use `on` syntax like `[red on yellow]text[/]`
**Style Objects**: For programmatic styling when markup isn't suitable

The markup syntax is perfect for static text, while `Style` objects give you dynamic control when building styles at runtime.

## Step 4: Progress Bars for Long Tasks

Many console applications need to show progress for long-running operations. Spectre.Console provides beautiful, animated progress bars that keep users informed about task completion.

```csharp:xmldocid
M:Spectre.Console.Examples.Console.Tutorials.GettingStartedExample.ShowProgressBar
```

Progress bars in Spectre.Console:
- Support multiple concurrent tasks
- Automatically calculate percentages
- Show smooth animations
- Include customizable descriptions and colors
- Handle terminal resizing gracefully

The progress API uses a context pattern - you define tasks within a progress context, then update them as work completes.

## Running the Complete Example

The full example brings together all these concepts in a single demonstration:

```csharp:xmldocid
T:Spectre.Console.Examples.Console.Tutorials.GettingStartedExample
```

To run this example:

```bash
dotnet run getting-started
```

## Next Steps

Now that you've mastered the basics, you can explore more advanced Spectre.Console features:

- **Interactive Prompts**: Ask users for input with validation
- **Live Rendering**: Update console content in real-time
- **Charts and Diagrams**: Display data visualizations
- **Custom Renderables**: Create your own console widgets

Check out the other tutorials in this section to dive deeper into these powerful features.

## Key Takeaways

- **Markup syntax** makes text styling simple and readable
- **Tables** handle complex data formatting automatically  
- **Multiple styling options** let you create professional-looking output
- **Progress bars** keep users informed during long operations
- **Fluent APIs** make the code easy to write and understand

Spectre.Console transforms plain console applications into rich, interactive experiences that users will appreciate.