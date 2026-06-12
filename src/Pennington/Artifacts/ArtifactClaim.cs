namespace Pennington.Artifacts;

using Routing;

/// <summary>
/// Matches any request path under <paramref name="Prefix"/>, optionally narrowed to paths ending
/// with <paramref name="Suffix"/> (e.g. <c>/search/</c> + <c>.json</c>).
/// </summary>
/// <param name="Prefix">Leading-slash path prefix, including its trailing slash (e.g. <c>/pdf/</c>).</param>
/// <param name="Suffix">Optional required path ending (e.g. <c>.pdf</c>); null claims the whole prefix.</param>
public record PrefixClaim(UrlPath Prefix, string? Suffix = null);

/// <summary>
/// Matches any request path ending with <paramref name="Suffix"/> at any depth — the mid-path
/// catch-all no endpoint route template can express (e.g. <c>/reference/api/llms.txt</c> via
/// <c>/llms.txt</c>). A path that is nothing but the suffix does not match, so a root file like
/// <c>/llms.txt</c> stays claimable by an <see cref="ExactClaim"/>.
/// </summary>
/// <param name="Suffix">Required path ending, including its leading slash (e.g. <c>/llms.txt</c>).</param>
public record SuffixClaim(string Suffix);

/// <summary>Matches exactly one request path.</summary>
/// <param name="Path">The leading-slash path (e.g. <c>/.well-known/atproto-did</c>).</param>
public record ExactClaim(UrlPath Path);

/// <summary>Union of the URL-territory shapes an <see cref="ArtifactClaim"/> can declare.</summary>
#if NET11_0_OR_GREATER
public union ArtifactClaimShape(PrefixClaim, SuffixClaim, ExactClaim);
#else
[System.Runtime.CompilerServices.Union]
public readonly struct ArtifactClaimShape : System.Runtime.CompilerServices.IUnion
{
    /// <summary>Wrapped case instance; inspect via pattern matching on the case types.</summary>
    public object? Value { get; }
    /// <summary>Wraps a <see cref="PrefixClaim"/>.</summary>
    public ArtifactClaimShape(PrefixClaim value) { Value = value; }
    /// <summary>Wraps a <see cref="SuffixClaim"/>.</summary>
    public ArtifactClaimShape(SuffixClaim value) { Value = value; }
    /// <summary>Wraps an <see cref="ExactClaim"/>.</summary>
    public ArtifactClaimShape(ExactClaim value) { Value = value; }
    /// <summary>Implicit conversion from <see cref="PrefixClaim"/>.</summary>
    public static implicit operator ArtifactClaimShape(PrefixClaim value) => new(value);
    /// <summary>Implicit conversion from <see cref="SuffixClaim"/>.</summary>
    public static implicit operator ArtifactClaimShape(SuffixClaim value) => new(value);
    /// <summary>Implicit conversion from <see cref="ExactClaim"/>.</summary>
    public static implicit operator ArtifactClaimShape(ExactClaim value) => new(value);
}
#endif

/// <summary>
/// A declared URL territory owned by an <see cref="IArtifactContentService"/>. Claims are derived
/// from options at construction — cheap, startup-stable, and consulted on every request by the
/// artifact router — so they must never trigger discovery, the site projection, or any other
/// lazy corpus work. The owning service's resolver stays authoritative: a claim match with a null
/// resolve falls through to content routing.
/// </summary>
/// <param name="Owner">Short stable name of the owning feature (e.g. <c>search</c>, <c>book</c>), shown in diag output and conflict warnings.</param>
/// <param name="Shape">The territory this claim reserves.</param>
/// <param name="Description">Human label shown in <c>diag routes</c> and namespace-conflict warnings.</param>
public sealed record ArtifactClaim(string Owner, ArtifactClaimShape Shape, string Description)
{
    /// <summary>True when <paramref name="path"/> (leading-slash request path) falls inside this territory.</summary>
    public bool Matches(string path) => Shape.Value switch
    {
        PrefixClaim p => path.StartsWith(p.Prefix.Value, StringComparison.OrdinalIgnoreCase)
            && (p.Suffix is null || path.EndsWith(p.Suffix, StringComparison.OrdinalIgnoreCase)),
        SuffixClaim s => path.EndsWith(s.Suffix, StringComparison.OrdinalIgnoreCase)
            && path.Length > s.Suffix.Length,
        ExactClaim e => string.Equals(path, e.Path.Value, StringComparison.OrdinalIgnoreCase),
        _ => false,
    };

    /// <summary>Glob-style rendering of the territory for diagnostics (e.g. <c>/search/**.json</c>, <c>**/llms.txt</c>).</summary>
    public string Pattern => Shape.Value switch
    {
        PrefixClaim p => p.Prefix.Value + "**" + p.Suffix,
        SuffixClaim s => "**" + s.Suffix,
        ExactClaim e => e.Path.Value,
        _ => "?",
    };
}
