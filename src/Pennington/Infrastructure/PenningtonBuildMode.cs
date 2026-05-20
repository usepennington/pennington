namespace Pennington.Infrastructure;

/// <summary>
/// Detects whether the host is running in static-build mode (CLI verb "build")
/// versus dev-serve mode. Centralized so every call site agrees on the array
/// shape — param-style arrays from <c>Main</c> have the verb at index 0;
/// <see cref="Environment.GetCommandLineArgs"/> has the executable at index 0
/// and the verb at index 1.
/// </summary>
public static class PenningtonBuildMode
{
    /// <summary>
    /// Returns true when <paramref name="args"/> (param-style: verb at index 0)
    /// starts with the "build" verb. Use when a caller already has the args
    /// array forwarded from <c>Main</c>.
    /// </summary>
    public static bool IsBuildMode(string[] args)
        => args.Length > 0 && args[0].Equals("build", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true when the current process was launched with the "build"
    /// verb. Reads <see cref="Environment.GetCommandLineArgs"/>, slices off the
    /// executable at index 0, and delegates to <see cref="IsBuildMode(string[])"/>.
    /// Use from middleware, processors, and other components that don't receive
    /// <c>Main</c>'s args directly.
    /// </summary>
    public static bool IsBuildMode()
    {
        var args = Environment.GetCommandLineArgs();
        return args.Length > 1 && IsBuildMode(args[1..]);
    }
}