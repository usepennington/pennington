namespace Pennington.Routing;

public readonly record struct FilePath(string Value)
{
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

    public string Extension => Path.GetExtension(Value);
    public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(Value);
    public string FileName => Path.GetFileName(Value);

    public override string ToString() => Value;
}
