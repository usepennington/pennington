namespace Pennington.ApiMetadata;

using System.Collections.Immutable;

/// <summary>Full detail for a documented type, including parsed xmldoc and inheritance data, but not its members (those stream from <see cref="IApiMetadataProvider.GetMembersAsync"/>).</summary>
/// <param name="Summary">Header describing the type.</param>
/// <param name="Xmldoc">Parsed xmldoc for the type itself.</param>
/// <param name="SignatureHtml">Pre-highlighted declaration HTML, or <see langword="null"/> when not available.</param>
/// <param name="Inheritance">Base-type uids, most-derived first. Empty for interfaces and <c>object</c>.</param>
/// <param name="Implements">Implemented-interface uids.</param>
public sealed record ApiTypeDetail(
    ApiTypeSummary Summary,
    ParsedXmlDoc Xmldoc,
    string? SignatureHtml,
    ImmutableArray<string> Inheritance,
    ImmutableArray<string> Implements);