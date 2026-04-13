namespace Pennington.Roslyn.Documentation;

using System.Collections.Immutable;

public record ParsedXmlDoc(
    ImmutableArray<XmlDocNode> Summary,
    ImmutableArray<XmlDocNode> Remarks,
    ImmutableDictionary<string, ImmutableArray<XmlDocNode>> Params,
    ImmutableDictionary<string, ImmutableArray<XmlDocNode>> TypeParams,
    ImmutableArray<XmlDocNode> Returns,
    ImmutableArray<XmlDocNode> Example,
    ImmutableArray<string> SeeAlso)
{
    public static ParsedXmlDoc Empty { get; } = new(
        Summary: [],
        Remarks: [],
        Params: ImmutableDictionary<string, ImmutableArray<XmlDocNode>>.Empty,
        TypeParams: ImmutableDictionary<string, ImmutableArray<XmlDocNode>>.Empty,
        Returns: [],
        Example: [],
        SeeAlso: []);

    public bool HasSummary => Summary.Length > 0;
    public bool HasRemarks => Remarks.Length > 0;
    public bool HasReturns => Returns.Length > 0;
    public bool HasExample => Example.Length > 0;
}
