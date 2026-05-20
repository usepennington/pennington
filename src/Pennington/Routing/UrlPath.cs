namespace Pennington.Routing;

/// <summary>A URL path value supporting composition and normalization.</summary>
/// <param name="Value">Underlying path string.</param>
public readonly record struct UrlPath(string Value)
{
    /// <summary>Implicitly converts a string to a <see cref="UrlPath"/>.</summary>
    public static implicit operator UrlPath(string value) => new(value);

    /// <summary>Joins two URL path segments, normalizing slashes.</summary>
    public static UrlPath operator /(UrlPath left, UrlPath right)
    {
        var l = left.Value.TrimEnd('/');
        var r = right.Value.TrimStart('/');
        if (string.IsNullOrEmpty(l))
        {
            return new UrlPath("/" + r);
        }

        if (string.IsNullOrEmpty(r))
        {
            return new UrlPath(l.StartsWith('/') ? l : "/" + l);
        }

        return new UrlPath(l + "/" + r);
    }

    /// <summary>Returns a path guaranteed to start with a slash.</summary>
    public UrlPath EnsureLeadingSlash()
        => Value.StartsWith('/') ? this : new UrlPath("/" + Value);

    /// <summary>Returns a path guaranteed to end with a slash.</summary>
    public UrlPath EnsureTrailingSlash()
        => Value.EndsWith('/') ? this : new UrlPath(Value + "/");

    /// <summary>Removes a trailing slash (except from the root path).</summary>
    public UrlPath RemoveTrailingSlash()
        => Value.Length > 1 && Value.EndsWith('/') ? new UrlPath(Value[..^1]) : this;

    /// <summary>Removes a leading slash if present.</summary>
    public UrlPath RemoveLeadingSlash()
        => Value.StartsWith('/') ? new UrlPath(Value[1..]) : this;

    /// <summary>Compares two URL paths ignoring trailing slashes, index.html suffixes, and case.</summary>
    public bool Matches(UrlPath other)
    {
        var a = Normalize(Value);
        var b = Normalize(other.Value);
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string path)
    {
        var s = path.TrimEnd('/');
        if (s.EndsWith("/index.html", StringComparison.OrdinalIgnoreCase))
        {
            s = s[..^"/index.html".Length];
        }
        else if (s.EndsWith("/index.htm", StringComparison.OrdinalIgnoreCase))
        {
            s = s[..^"/index.htm".Length];
        }

        if (string.IsNullOrEmpty(s))
        {
            s = "/";
        }

        return s.ToLowerInvariant();
    }

    /// <summary>Returns the underlying URL string.</summary>
    public override string ToString() => Value;
}