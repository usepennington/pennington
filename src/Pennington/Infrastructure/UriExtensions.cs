namespace Pennington.Infrastructure;

/// <summary>
/// Explicit options for <see cref="UriExtensions"/>'s <c>Uri.TryCreate</c>
/// overload. Modeled on the shape of the proposed BCL <c>UriCreationOptions</c>
/// (dotnet/runtime), but the shipped BCL type is minimal and omits
/// <c>AllowImplicitFilePaths</c>, so we keep our own.
/// </summary>
internal struct UriParseOptions
{
    public UriKind UriKind { get; set; }

    /// <summary>
    /// When false, strings that parse only via implicit-file-path interpretation
    /// (e.g. <c>/preview/</c> on Linux, <c>C:\foo</c> on Windows) are rejected.
    /// Guards against the cross-platform trap where <c>Uri.TryCreate("/x", Absolute, _)</c>
    /// returns true on Linux and false on Windows.
    /// </summary>
    public bool AllowImplicitFilePaths { get; set; }
}

internal static class UriExtensions
{
    extension(Uri)
    {
        public static bool TryCreate(string? uriString, in UriParseOptions options, out Uri? result)
        {
            if (!Uri.TryCreate(uriString, options.UriKind, out result))
            {
                return false;
            }

            if (!options.AllowImplicitFilePaths
                && result is { IsAbsoluteUri: true, Scheme: "file" }
                && !(uriString?.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                result = null;
                return false;
            }

            return true;
        }
    }
}