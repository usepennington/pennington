namespace Pennington.Generation;

using System.Collections.Immutable;

/// <summary>
/// Singleton store for the most recent audit pass. Read by the dev-mode overlay
/// processor (per request, filtered to the current route) and by
/// <see cref="OutputGenerationService"/> at the end of a static build (copied into
/// the <see cref="BuildReport"/>).
/// </summary>
public interface IAuditCache
{
    /// <summary>The diagnostics produced by the most recent run, in insertion order.</summary>
    ImmutableList<BuildDiagnostic> Diagnostics { get; }

    /// <summary>Raised after the cache is replaced. Use to log or refresh derived state.</summary>
    event Action? Updated;
}