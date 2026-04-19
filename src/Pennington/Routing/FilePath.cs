namespace Pennington.Routing;

/// <summary>A file system path value used for content source and output locations.</summary>
/// <param name="Value">Underlying path string.</param>
public readonly record struct FilePath(string Value)
{
    /// <summary>Implicitly converts a string to a <see cref="FilePath"/>.</summary>
    public static implicit operator FilePath(string value) => new(value);

    /// <summary>Combine two file paths.</summary>
    public static FilePath operator /(FilePath left, FilePath right)
    {
        var l = left.Value.TrimEnd('/', '\\');
        var r = right.Value.TrimStart('/', '\\');
        if (string.IsNullOrEmpty(l)) return new FilePath(r);
        if (string.IsNullOrEmpty(r)) return new FilePath(l);
        return new FilePath(l + "/" + r);
    }

    /// <summary>The file extension including the leading dot, or empty if none.</summary>
    public string Extension => Path.GetExtension(Value);

    /// <summary>The file name without its extension.</summary>
    public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(Value);

    /// <summary>The file name with extension.</summary>
    public string FileName => Path.GetFileName(Value);

    /// <summary>Returns the underlying path string.</summary>
    public override string ToString() => Value;
}
