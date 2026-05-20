namespace Pennington.Markdown.Shortcodes;

/// <summary>One parsed shortcode call site, supplied to <see cref="IShortcode.ExecuteAsync"/>.</summary>
/// <param name="PositionalArgs">Positional arguments in source order; empty when none were supplied.</param>
/// <param name="NamedArgs">Named (<c>key=value</c>) arguments; keys are case-insensitive.</param>
/// <param name="Content">Inline content between opener and closer; <see langword="null"/> for self-closing tags.</param>
public sealed record ShortcodeInvocation(
    IReadOnlyList<string> PositionalArgs,
    IReadOnlyDictionary<string, string> NamedArgs,
    string? Content);
