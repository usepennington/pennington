namespace Pennington.Roslyn.Tests.Documentation;

using System.Collections.Generic;
using System.Linq;
using Pennington.Roslyn.Documentation;

internal static class XmlDocNodeAssertions
{
    public static T ShouldBeCase<T>(this XmlDocNode node) where T : class
    {
        if (node is T match)
        {
            return match;
        }

        throw new ShouldAssertException(
            $"Expected XmlDocNode case {typeof(T).Name} but was a different case.");
    }

    public static IEnumerable<T> Cases<T>(this IEnumerable<XmlDocNode> nodes) where T : class
        => nodes.Select(n => n is T t ? t : null).Where(t => t is not null).Select(t => t!);
}