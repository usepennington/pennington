namespace Pennington.Roslyn.Documentation;

using System.Collections.Immutable;

public record TextNode(string Text);
public record InlineCodeNode(string Text);
public record CodeBlockNode(string Language, string Text);
public record ParaNode(ImmutableArray<XmlDocNode> Children);
public record CrefNode(string CrefId, string? DisplayText);
public record ParamRefNode(string ParamName);
public record TypeParamRefNode(string ParamName);
public record ListNode(string Kind, ImmutableArray<XmlDocListItem> Items);
public record XmlDocListItem(ImmutableArray<XmlDocNode> Term, ImmutableArray<XmlDocNode> Description);

public union XmlDocNode(
    TextNode,
    InlineCodeNode,
    CodeBlockNode,
    ParaNode,
    CrefNode,
    ParamRefNode,
    TypeParamRefNode,
    ListNode);
