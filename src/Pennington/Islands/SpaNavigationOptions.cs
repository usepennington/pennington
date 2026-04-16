namespace Pennington.Islands;

/// <summary>Configuration for SPA navigation.</summary>
public sealed class SpaNavigationOptions
{
    /// <summary>URL prefix under which SPA data endpoints are mounted.</summary>
    public string DataPath { get; set; } = "/_spa-data";
}