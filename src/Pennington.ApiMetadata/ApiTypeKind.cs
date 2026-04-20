namespace Pennington.ApiMetadata;

/// <summary>Kind of a documented type.</summary>
public enum ApiTypeKind
{
    /// <summary>A reference type declared with <c>class</c>.</summary>
    Class,
    /// <summary>A value type declared with <c>struct</c>.</summary>
    Struct,
    /// <summary>An <c>interface</c> declaration.</summary>
    Interface,
    /// <summary>An <c>enum</c> declaration.</summary>
    Enum,
    /// <summary>A <c>record</c> or <c>record struct</c> declaration.</summary>
    Record,
    /// <summary>A <c>delegate</c> declaration.</summary>
    Delegate,
}
