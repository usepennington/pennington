namespace Penn.Infrastructure;

internal static class StringExtensions
{
    /// <summary>
    /// Splits on NewLines, first normalizing line endings.
    /// </summary>
    public static string[] SplitNewLines(this string s, StringSplitOptions options = StringSplitOptions.None)
    {
        return s.ReplaceLineEndings(Environment.NewLine).Split(Environment.NewLine, options);
    }
}
