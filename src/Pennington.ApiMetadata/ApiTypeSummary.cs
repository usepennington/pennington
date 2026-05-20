namespace Pennington.ApiMetadata;

/// <summary>Lightweight header describing a documented type, used for listings, slug disambiguation, and cross-link display names.</summary>
/// <param name="Uid">Canonical xmldocid (e.g. <c>T:Namespace.TypeName</c>). Normalized to xmldocid form regardless of source backend.</param>
/// <param name="Name">Short type name without namespace (e.g. <c>ContentPipeline</c>).</param>
/// <param name="Namespace">Fully-qualified containing namespace, empty for the global namespace.</param>
/// <param name="Assembly">Declaring assembly name without extension.</param>
/// <param name="Kind">Category of the type.</param>
/// <param name="Summary">First-sentence plain-text summary, or <see langword="null"/> when no xmldoc summary is available.</param>
public sealed record ApiTypeSummary(
    string Uid,
    string Name,
    string Namespace,
    string Assembly,
    ApiTypeKind Kind,
    string? Summary)
{
    /// <summary>Fully-qualified type name (namespace + dot + type name).</summary>
    public string FullTypeName =>
        string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";
}