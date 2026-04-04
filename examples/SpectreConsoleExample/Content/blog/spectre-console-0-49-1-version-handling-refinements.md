---
title: "Spectre.Console 0.49.1: Version Handling Refinements"
author: "Spectre.Console Team"
description: "A focused patch release that refines version flag handling with new opt-in controls and automatic application version detection for better CLI experiences."
date: 2024-04-25
tags: ["release", "patch", "version", "cli"]
repository: "https://github.com/spectreconsole/spectre.console/releases/tag/0.49.1"
---

Spectre.Console 0.49.1: Version Handling Refinements

Spectre.Console 0.49.1 is a targeted patch release that focuses on refining version handling behavior in CLI applications. Based on community feedback from the 0.49.0 release, this update provides better control over version flag display and introduces convenient extension methods for automatic version detection.

## What's New in 0.49.1

### Enhanced Version Flag Control

The main focus of this release is improving how version flags (`-v|--version`) are handled in CLI applications:

- **Opt-in version flags**: Version flags are now opt-in rather than automatic, giving developers more control (Patrik Svensson)
- **Automatic application version detection**: New extension method to automatically use assembly version information (Patrik Svensson)
- **Improved tooling support**: Added Verify.Tool as a dotnet tool for better development workflows (Patrik Svensson)

### Documentation and Branding Updates

- **0.49 release blog post**: Comprehensive documentation of the previous release features (Patrik Svensson)
- **Updated social media cards**: Now showcase .NET 8.0 support prominently (Patrik Svensson)

## Key Changes

### Version Flag Behavior

Previously, version flags were automatically available for all CLI applications. This release makes them opt-in:

```csharp
// Old behavior (automatic)
var app = new CommandApp();
// -v|--version was always available

// New behavior (explicit control)
var app = new CommandApp();
app.Configure(config =>
{
    config.UseApplicationVersion(); // Now opt-in
});
```

### Automatic Version Detection

For applications that want to use their assembly version automatically:

```csharp
var app = new CommandApp();
app.Configure(config =>
{
    config.UseAutomaticApplicationVersion(); // Uses assembly version
});
```

This is particularly useful for applications using automated versioning tools like GitVersion, MinVer, or similar.

## Why This Change?

The shift to opt-in version handling addresses several developer concerns:

1. **Cleaner help output**: Applications that don't need version flags won't show them unnecessarily
2. **Better control**: Developers can choose exactly when and how version information is displayed
3. **Consistency**: Aligns with the principle that CLI features should be explicit rather than implicit

## Migration Guide

If your application relied on automatic version flags in 0.49.0, you'll need to explicitly enable them:

```csharp
// Add this to your CommandApp configuration
app.Configure(config =>
{
    config.UseApplicationVersion(); // Restore previous behavior
    
    // Or use automatic detection
    config.UseAutomaticApplicationVersion(); // Uses assembly info
});
```

## Performance and Tooling

- **Development tools**: Added Verify.Tool for improved snapshot testing workflows
- **Documentation pipeline**: Enhanced build process for better documentation generation

## Get Started

Update your package reference:

```xml
<PackageReference Include="Spectre.Console" Version="0.49.1" />
```

This patch release ensures that the version handling improvements introduced in 0.49.0 work exactly as intended, providing the right balance of convenience and control for CLI application developers.

## Community Impact

This release demonstrates our commitment to responsive development based on community feedback. The quick turnaround on refining version handling shows how valuable community input is in shaping Spectre.Console's evolution.

---

*Released on April 25, 2024*