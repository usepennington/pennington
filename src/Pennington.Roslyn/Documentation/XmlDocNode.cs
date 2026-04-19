namespace Pennington.Roslyn.Documentation;

using System.Collections.Immutable;

/// <summary>Literal text content within an xmldoc node tree.</summary>
/// <param name="Text">The text content.</param>
public record TextNode(string Text);

/// <summary>Inline code span from a <c>&lt;c&gt;</c> element.</summary>
/// <param name="Text">The code text.</param>
public record InlineCodeNode(string Text);

/// <summary>Fenced code block from a <c>&lt;code&gt;</c> element.</summary>
/// <param name="Language">Language identifier used by the code fence (e.g. <c>csharp</c>).</param>
/// <param name="Text">The code content with leading indentation stripped.</param>
public record CodeBlockNode(string Language, string Text);

/// <summary>Paragraph block from a <c>&lt;para&gt;</c> element.</summary>
/// <param name="Children">Inline nodes contained within the paragraph.</param>
public record ParaNode(ImmutableArray<XmlDocNode> Children);

/// <summary>Cross-reference from a <c>&lt;see cref="..."/&gt;</c> or <c>&lt;see href="..."/&gt;</c> element.</summary>
/// <param name="CrefId">The raw cref id or href value.</param>
/// <param name="DisplayText">Optional display text from the element body.</param>
public record CrefNode(string CrefId, string? DisplayText);

/// <summary>Parameter reference from a <c>&lt;paramref name="..."/&gt;</c> element.</summary>
/// <param name="ParamName">Name of the referenced parameter.</param>
public record ParamRefNode(string ParamName);

/// <summary>Type parameter reference from a <c>&lt;typeparamref name="..."/&gt;</c> element.</summary>
/// <param name="ParamName">Name of the referenced type parameter.</param>
public record TypeParamRefNode(string ParamName);

/// <summary>List block from a <c>&lt;list&gt;</c> element.</summary>
/// <param name="Kind">List type attribute value (e.g. <c>bullet</c>, <c>number</c>, <c>table</c>).</param>
/// <param name="Items">Items contained in the list.</param>
public record ListNode(string Kind, ImmutableArray<XmlDocListItem> Items);

/// <summary>Single item within a <see cref="ListNode"/>.</summary>
/// <param name="Term">Nodes from the <c>&lt;term&gt;</c> element.</param>
/// <param name="Description">Nodes from the <c>&lt;description&gt;</c> element (or the item body when no description element is present).</param>
public record XmlDocListItem(ImmutableArray<XmlDocNode> Term, ImmutableArray<XmlDocNode> Description);

/// <summary>Discriminated union of node kinds that make up a parsed xmldoc tree.</summary>
#if NET11_0_OR_GREATER
public union XmlDocNode(
    TextNode,
    InlineCodeNode,
    CodeBlockNode,
    ParaNode,
    CrefNode,
    ParamRefNode,
    TypeParamRefNode,
    ListNode);
#else
[System.Runtime.CompilerServices.Union]
public readonly struct XmlDocNode : System.Runtime.CompilerServices.IUnion
{
    /// <summary>Wrapped case instance; inspect via pattern matching on the case types.</summary>
    public object? Value { get; }
    /// <summary>Wraps a <see cref="TextNode"/>.</summary>
    public XmlDocNode(TextNode value) { Value = value; }
    /// <summary>Wraps an <see cref="InlineCodeNode"/>.</summary>
    public XmlDocNode(InlineCodeNode value) { Value = value; }
    /// <summary>Wraps a <see cref="CodeBlockNode"/>.</summary>
    public XmlDocNode(CodeBlockNode value) { Value = value; }
    /// <summary>Wraps a <see cref="ParaNode"/>.</summary>
    public XmlDocNode(ParaNode value) { Value = value; }
    /// <summary>Wraps a <see cref="CrefNode"/>.</summary>
    public XmlDocNode(CrefNode value) { Value = value; }
    /// <summary>Wraps a <see cref="ParamRefNode"/>.</summary>
    public XmlDocNode(ParamRefNode value) { Value = value; }
    /// <summary>Wraps a <see cref="TypeParamRefNode"/>.</summary>
    public XmlDocNode(TypeParamRefNode value) { Value = value; }
    /// <summary>Wraps a <see cref="ListNode"/>.</summary>
    public XmlDocNode(ListNode value) { Value = value; }
    /// <summary>Implicit conversion from <see cref="TextNode"/>.</summary>
    public static implicit operator XmlDocNode(TextNode value) => new(value);
    /// <summary>Implicit conversion from <see cref="InlineCodeNode"/>.</summary>
    public static implicit operator XmlDocNode(InlineCodeNode value) => new(value);
    /// <summary>Implicit conversion from <see cref="CodeBlockNode"/>.</summary>
    public static implicit operator XmlDocNode(CodeBlockNode value) => new(value);
    /// <summary>Implicit conversion from <see cref="ParaNode"/>.</summary>
    public static implicit operator XmlDocNode(ParaNode value) => new(value);
    /// <summary>Implicit conversion from <see cref="CrefNode"/>.</summary>
    public static implicit operator XmlDocNode(CrefNode value) => new(value);
    /// <summary>Implicit conversion from <see cref="ParamRefNode"/>.</summary>
    public static implicit operator XmlDocNode(ParamRefNode value) => new(value);
    /// <summary>Implicit conversion from <see cref="TypeParamRefNode"/>.</summary>
    public static implicit operator XmlDocNode(TypeParamRefNode value) => new(value);
    /// <summary>Implicit conversion from <see cref="ListNode"/>.</summary>
    public static implicit operator XmlDocNode(ListNode value) => new(value);
}
#endif
