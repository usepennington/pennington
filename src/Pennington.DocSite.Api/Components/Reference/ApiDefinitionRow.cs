namespace Pennington.DocSite.Api.Components.Reference;

/// <summary>
/// One row in an <c>ApiDefinitionList</c>: a named definition with a type, an
/// optional default value, an optional "required" flag, and a description.
/// </summary>
public sealed record ApiDefinitionRow(
    string Name,
    string TypeDisplay,
    string? DefaultValue,
    bool Required,
    string DescriptionHtml);
