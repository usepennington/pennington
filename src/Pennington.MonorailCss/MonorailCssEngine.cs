using MonorailCss;

namespace Pennington.MonorailCss;

/// <summary>
/// Owning handle for the <see cref="CssFramework"/> instance Pennington configures from
/// <see cref="MonorailCssOptions"/>. Pennington's internals (the stylesheet endpoint, the
/// discovery integration) inject this engine — never <see cref="CssFramework"/> directly —
/// and Pennington deliberately does not register <see cref="CssFramework"/> with the DI
/// container. A host that wants its own framework registers one explicitly without any
/// chance of colliding with Pennington's.
///
/// Inject this engine and read <see cref="Framework"/> when you specifically want the
/// instance Pennington built — for ad-hoc <c>CompileUtilityClass</c> calls, theme
/// inspection, or anything else <see cref="CssFramework"/> exposes. The instance is the
/// same one Pennington feeds to the discovery pipeline, so the theme, custom utilities,
/// applies, and prose customization match what <c>/styles.css</c> serves.
/// </summary>
public sealed class MonorailCssEngine
{
    /// <summary>
    /// Initializes a new instance wrapping the supplied framework. Constructed once during DI
    /// registration; not intended for ad-hoc instantiation outside the container.
    /// </summary>
    /// <param name="framework">The configured framework instance to wrap.</param>
    public MonorailCssEngine(CssFramework framework)
    {
        Framework = framework;
    }

    /// <summary>
    /// Gets the configured framework instance Pennington built from
    /// <see cref="MonorailCssOptions"/>. Same instance the stylesheet endpoint uses and the
    /// discovery pipeline validates against.
    /// </summary>
    public CssFramework Framework { get; }
}
