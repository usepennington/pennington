namespace Pennington.ApiMetadata;

/// <summary>One public static extension method discovered in a workspace assembly, projected for reference-doc rendering.</summary>
/// <param name="Name">Short method name (no parameter list, no enclosing type).</param>
/// <param name="Signature">Full C# signature including return type and parameter list.</param>
/// <param name="Package">Owning assembly name, used as the package label on the rendered page.</param>
/// <param name="Uid">Canonical xmldocid (<c>M:...</c>) of the method.</param>
/// <param name="ReceiverTypeName">Unqualified short name of the first (receiver) parameter's type, used as the grouping key.</param>
/// <param name="Xmldoc">Parsed xmldoc for the method, with summary/remarks/returns/etc.</param>
public sealed record ExtensionMethodEntry(
    string Name,
    string Signature,
    string Package,
    string Uid,
    string ReceiverTypeName,
    ParsedXmlDoc Xmldoc);