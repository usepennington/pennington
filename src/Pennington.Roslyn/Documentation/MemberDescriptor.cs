namespace Pennington.Roslyn.Documentation;

public enum MemberKind
{
    Properties,
    Fields,
    Methods,
    Constructors,
    Events,
    All,
}

public enum AccessFilter
{
    Public,
    Protected,
    PublicAndProtected,
}

public enum MemberOrder
{
    Alphabetical,
    Declaration,
}

public record MemberDescriptor(
    string Name,
    string XmlDocId,
    string TypeDisplay,
    string? DefaultValue,
    bool IsRequired,
    ParsedXmlDoc Xmldoc,
    MemberKind Kind);