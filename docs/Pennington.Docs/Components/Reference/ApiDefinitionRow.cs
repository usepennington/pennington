namespace Pennington.Docs.Components.Reference;

public sealed record ApiDefinitionRow(
    string Name,
    string TypeDisplay,
    string? DefaultValue,
    bool Required,
    string DescriptionHtml);
