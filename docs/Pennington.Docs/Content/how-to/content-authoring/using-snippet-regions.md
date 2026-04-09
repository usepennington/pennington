---
title: "Using Snippet Regions in Code Blocks"
description: "Show only relevant portions of a code file using include and exclude region directives"
uid: "penn.how-to.using-snippet-regions"
order: 16
---

## Beat 1: Include Regions

You want to show a specific method or section from a larger source file without displaying boilerplate like using statements or namespace declarations.

Use `// [!code include-start]` and `// [!code include-end]` markers to show only the lines between the markers.

### What to show
- A code block containing a full C# file with `// [!code include-start]` and `// [!code include-end]` markers around the setup method -- only the lines between the markers appear in the rendered output
- Reference `CodeTransformer.IsSnippetDirective` which recognizes `include-start`, `include-end`, `exclude-start`, `exclude-end`
- Reference `CodeTransformer.ValidateAndBuildSnippetRegions` which pairs start/end markers into `SnippetRegion` records with `SnippetRegionType.Include`
- Reference `CodeTransformer.DetermineLinesToRemove` which calculates which lines to strip: for include regions, everything outside the region is removed

### Key points
- The directive marker lines themselves are always removed from output
- Include and exclude regions cannot be nested (the validator returns `IsValid = false` for nested regions)
- After line removal, `CodeTransformer.NormalizeLineIndents` re-normalizes indentation so the output does not have excessive leading whitespace

## Beat 2: Exclude Regions

Use `// [!code exclude-start]` and `// [!code exclude-end]` to hide boilerplate while showing the rest of the file. This is the inverse of include regions.

### What to show
- A code block using `// [!code exclude-start]` and `// [!code exclude-end]` to hide boilerplate (using statements, namespace declaration) while showing the rest
- Reference `CodeTransformer.ValidateAndBuildSnippetRegions` which pairs start/end markers into `SnippetRegion` records with `SnippetRegionType.Exclude`
- Reference `CodeTransformer.DetermineLinesToRemove` which calculates which lines to strip: for exclude regions, only the marked range is removed

### Key points
- Exclude regions remove only the marked range and the directive lines, leaving everything else visible
- Multiple exclude regions can be used in the same code block to hide several separate sections
- Include and exclude regions can technically appear in the same code block (include regions determine visible lines first, then exclude regions remove additional ranges), but mixing them is not recommended as the interaction can be confusing

## Beat 3: Combining Snippet Regions with Other Directives

Snippet regions compose with highlight, focus, diff, and other line directives. After region extraction, line numbers are recalculated for remaining directives.

### What to show
- A code block with an include region that also uses `[!code highlight]` on key lines inside the region
- After line removal, `CodeTransformer.AdjustTransformationsAfterLineRemoval` recalculates line numbers for any remaining highlight/focus/diff directives
- `CodeTransformer.NormalizeLineIndents` re-normalizes indentation after snippet extraction so the output does not have excessive leading whitespace

### Key points
- Snippet regions compose with other directives: you can have `[!code highlight]` inside an include region
- Line number recalculation ensures directives remain accurate after lines are removed
- The indentation normalization step removes common leading whitespace that results from extracting an indented region
