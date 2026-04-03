namespace Penn.Routing;

public readonly record struct UrlPath(string Value)
{
    public static implicit operator UrlPath(string value) => new(value);

    public static UrlPath operator /(UrlPath left, UrlPath right)
    {
        var l = left.Value.TrimEnd('/');
        var r = right.Value.TrimStart('/');
        if (string.IsNullOrEmpty(l)) return new UrlPath("/" + r);
        if (string.IsNullOrEmpty(r)) return new UrlPath(l.StartsWith('/') ? l : "/" + l);
        return new UrlPath(l + "/" + r);
    }

    public UrlPath EnsureLeadingSlash()
        => Value.StartsWith('/') ? this : new UrlPath("/" + Value);

    public UrlPath EnsureTrailingSlash()
        => Value.EndsWith('/') ? this : new UrlPath(Value + "/");

    public UrlPath RemoveTrailingSlash()
        => Value.Length > 1 && Value.EndsWith('/') ? new UrlPath(Value[..^1]) : this;

    public UrlPath RemoveLeadingSlash()
        => Value.StartsWith('/') ? new UrlPath(Value[1..]) : this;

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
            s = s[..^"/index.html".Length];
        else if (s.EndsWith("/index.htm", StringComparison.OrdinalIgnoreCase))
            s = s[..^"/index.htm".Length];
        if (string.IsNullOrEmpty(s)) s = "/";
        return s.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
