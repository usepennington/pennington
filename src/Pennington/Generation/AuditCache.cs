namespace Pennington.Generation;

using System.Collections.Immutable;

/// <summary>Default <see cref="IAuditCache"/> implementation; written to by <see cref="AuditRunner"/>.</summary>
public sealed class AuditCache : IAuditCache
{
    private ImmutableList<BuildDiagnostic> _diagnostics = ImmutableList<BuildDiagnostic>.Empty;

    /// <inheritdoc/>
    public ImmutableList<BuildDiagnostic> Diagnostics => _diagnostics;

    /// <inheritdoc/>
    public event Action? Updated;

    /// <summary>Replaces the cached snapshot and raises <see cref="Updated"/>.</summary>
    internal void Set(ImmutableList<BuildDiagnostic> diagnostics)
    {
        _diagnostics = diagnostics;
        Updated?.Invoke();
    }
}
