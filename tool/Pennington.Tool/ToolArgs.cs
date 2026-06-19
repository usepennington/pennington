namespace Pennington.Tool;

/// <summary>
/// Splits the tool's own options out of the argument list before the rest is handed to the
/// Pennington engine. The only tool-owned option is <c>--root</c> (alias <c>-r</c>), which selects
/// the site folder; everything else is a Pennington verb/flag (<c>build</c>, <c>diag</c>, …) and is
/// forwarded verbatim.
/// </summary>
internal static class ToolArgs
{
    /// <summary>
    /// Returns the absolute site-root folder and the engine-bound argument list.
    /// The root defaults to the current working directory. Both <c>--root=PATH</c> and
    /// <c>--root PATH</c> (and the <c>-r</c> alias) are accepted, but the engine's run-mode detector
    /// reads the raw process command line too, so the equals form is safest with <c>build</c>
    /// (a space-separated value can otherwise be misread as a positional base URL).
    /// </summary>
    public static (string Root, string[] Forward) Resolve(string[] args)
    {
        string? root = null;
        var forward = new List<string>(args.Length);

        for (var i = 0; i < args.Length; i++)
        {
            var a = args[i];
            if (TrySplitEquals(a, "--root", out var v) || TrySplitEquals(a, "-r", out v))
            {
                root = v;
            }
            else if (a is "--root" or "-r")
            {
                if (i + 1 < args.Length)
                {
                    root = args[++i];
                }
            }
            else
            {
                forward.Add(a);
            }
        }

        root = Path.GetFullPath(root ?? Directory.GetCurrentDirectory());
        return (root, forward.ToArray());
    }

    private static bool TrySplitEquals(string token, string name, out string value)
    {
        var prefix = name + "=";
        if (token.StartsWith(prefix, StringComparison.Ordinal))
        {
            value = token[prefix.Length..];
            return true;
        }

        value = "";
        return false;
    }
}
