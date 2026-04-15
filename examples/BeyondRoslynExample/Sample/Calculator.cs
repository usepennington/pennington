namespace BeyondRoslynExample.Sample;

/// <summary>
/// A tiny arithmetic helper used as the tutorial's xmldocid target. Nothing
/// about this class is clever — the point is that the tutorial's doc prose
/// can fence <c>M:BeyondRoslynExample.Sample.Calculator.Add(System.Int32,System.Int32)</c>
/// and pull the real source into rendered HTML.
/// </summary>
public sealed class Calculator
{
    /// <summary>Adds two integers.</summary>
    /// <param name="a">First addend.</param>
    /// <param name="b">Second addend.</param>
    /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public int Add(int a, int b)
    {
        return a + b;
    }

    /// <summary>Multiplies two integers.</summary>
    /// <param name="a">First factor.</param>
    /// <param name="b">Second factor.</param>
    /// <returns>The product of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public int Multiply(int a, int b)
    {
        return a * b;
    }

    /// <summary>Returns the arithmetic mean of a non-empty sequence.</summary>
    /// <param name="values">Values to average. Must contain at least one element.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="values"/> is empty.</exception>
    public double Mean(IReadOnlyList<int> values)
    {
        if (values.Count == 0)
        {
            throw new ArgumentException("At least one value is required.", nameof(values));
        }

        var total = 0L;
        foreach (var v in values)
        {
            total += v;
        }

        return (double)total / values.Count;
    }
}