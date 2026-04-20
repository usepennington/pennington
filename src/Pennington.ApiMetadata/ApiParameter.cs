namespace Pennington.ApiMetadata;

using System.Collections.Immutable;

/// <summary>One parameter of a method or constructor, formatted for display.</summary>
/// <param name="Name">Parameter name as declared.</param>
/// <param name="TypeDisplay">Fully-formatted type display including ref/out/in prefixes and optional markers.</param>
/// <param name="Description">Parsed xmldoc nodes from the matching <c>&lt;param&gt;</c> element.</param>
public sealed record ApiParameter(
    string Name,
    string TypeDisplay,
    ImmutableArray<XmlDocNode> Description);
