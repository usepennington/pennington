namespace ExtensibilityLabExample;

using ExtensibilityLabExample.Components;
using Pennington.Islands;
using Pennington.Routing;

/// <summary>
/// Implements <see cref="IIslandRenderer"/> by subclassing
/// <see cref="RazorIslandRenderer{TComponent}"/>. Renders the
/// <see cref="ChartIsland"/> Razor component for the
/// <c>data-spa-island="chart"</c> slot on any content page that carries
/// one.
/// <para>
/// Registered via
/// <c>options.Islands.Register&lt;ChartIslandRenderer&gt;("chart")</c>
/// in <c>Program.cs</c>. The name string matches the <c>data-spa-island</c>
/// attribute value; <see cref="IslandName"/> exposes the same value so
/// <c>SpaPageDataService</c> can key the JSON envelope.
/// </para>
/// <para>
/// <see cref="BuildParametersAsync"/> returns a parameter dictionary
/// for every route under <c>/chart-demo/</c> (or the per-release pages)
/// and returns <see langword="null"/> everywhere else — a null return
/// tells the base class to skip this island for that page.
/// </para>
/// <para>
/// Backs how-to 2.3.60 <c>/how-to/extensibility/island-renderer</c>.
/// </para>
/// </summary>
public sealed class ChartIslandRenderer : RazorIslandRenderer<ChartIsland>
{
    public ChartIslandRenderer(ComponentRenderer renderer) : base(renderer) { }

    public override string IslandName => "chart";

    protected override Task<IDictionary<string, object?>?> BuildParametersAsync(ContentRoute route)
    {
        // Only render a chart on pages that advertise one. Saves work on
        // every other page in the site.
        var path = route.CanonicalPath.Value;
        if (!path.Contains("/chart-demo", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<IDictionary<string, object?>?>(null);

        IDictionary<string, object?> parameters = new Dictionary<string, object?>
        {
            ["Label"] = "Quarterly widgets",
            ["Values"] = new[] { 12, 19, 7, 24 },
        };
        return Task.FromResult<IDictionary<string, object?>?>(parameters);
    }
}
