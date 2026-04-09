# P0: HTML Encoding in Xref Resolution ✅ DONE

## Problem
`XrefResolvingService.ResolveXrefTagsAsync()` (`src/Penn/Infrastructure/XrefResolvingService.cs`, line 62) uses raw string interpolation to build `<a>` tags. If `xref.Title` or `xref.Route.CanonicalPath.Value` contains HTML-special characters (quotes, angle brackets, ampersands), the output HTML is malformed or potentially exploitable.

## Current State
- Line 62: `$"""<a href="{xref.Route.CanonicalPath.Value}">{xref.Title}</a>"""`
- Line 67: Same pattern for error case with `uid` interpolated into attributes
- The `ResolveXrefLinksAsync` method (line 76) uses AngleSharp DOM manipulation which is safe — only the string-based `ResolveXrefTagsAsync` path is affected
- Route canonical paths are internally generated (low risk), but titles come from front matter content (user-controlled)

## Requirements
- HTML-encode `xref.Title` when interpolated as element content (encode `<`, `>`, `&`)
- HTML-attribute-encode `xref.Route.CanonicalPath.Value` and `uid` when interpolated into attribute values (encode `"`, `<`, `>`, `&`)
- Use `System.Net.WebUtility.HtmlEncode` or equivalent — do not add new dependencies
- Add unit tests with titles containing: double quotes, angle brackets, ampersands, and single quotes
- Apply the same fix to the error-case replacement string on line 67

## Key Files
- `src/Penn/Infrastructure/XrefResolvingService.cs` — fix lines 62 and 67
- `tests/Penn.Tests/` — add encoding tests
