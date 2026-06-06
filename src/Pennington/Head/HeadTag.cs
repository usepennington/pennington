namespace Pennington.Head;

using System.Collections.Immutable;

/// <summary>The document <c>&lt;title&gt;</c>. Deduplicated to exactly one.</summary>
/// <param name="Text">Title text.</param>
public sealed record TitleTag(string Text);

/// <summary>A <c>&lt;meta name="..." content="..."&gt;</c> tag (description, <c>twitter:*</c>, generator).</summary>
/// <param name="Name">The <c>name</c> attribute.</param>
/// <param name="Content">The <c>content</c> attribute.</param>
public sealed record MetaNameTag(string Name, string Content);

/// <summary>A <c>&lt;meta property="..." content="..."&gt;</c> tag (OpenGraph <c>og:*</c>).</summary>
/// <param name="Property">The <c>property</c> attribute.</param>
/// <param name="Content">The <c>content</c> attribute.</param>
public sealed record MetaPropertyTag(string Property, string Content);

/// <summary>A <c>&lt;link&gt;</c> tag (canonical, alternate, stylesheet, preload, verification).</summary>
/// <param name="Rel">The <c>rel</c> attribute.</param>
/// <param name="Href">The <c>href</c> attribute.</param>
public sealed record LinkTag(string Rel, string Href)
{
    /// <summary>Extra attributes in emission order (e.g. <c>type</c>, <c>title</c>, <c>hreflang</c>, <c>as</c>, <c>crossorigin</c>).</summary>
    public ImmutableArray<KeyValuePair<string, string?>> Attributes { get; init; } = [];
}

/// <summary>A <c>&lt;script&gt;</c> tag — JSON-LD, a deferred asset, or an inline bootstrap.</summary>
public sealed record ScriptTag
{
    /// <summary>External script URL; mutually exclusive with <see cref="InlineBody"/>.</summary>
    public string? Src { get; init; }

    /// <summary>Inline script body (e.g. a JSON-LD payload or a theme bootstrap).</summary>
    public string? InlineBody { get; init; }

    /// <summary>The <c>type</c> attribute (e.g. <c>application/ld+json</c>).</summary>
    public string? Type { get; init; }

    /// <summary>Emit a <c>defer</c> attribute on an external script.</summary>
    public bool Defer { get; init; }
}

/// <summary>An escape hatch carrying arbitrary head markup verbatim (e.g. <c>AdditionalHtmlHeadContent</c>).</summary>
/// <param name="Html">Raw HTML inserted into the head as-is.</param>
public sealed record RawTag(string Html);

/// <summary>The set of elements a contributor can place in the document <c>&lt;head&gt;</c>.</summary>
#if NET11_0_OR_GREATER
public union HeadTag(TitleTag, MetaNameTag, MetaPropertyTag, LinkTag, ScriptTag, RawTag);
#else
[System.Runtime.CompilerServices.Union]
public readonly struct HeadTag : System.Runtime.CompilerServices.IUnion
{
    /// <summary>Wrapped case instance; inspect via pattern matching on the case types.</summary>
    public object? Value { get; }
    /// <summary>Wraps a <see cref="TitleTag"/>.</summary>
    public HeadTag(TitleTag value) { Value = value; }
    /// <summary>Wraps a <see cref="MetaNameTag"/>.</summary>
    public HeadTag(MetaNameTag value) { Value = value; }
    /// <summary>Wraps a <see cref="MetaPropertyTag"/>.</summary>
    public HeadTag(MetaPropertyTag value) { Value = value; }
    /// <summary>Wraps a <see cref="LinkTag"/>.</summary>
    public HeadTag(LinkTag value) { Value = value; }
    /// <summary>Wraps a <see cref="ScriptTag"/>.</summary>
    public HeadTag(ScriptTag value) { Value = value; }
    /// <summary>Wraps a <see cref="RawTag"/>.</summary>
    public HeadTag(RawTag value) { Value = value; }
    /// <summary>Implicit conversion from <see cref="TitleTag"/>.</summary>
    public static implicit operator HeadTag(TitleTag value) => new(value);
    /// <summary>Implicit conversion from <see cref="MetaNameTag"/>.</summary>
    public static implicit operator HeadTag(MetaNameTag value) => new(value);
    /// <summary>Implicit conversion from <see cref="MetaPropertyTag"/>.</summary>
    public static implicit operator HeadTag(MetaPropertyTag value) => new(value);
    /// <summary>Implicit conversion from <see cref="LinkTag"/>.</summary>
    public static implicit operator HeadTag(LinkTag value) => new(value);
    /// <summary>Implicit conversion from <see cref="ScriptTag"/>.</summary>
    public static implicit operator HeadTag(ScriptTag value) => new(value);
    /// <summary>Implicit conversion from <see cref="RawTag"/>.</summary>
    public static implicit operator HeadTag(RawTag value) => new(value);
}
#endif
