namespace Pennington.ApiMetadata;

using System.Collections.Immutable;

/// <summary>Single member (property, field, method, constructor, or event) of a documented type, with all display strings pre-formatted by the backend provider.</summary>
/// <param name="Uid">Canonical xmldocid (e.g. <c>M:Namespace.Type.Method(System.Int32)</c>).</param>
/// <param name="Name">Display name of the member (for methods, includes type-parameter list; for constructors, the containing type name).</param>
/// <param name="Kind">The kind of member this record represents.</param>
/// <param name="TypeDisplay">Human-readable type signature (return type for methods, declared type for properties/fields/events).</param>
/// <param name="DefaultValue">Formatted default value literal, or <see langword="null"/> when none is declared.</param>
/// <param name="IsRequired">Whether the member carries the <c>required</c> modifier.</param>
/// <param name="HasInheritDocDirective">Whether the source xmldoc carried an <c>&lt;inheritdoc/&gt;</c> directive; consumers use this to suppress "missing summary" diagnostics when inheritance didn't resolve.</param>
/// <param name="Xmldoc">Parsed xmldoc for the member.</param>
/// <param name="SignatureHtml">Pre-highlighted declaration HTML ready to inject with <c>@((MarkupString)…)</c>, or <see langword="null"/> when no declaration signature is available.</param>
/// <param name="Parameters">Formatted parameter list for methods/constructors; empty for properties/fields/events.</param>
/// <param name="ReturnTypeDisplay">Formatted return type for methods that return a value; <see langword="null"/> otherwise.</param>
public sealed record ApiMember(
    string Uid,
    string Name,
    MemberKind Kind,
    string TypeDisplay,
    string? DefaultValue,
    bool IsRequired,
    bool HasInheritDocDirective,
    ParsedXmlDoc Xmldoc,
    string? SignatureHtml,
    ImmutableArray<ApiParameter> Parameters,
    string? ReturnTypeDisplay);
