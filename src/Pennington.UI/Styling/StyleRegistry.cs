namespace Pennington.UI.Styling;

using System.Collections.Frozen;
using System.Collections.Immutable;

/// <summary>Layer a resolved style slot's effective value came from.</summary>
public enum StyleSource
{
    /// <summary>The Pennington.UI component default.</summary>
    Default,

    /// <summary>A site template's skin, which replaces the component default wholesale.</summary>
    TemplateSkin,

    /// <summary>A consumer override, Tailwind-merged over the skin or component default.</summary>
    ConsumerOverride,
}

/// <summary>One resolved style slot: the effective class string plus the per-layer inputs that produced it.</summary>
/// <param name="Key">Slot key from <see cref="StyleKeys"/>.</param>
/// <param name="Effective">Class string the components render.</param>
/// <param name="Source">Layer that determined <paramref name="Effective"/>.</param>
/// <param name="DefaultValue">The Pennington.UI component default for the slot.</param>
/// <param name="SkinValue">The template skin's value, when the template re-skins this slot.</param>
/// <param name="OverrideValue">The consumer's override, when one was supplied.</param>
public sealed record StyleEntry(
    string Key,
    string Effective,
    StyleSource Source,
    string DefaultValue,
    string? SkinValue,
    string? OverrideValue);

/// <summary>
/// Slot-key → CSS class registry behind the overridable styling on Pennington.UI components.
/// Three layers: component defaults (<see cref="UiStyleDefaults"/>), an optional template skin
/// that replaces a default wholesale, and optional consumer overrides that are merged over the
/// result with Tailwind conflict resolution (conflicting utilities replaced, the rest kept) —
/// <c>effective = merge(skin ?? default, override)</c>, where <c>merge</c> is the MonorailCSS
/// class merger supplied by the caller. Component <c>*Class</c> parameters add a per-instance
/// layer on top via <see cref="Merge(string, string?)"/>. Keys are case-insensitive.
/// </summary>
public sealed class StyleRegistry
{
    private readonly FrozenDictionary<string, StyleEntry> _entries;
    private readonly Func<string, string, string>? _mergeOverride;

    private StyleRegistry(FrozenDictionary<string, StyleEntry> entries, Func<string, string, string>? mergeOverride)
    {
        _entries = entries;
        _mergeOverride = mergeOverride;
        Entries = [.. entries.Values.OrderBy(e => e.Key, StringComparer.Ordinal)];
    }

    /// <summary>Every slot sorted by key, with per-layer inputs — consumed by <c>diag styles</c> and tests.</summary>
    public ImmutableList<StyleEntry> Entries { get; }

    /// <summary>Effective class string for <paramref name="key"/>; throws on an unknown key, listing the valid catalog.</summary>
    public string this[string key] => Get(key);

    /// <summary>Effective class string for <paramref name="key"/>; throws on an unknown key, listing the valid catalog.</summary>
    public string Get(string key) =>
        _entries.TryGetValue(key, out var entry) ? entry.Effective : throw UnknownKey(key);

    /// <summary>
    /// Effective class string for <paramref name="key"/> with <paramref name="classes"/>
    /// Tailwind-merged over it — the per-instance layer behind component <c>*Class</c>
    /// parameters. Null or empty <paramref name="classes"/> returns the effective value unchanged.
    /// </summary>
    public string Merge(string key, string? classes) =>
        string.IsNullOrEmpty(classes) ? Get(key) : Merge(Get(key), classes, _mergeOverride);

    /// <summary>
    /// Builds a registry from the component defaults, an optional template skin, and optional
    /// consumer overrides. Skin and override keys must exist in <see cref="StyleKeys"/>; an
    /// unknown key throws an <see cref="InvalidOperationException"/> naming the valid catalog.
    /// </summary>
    /// <param name="templateSkin">Per-slot replacements a site template applies to the component defaults.</param>
    /// <param name="overrides">Per-slot consumer classes Tailwind-merged over the skinned defaults.</param>
    /// <param name="mergeOverride">
    /// Tailwind-aware merge for overridden slots and per-instance <see cref="Merge(string, string?)"/>
    /// calls — <c>(baseClasses, overrideClasses) =&gt; effective</c>, typically the MonorailCSS class
    /// merger from <c>MonorailCssService.CreateClassMerger</c>. When null the override is appended
    /// without conflict resolution (the bare-host fallback).
    /// </param>
    public static StyleRegistry Create(
        IReadOnlyDictionary<string, string>? templateSkin = null,
        IReadOnlyDictionary<string, string>? overrides = null,
        Func<string, string, string>? mergeOverride = null)
    {
        // Re-key the caller's dictionaries case-insensitively — their comparer is whatever
        // the consumer constructed, and slot lookups below must honor the catalog's casing rule.
        var skinLayer = Normalize(templateSkin, "template skin");
        var overrideLayer = Normalize(overrides, "style override");

        var entries = new Dictionary<string, StyleEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, defaultValue) in UiStyleDefaults.Defaults)
        {
            string? skinValue = null;
            if (skinLayer?.TryGetValue(key, out var skin) == true)
            {
                skinValue = skin;
            }

            string? overrideValue = null;
            if (overrideLayer?.TryGetValue(key, out var over) == true)
            {
                overrideValue = over;
            }

            var baseValue = skinValue ?? defaultValue;
            var (effective, source) = overrideValue is null
                ? (baseValue, skinValue is null ? StyleSource.Default : StyleSource.TemplateSkin)
                : (Merge(baseValue, overrideValue, mergeOverride), StyleSource.ConsumerOverride);

            entries[key] = new StyleEntry(key, effective, source, defaultValue, skinValue, overrideValue);
        }

        return new StyleRegistry(entries.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase), mergeOverride);
    }

    // The MonorailCSS merger derives conflicts from what each class compiles to in the owning
    // framework — gap/semantic-palette/custom-utility groups are all handled natively, with no
    // hand-maintained class-group config to patch. Without a merger (a bare host that didn't
    // supply one), append the override so its classes are at least present, accepting that
    // conflicting base utilities are not removed.
    private static string Merge(string baseValue, string overrideValue, Func<string, string, string>? merge) =>
        merge is not null
            ? merge(baseValue, overrideValue)
            : string.IsNullOrEmpty(baseValue) ? overrideValue : $"{baseValue} {overrideValue}";

    private static Dictionary<string, string>? Normalize(
        IReadOnlyDictionary<string, string>? layer, string layerName)
    {
        if (layer is null)
        {
            return null;
        }

        var normalized = new Dictionary<string, string>(layer.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in layer)
        {
            if (!UiStyleDefaults.Defaults.ContainsKey(key))
            {
                throw UnknownKey(key, layerName);
            }

            normalized[key] = value;
        }

        return normalized;
    }

    private static InvalidOperationException UnknownKey(string key, string layerName = "style") =>
        new($"Unknown {layerName} key '{key}'. Valid keys: " +
            string.Join(", ", UiStyleDefaults.Defaults.Keys.OrderBy(k => k, StringComparer.Ordinal)));
}
