namespace Pennington.Tests;

using Pennington.Infrastructure;
using Pennington.Pipeline;

/// <summary>
/// Shouldly-style assertions that check a union case type and extract the value.
/// Replaces the verbose pattern of ShouldBeTrue + switch + ShouldNotBeNull.
///
/// Matching an open generic case type against the union instance is disallowed (CS8780), so the
/// check goes through the synthesized <c>Value</c>; each union type gets its own overload.
/// </summary>
public static class UnionAssertExtensions
{
    public static TCase ShouldBeCase<TCase>(this ContentItem union) where TCase : class
    {
        if (union.Value is TCase result)
        {
            return result;
        }

        throw new ShouldAssertException(
            $"Expected ContentItem to be {typeof(TCase).Name}");
    }

    public static TCase ShouldBeCase<TCase>(this ContentSource union) where TCase : class
    {
        if (union.Value is TCase result)
        {
            return result;
        }

        throw new ShouldAssertException(
            $"Expected ContentSource to be {typeof(TCase).Name}");
    }

    public static TCase ShouldBeCase<TCase>(this LinkCheckResult union) where TCase : class
    {
        if (union.Value is TCase result)
        {
            return result;
        }

        throw new ShouldAssertException(
            $"Expected LinkCheckResult to be {typeof(TCase).Name}");
    }
}