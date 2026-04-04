---
title: "Spectre.Console 0.48.0: NET8 Localization Customization"
author: "Spectre.Console Team"
description: "A significant release bringing .NET 8.0 support, comprehensive localization capabilities, extensive customization options, and enhanced table rendering with new async command functionality."
date: 2023-11-20
tags: ["release", "net8", "localization", "customization"]
repository: "https://github.com/spectreconsole/spectre.console/releases/tag/0.48.0"
---

Spectre.Console 0.48.0: NET8 Localization Customization

Spectre.Console 0.48.0 represents a major milestone with the introduction of .NET 8.0 support, comprehensive localization infrastructure, and extensive customization capabilities. This release significantly expands the framework's flexibility while maintaining the clean, intuitive API that developers love.

## What's New in 0.48.0

### .NET 8.0 Support

The headline feature of this release is full .NET 8.0 support:

- **.NET 8.0 target framework** added for optimal performance on the latest runtime (Patrik Svensson)
- **Maintained backward compatibility** with existing .NET versions
- **Enhanced build pipeline** to support multiple framework targets (Patrik Svensson)
- **Updated dependencies** to leverage .NET 8.0 improvements

### Comprehensive Localization Support

Internationalization becomes a first-class citizen in Spectre.Console:

- **Localization infrastructure** for help providers enabling multi-language CLI applications (Frank Ray)
- **Custom help provider support** allowing complete customization of help text generation (Frank Ray)
- **Flexible resource management** for different cultures and regions
- **Extensible translation system** for custom command applications

### Enhanced Table Rendering

Table components received significant improvements:

- **Row separators** can now be displayed between table rows for better visual organization (Patrik Svensson)
- **Zero-width column support** for tables with conditional columns (Fraser Waters)
- **Better column measurement** with greedy sizing for optimal space utilization (Nils Andresen)
- **Improved rendering** for edge cases and complex table scenarios

### Advanced Customization Options

This release dramatically expands customization capabilities:

- **Custom confirmation prompt styling** with full control over appearance (Will Baldoumas)
- **Breakdown chart color customization** for data visualization components (Nils Andresen)
- **Progress bar headers and footers** for enhanced progress displays (Phil Scott)
- **Nullable style support** in default value and choices styling (Cédric Luthi)

### Async Command Framework

Command execution becomes more powerful with async support:

- **AddAsyncDelegate functionality** for asynchronous command handlers (Ignacio Calvo)
- **Comprehensive async command testing** to ensure reliability (Frank Ray)
- **Better exception handling** in asynchronous scenarios
- **Improved performance** for I/O bound command operations

### Text and Layout Improvements

Various text rendering and layout issues were addressed:

- **TextPath rendering fixes** for better path display (Patrik Svensson)
- **Figlet centering improvements** preventing layout exceptions (Ola Bäcker)
- **Safe height calculations** for console output (Cédric Luthi)
- **Row measurement optimizations** for complex layouts (Nils Andresen)

### Developer Experience Enhancements

- **Better default values** for FileInfo and DirectoryInfo parameters (Cédric Luthi)
- **Improved error handling** with standardized exception handlers (Nils Andresen)
- **Enhanced fake type registrar** for better testing support (Nils Andresen)
- **Argument vector settings** for more control over command parsing (Nils Andresen)

## Breaking Changes

While maintaining strong backward compatibility, some changes may affect advanced users:

- **Help provider interface** has been extended for localization support
- **Exception handler naming** has been standardized (SetErrorHandler → SetExceptionHandler)
- **Type registrar behavior** has been refined for better dependency injection

## Key Contributors

Special thanks to the amazing contributors who made this release possible:

- **Frank Ray**: Localization infrastructure and help provider enhancements
- **Patrik Svensson**: .NET 8.0 support and core framework improvements
- **Nils Andresen**: Table rendering, type registration, and testing improvements
- **Cédric Luthi**: Console output safety and styling enhancements
- **Will Baldoumas**: Confirmation prompt styling
- **Phil Scott**: Progress bar header/footer functionality
- **Fraser Waters**: Zero-width column support and table improvements
- **Ignacio Calvo**: Async delegate functionality

## Real-World Examples

### Localized CLI Application

```csharp
var app = new CommandApp();
app.Configure(config =>
{
    config.Settings.HelpProviderFactory = () => new LocalizedHelpProvider("es-ES");
});
```

### Enhanced Table with Separators

```csharp
var table = new Table()
    .AddColumn("Name")
    .AddColumn("Value")
    .ShowRowSeparators() // New feature
    .AddRow("Item 1", "Value 1")
    .AddRow("Item 2", "Value 2");
```

### Async Command Handler

```csharp
app.Configure(config =>
{
    config.AddAsyncDelegate<Settings>("process", async (context, settings) =>
    {
        await ProcessDataAsync(settings.InputFile);
        return 0;
    });
});
```

## Performance Improvements

- **Optimized table rendering** for large datasets
- **Improved memory usage** in console output scenarios
- **Better async performance** with proper Task handling
- **Reduced allocations** in hot paths

## Get Started

Update your package reference:

```xml
<PackageReference Include="Spectre.Console" Version="0.48.0" />
```

For applications targeting .NET 8.0, you'll benefit from improved performance and the latest runtime optimizations while maintaining full compatibility with earlier versions.

## Migration Guide

Most applications will upgrade seamlessly. Key considerations:

1. **Help providers**: If you've customized help generation, review the new localization APIs
2. **Exception handlers**: Update any custom error handlers to use the new naming convention
3. **.NET 8.0**: Consider upgrading your target framework for optimal performance

## Community Impact

This release demonstrates the vibrant Spectre.Console community with contributions spanning internationalization, async programming, and advanced customization. The addition of comprehensive localization support opens the library to a truly global audience.

## Looking Forward

Version 0.48.0 establishes a solid foundation for international applications and sets the stage for even more advanced features in upcoming releases. The combination of .NET 8.0 support, localization infrastructure, and enhanced customization capabilities makes this one of the most significant releases in Spectre.Console's history.

---

*Released on November 20, 2023*