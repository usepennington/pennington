namespace Pennington.Roslyn.Documentation;

using System.Collections.Immutable;

/// <summary>Structured representation of a parsed xmldoc comment split into its standard sections.</summary>
/// <param name="Summary">Nodes from the <c>&lt;summary&gt;</c> element.</param>
/// <param name="Remarks">Nodes from the <c>&lt;remarks&gt;</c> element.</param>
/// <param name="Params">Per-parameter nodes keyed by parameter name, from <c>&lt;param&gt;</c> elements.</param>
/// <param name="TypeParams">Per-type-parameter nodes keyed by name, from <c>&lt;typeparam&gt;</c> elements.</param>
/// <param name="Returns">Nodes from the <c>&lt;returns&gt;</c> element.</param>
/// <param name="Example">Nodes from the <c>&lt;example&gt;</c> element.</param>
/// <param name="SeeAlso">Cref values collected from <c>&lt;seealso&gt;</c> elements.</param>
public record ParsedXmlDoc(
    ImmutableArray<XmlDocNode> Summary,
    ImmutableArray<XmlDocNode> Remarks,
    ImmutableDictionary<string, ImmutableArray<XmlDocNode>> Params,
    ImmutableDictionary<string, ImmutableArray<XmlDocNode>> TypeParams,
    ImmutableArray<XmlDocNode> Returns,
    ImmutableArray<XmlDocNode> Example,
    ImmutableArray<string> SeeAlso)
{
    /// <summary>Shared empty instance used when no xmldoc is available.</summary>
    public static ParsedXmlDoc Empty { get; } = new(
        Summary: [],
        Remarks: [],
        Params: ImmutableDictionary<string, ImmutableArray<XmlDocNode>>.Empty,
        TypeParams: ImmutableDictionary<string, ImmutableArray<XmlDocNode>>.Empty,
        Returns: [],
        Example: [],
        SeeAlso: []);

    /// <summary>True when <see cref="Summary"/> contains any nodes.</summary>
    public bool HasSummary => Summary.Length > 0;
    /// <summary>True when <see cref="Remarks"/> contains any nodes.</summary>
    public bool HasRemarks => Remarks.Length > 0;
    /// <summary>True when <see cref="Returns"/> contains any nodes.</summary>
    public bool HasReturns => Returns.Length > 0;
    /// <summary>True when <see cref="Example"/> contains any nodes.</summary>
    public bool HasExample => Example.Length > 0;
}