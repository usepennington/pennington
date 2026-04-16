namespace Pennington.Roslyn.Documentation;

/// <summary>Kind of type member to include when enumerating members for documentation rendering.</summary>
public enum MemberKind
{
    /// <summary>Properties declared on the type.</summary>
    Properties,
    /// <summary>Fields declared on the type.</summary>
    Fields,
    /// <summary>Methods declared on the type (excluding constructors).</summary>
    Methods,
    /// <summary>Constructors declared on the type.</summary>
    Constructors,
    /// <summary>Events declared on the type.</summary>
    Events,
    /// <summary>All kinds of members.</summary>
    All,
}

/// <summary>Accessibility filter applied when enumerating members.</summary>
public enum AccessFilter
{
    /// <summary>Include only public members.</summary>
    Public,
    /// <summary>Include only protected members.</summary>
    Protected,
    /// <summary>Include public and protected members.</summary>
    PublicAndProtected,
}

/// <summary>Ordering used when rendering a list of members.</summary>
public enum MemberOrder
{
    /// <summary>Sort members alphabetically by name.</summary>
    Alphabetical,
    /// <summary>Preserve the declaration order from source.</summary>
    Declaration,
}

/// <summary>Describes a single type member resolved for documentation rendering.</summary>
/// <param name="Name">Display name of the member.</param>
/// <param name="XmlDocId">Roslyn xmldocid (e.g. <c>M:Ns.Type.Method(System.Int32)</c>) that uniquely identifies the member.</param>
/// <param name="TypeDisplay">Human-readable type signature (return type for methods, declared type for properties/fields).</param>
/// <param name="DefaultValue">Formatted default value when the member declares one; otherwise <see langword="null"/>.</param>
/// <param name="IsRequired">Whether the member is marked <c>required</c>.</param>
/// <param name="Xmldoc">Parsed xmldoc content for the member.</param>
/// <param name="Kind">The kind of member this descriptor represents.</param>
public record MemberDescriptor(
    string Name,
    string XmlDocId,
    string TypeDisplay,
    string? DefaultValue,
    bool IsRequired,
    ParsedXmlDoc Xmldoc,
    MemberKind Kind);
