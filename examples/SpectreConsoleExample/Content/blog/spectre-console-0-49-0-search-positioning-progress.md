---
title: "Spectre.Console 0.49.0: Search Positioning Progress"
author: "Spectre.Console Team"
description: "A feature-packed release introducing search capabilities in selection prompts, flexible progress task positioning, enhanced exception formatting, and numerous UI improvements."
date: 2024-04-23
tags: ["release", "search", "progress", "ui"]
repository: "https://github.com/spectreconsole/spectre.console/releases/tag/0.49.0"
---

Spectre.Console 0.49.0: Search Positioning Progress

Spectre.Console 0.49.0 delivers a comprehensive set of new features and improvements that enhance user interaction, progress visualization, and developer experience. This release introduces powerful search capabilities, flexible progress task management, and numerous quality-of-life improvements across the entire library.

## What's New in 0.49.0

### Interactive Search in Selection Prompts

One of the most requested features is now available - search functionality in selection prompts:

- **Real-time search** in selection and multi-selection prompts (Stuart Lang)
- **Custom search logic** support for complex item types
- **Smooth filtering** that preserves user experience
- **Keyboard navigation** works seamlessly with search results

```csharp
var selection = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Choose your favorite framework:")
        .EnableSearch() // New feature!
        .AddChoices(frameworks));
```

### Flexible Progress Task Positioning

Progress displays now offer unprecedented flexibility:

- **Insert tasks before or after** existing tasks (Tom Longhurst)
- **Dynamic task ordering** during execution
- **Better visual organization** for complex progress scenarios

```csharp
await AnsiConsole.Progress()
    .StartAsync(async ctx =>
    {
        var task1 = ctx.AddTask("Download");
        var task2 = ctx.AddTaskBefore(task1, "Prepare"); // Insert before
        var task3 = ctx.AddTaskAfter(task1, "Process");  // Insert after
    });
```

### Enhanced Exception Handling

Exception formatting has been significantly improved:

- **NoStackTrace format option** for cleaner error displays (Gerardo Grignoli)
- **Better generic type formatting** with proper type name resolution (Cédric Luthi)
- **Improved error readability** across different scenarios

### Advanced CLI Features

The command-line framework received substantial upgrades:

- **Raw argument exposure** on command context for advanced scenarios (Patrik Svensson)
- **Token representation** for remaining arguments with better parsing (Patrik Svensson)
- **Automatic command settings registration** reducing boilerplate code (Patrik Svensson)
- **Multiple interceptor support** for complex command pipelines (Nils Andresen)
- **Type resolver integration** in exception handlers (Nils Andresen)

### Progress Bar Enhancements

Progress visualization got powerful new features:

- **Custom value formatters** for progress bars (Jonathan Sheely)
- **Deadlock prevention** when cancelling prompts (Caelan Sayler)
- **Better rendering** for odd page sizes in list prompts (Nils Andresen)

### User Interface Improvements

Numerous UI components received attention:

- **Configurable help provider colors** for better customization (Frank Ray)
- **Pipe character support** for listing command options (Frank Ray)
- **Backspace handling** improvements for secret prompts (Daniel Weber)
- **Better prompt validation** for multi-selection scenarios (Cédric Luthi)

### Internationalization Support

- **Multiple culture resource support** in help providers (Eduardo Tolino)
- **Improved localization** infrastructure for better international support

## Performance and Quality

- **Updated dependencies** to latest versions for better performance
- **Enhanced testing coverage** across all components
- **Build-time package optimization** for versioning and analysis tools (Chet Husk)
- **Line ending standardization** for better cross-platform compatibility (Nils Andresen)

## Breaking Changes

This release maintains backward compatibility while introducing new capabilities:

- **Search functionality** is opt-in and doesn't affect existing prompts
- **Progress task positioning** extends existing APIs without changes
- **CLI enhancements** are additive and maintain existing behavior

## Key Contributors

Special recognition for the outstanding contributors:

- **Stuart Lang**: Search functionality implementation
- **Tom Longhurst**: Progress task positioning features  
- **Frank Ray**: CLI improvements and help system enhancements
- **Patrik Svensson**: Core CLI framework improvements
- **Nils Andresen**: Multiple interceptor support and rendering fixes
- **Cédric Luthi**: Exception formatting and prompt validation
- **Gerardo Grignoli**: Exception handling improvements
- **Jonathan Sheely**: Progress bar value formatting

## Real-World Examples

### Searchable File Selection

```csharp
var files = Directory.GetFiles(".", "*.*", SearchOption.AllDirectories);
var selected = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Select a file to process:")
        .EnableSearch()
        .PageSize(20)
        .AddChoices(files.Select(Path.GetFileName)));
```

### Complex Progress Scenarios

```csharp
await AnsiConsole.Progress()
    .StartAsync(async ctx =>
    {
        var download = ctx.AddTask("Downloading");
        var prepare = ctx.AddTaskBefore(download, "Preparing");
        var process = ctx.AddTaskAfter(download, "Processing");
        
        // Tasks will display in order: Prepare → Download → Process
    });
```

## Get Started

Update your package reference:

```xml
<PackageReference Include="Spectre.Console" Version="0.49.0" />
```

This release significantly expands Spectre.Console's capabilities while maintaining the simplicity and elegance that developers love. The addition of search functionality and flexible progress management opens up new possibilities for creating sophisticated yet user-friendly console applications.

## Looking Ahead

Version 0.49.0 sets the stage for even more exciting features in upcoming releases. The enhanced CLI framework and improved UI components provide a solid foundation for the advanced features we're planning.

---

*Released on April 23, 2024*