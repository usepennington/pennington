namespace Pennington.FrontMatter;

/// <summary>
/// Helpers that combine <see cref="IFrontMatter"/> capability checks into the single
/// "skip this page during build" decision every content service and feed builder asks.
/// </summary>
public static class FrontMatterExtensions
{
    /// <summary>
    /// True when <see cref="IFrontMatter.Date"/> is set to a moment after <paramref name="clock"/>'s
    /// current wall-clock time. Comparison uses the wall-clock time in <paramref name="clock"/>'s
    /// configured local zone (without re-converting through the system zone), which is how YAML
    /// <c>date:</c> values — parsed as <see cref="DateTimeKind.Unspecified"/> — are meant to read.
    /// </summary>
    public static bool IsScheduled(this IFrontMatter frontMatter, TimeProvider clock)
        => frontMatter.Date is { } date && date > clock.GetLocalNow().DateTime;

    /// <summary>
    /// True when the page should be excluded from production build output — either explicitly
    /// drafted (<see cref="IFrontMatter.IsDraft"/>) or scheduled for a future
    /// <see cref="IFrontMatter.Date"/>. Mirrors the dev-vs-build split: dev-time requests still
    /// render so authors can preview, but the static crawler skips these routes.
    /// </summary>
    public static bool IsHiddenFromBuild(this IFrontMatter frontMatter, TimeProvider clock)
        => frontMatter.IsDraft || frontMatter.IsScheduled(clock);
}
