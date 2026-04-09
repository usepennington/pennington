using Pennington.Infrastructure;
using Pennington.Pipeline;

namespace Pennington.Tests;

/// <summary>
/// Shouldly-style assertions that check a union case type and extract the value.
/// Replaces the verbose pattern of ShouldBeTrue + switch + ShouldNotBeNull.
///
/// C# 15 unions require switch expressions for pattern matching (not <c>is</c> via <c>object</c>),
/// so each union type needs a dedicated overload.
/// </summary>
public static class UnionAssertExtensions
{
    public static TCase ShouldBeCase<TCase>(this ContentItem union) where TCase : class
    {
        if (union switch { TCase t => t, _ => null } is TCase result)
        {
            return result;
        }

        throw new ShouldAssertException(
            $"Expected ContentItem to be {typeof(TCase).Name}");
    }

    public static TCase ShouldBeCase<TCase>(this ContentSource union) where TCase : class
    {
        if (union switch { TCase t => t, _ => null } is TCase result)
        {
            return result;
        }

        throw new ShouldAssertException(
            $"Expected ContentSource to be {typeof(TCase).Name}");
    }

    public static TCase ShouldBeCase<TCase>(this LinkCheckResult union) where TCase : class
    {
        if (union switch { TCase t => t, _ => null } is TCase result)
        {
            return result;
        }

        throw new ShouldAssertException(
            $"Expected LinkCheckResult to be {typeof(TCase).Name}");
    }

    public static TCase ShouldBeCase<TCase>(this ProgrammaticContent union) where TCase : class
    {
        if (union switch { TCase t => t, _ => null } is TCase result)
        {
            return result;
        }

        throw new ShouldAssertException(
            $"Expected ProgrammaticContent to be {typeof(TCase).Name}");
    }
}
