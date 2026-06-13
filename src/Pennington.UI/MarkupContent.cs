namespace Pennington.UI;

using Microsoft.AspNetCore.Components;

/// <summary>
/// A chrome content value that is either a raw HTML/markup string or a <see cref="RenderFragment"/>.
/// Implicitly converts from both, so consumers assign a string or a fragment directly; render it via
/// <see cref="Content"/>. Strings are emitted as raw markup, not markdown-processed.
/// </summary>
public readonly struct MarkupContent
{
    private readonly RenderFragment? _content;

    /// <summary>Wraps a raw HTML/markup string.</summary>
    public MarkupContent(string html) => _content = builder => builder.AddMarkupContent(0, html);

    /// <summary>Wraps a <see cref="RenderFragment"/>.</summary>
    public MarkupContent(RenderFragment fragment) => _content = fragment;

    /// <summary>The fragment that emits this content; a no-op when default-constructed.</summary>
    public RenderFragment Content => _content ?? (static _ => { });

    /// <summary>Converts a raw HTML/markup string.</summary>
    public static implicit operator MarkupContent(string html) => new(html);

    /// <summary>Converts a <see cref="RenderFragment"/>.</summary>
    public static implicit operator MarkupContent(RenderFragment fragment) => new(fragment);
}
