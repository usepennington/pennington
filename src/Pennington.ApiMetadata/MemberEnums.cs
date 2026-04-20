namespace Pennington.ApiMetadata;

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
