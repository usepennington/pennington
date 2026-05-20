namespace Pennington.Markdown.Shortcodes;

using FrontMatter;
using Routing;

/// <summary>Per-invocation context describing the page that hosts the shortcode call site.</summary>
/// <param name="Route">Route of the page being rendered.</param>
/// <param name="Metadata">Front matter of the page being rendered.</param>
public sealed record ShortcodeContext(ContentRoute Route, IFrontMatter Metadata);
