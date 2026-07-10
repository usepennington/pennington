using Beck;
using Beck.Rendering;

namespace Pennington.Beck;

/// <summary>Options for the <c>```beck</c> fence renderer registered by <see cref="BeckServiceExtensions.AddPenningtonBeck"/>.</summary>
public sealed class BeckOptions
{
    /// <summary>
    /// The base <see cref="SvgRenderOptions"/> applied to every fence render — measurer, fonts,
    /// theme, site-wide default style, custom style registry. <see cref="SvgRenderOptions.Animation"/>
    /// is overridden per fence by the <c>static</c>/<c>scrub</c> flags.
    /// </summary>
    public SvgRenderOptions RenderOptions { get; set; } = new();

    /// <summary>
    /// Root directory the file paths in a <c>```beck:symbol</c> fence body resolve against.
    /// Defaults to the working directory, matching the tree-sitter <c>:symbol</c> convention.
    /// </summary>
    public string ContentRoot { get; set; } = ".";

    /// <summary>
    /// Emits a fullscreen-zoom button into each rendered embed, backed by a small inline
    /// script and stylesheet contributed to the document head (the one piece of client
    /// JavaScript in this package — rendering stays server-side). Set <c>false</c> to keep
    /// embeds as bare SVG with no client behavior.
    /// </summary>
    public bool Zoom { get; set; } = true;
}
