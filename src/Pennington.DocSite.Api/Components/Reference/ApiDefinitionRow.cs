namespace Pennington.DocSite.Api.Components.Reference;

/// <summary>
/// One row in an <c>ApiDefinitionList</c>: a named definition with a type, an
/// optional default value, an optional "required" flag, and a description.
/// </summary>
/// <param name="Name">Identifier shown in the Name column.</param>
/// <param name="TypeDisplay">Human-readable type, shown in the Type column.</param>
/// <param name="DefaultValue">Default value as it would appear in source, or <c>null</c> when no default applies.</param>
/// <param name="Required">When <c>true</c>, the row renders a "required" badge.</param>
/// <param name="DescriptionHtml">Pre-rendered HTML for the description column (already escaped).</param>
public sealed record ApiDefinitionRow(
    string Name,
    string TypeDisplay,
    string? DefaultValue,
    bool Required,
    string DescriptionHtml);
