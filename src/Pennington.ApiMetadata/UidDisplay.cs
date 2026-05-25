namespace Pennington.ApiMetadata;

/// <summary>Formatting helpers for xmldocid strings (the <c>T:</c>, <c>M:</c>, <c>P:</c>… prefixed uids the C# compiler emits).</summary>
public static class UidDisplay
{
    /// <summary>Returns the short, unqualified display name for a uid (e.g. <c>T:System.Collections.Generic.List`1</c> → <c>List</c>), stripping the kind prefix, any parameter list, the namespace, and generic-arity markers.</summary>
    public static string Shorten(string uid)
    {
        var cleaned = uid.Length >= 2 && uid[1] == ':' ? uid[2..] : uid;
        var paren = cleaned.IndexOf('(');
        if (paren >= 0)
        {
            cleaned = cleaned[..paren];
        }

        var lastDot = cleaned.LastIndexOf('.');
        var name = lastDot >= 0 ? cleaned[(lastDot + 1)..] : cleaned;
        var backtick = name.IndexOf('`');
        return backtick >= 0 ? name[..backtick] : name;
    }
}