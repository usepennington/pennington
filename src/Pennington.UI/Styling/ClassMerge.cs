namespace Pennington.UI.Styling;

/// <summary>
/// Tailwind-aware class merge a component uses to fold a per-instance <c>*Class</c> parameter
/// over its variant base, so a passed utility knocks out the conflicting base utility instead of
/// duplicating it. Site templates register one built from
/// <c>MonorailCssService.CreateClassMerger(...)</c>; bare hosts leave it unregistered and the
/// component falls back to appending (conflicting base utilities are not removed, but the passed
/// classes are at least present).
/// </summary>
public sealed class ClassMerge
{
    private readonly Func<string, string, string>? _merge;

    /// <param name="merge">The conflict-aware merge, or <see langword="null"/> to append.</param>
    public ClassMerge(Func<string, string, string>? merge = null) => _merge = merge;

    /// <summary>
    /// Folds <paramref name="extra"/> over <paramref name="baseClasses"/>. Returns
    /// <paramref name="baseClasses"/> unchanged when <paramref name="extra"/> is null or empty.
    /// </summary>
    public string Apply(string baseClasses, string? extra)
        => string.IsNullOrEmpty(extra)
            ? baseClasses
            : _merge is not null ? _merge(baseClasses, extra) : $"{baseClasses} {extra}".Trim();
}
