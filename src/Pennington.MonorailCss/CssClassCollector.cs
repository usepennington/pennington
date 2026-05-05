using MonorailCss.Discovery;

namespace Pennington.MonorailCss;

/// <summary>
/// Backwards-compatible facade that exposes the discovery pipeline's class set under the
/// pre-Discovery type name. New code should inject <see cref="IClassRegistry"/> directly;
/// this type exists so older callers that took a <c>CssClassCollector</c> dependency keep
/// compiling unchanged.
/// </summary>
public class CssClassCollector
{
    private readonly IClassRegistry _registry;

    /// <summary>
    /// Initializes a new instance backed by the supplied class registry.
    /// </summary>
    public CssClassCollector(IClassRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Returns the discovered CSS class names. The discovery pipeline keeps this set fresh —
    /// no manual <c>AddClasses</c> calls are required.
    /// </summary>
    public IReadOnlyCollection<string> GetClasses() => _registry.GetClasses();

    /// <summary>
    /// Kept for source compatibility; the discovery pipeline accumulates classes automatically,
    /// so this no longer does anything.
    /// </summary>
    [Obsolete("MonorailCss.Discovery now scans assemblies and source files automatically. AddClasses is a no-op.")]
    public void AddClasses(string url, IEnumerable<string> classes)
    {
        // intentionally empty
    }

    /// <summary>Kept for source compatibility. The discovery pipeline manages its own locking.</summary>
    [Obsolete("MonorailCss.Discovery handles synchronization internally. BeginProcessing is a no-op.")]
    public void BeginProcessing()
    {
    }

    /// <summary>Kept for source compatibility. The discovery pipeline manages its own locking.</summary>
    [Obsolete("MonorailCss.Discovery handles synchronization internally. EndProcessing is a no-op.")]
    public void EndProcessing()
    {
    }
}
