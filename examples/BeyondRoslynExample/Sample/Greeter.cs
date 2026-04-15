namespace BeyondRoslynExample.Sample;

/// <summary>
/// Builds friendly greetings. Exists so the tutorial's second xmldocid fence
/// can reference a type other than <see cref="Calculator"/>.
/// </summary>
public sealed class Greeter
{
    /// <summary>The greeting prefix, e.g. <c>"Hello"</c> or <c>"Bonjour"</c>.</summary>
    public string Prefix { get; }

    /// <summary>Creates a greeter with the supplied <paramref name="prefix"/>.</summary>
    public Greeter(string prefix)
    {
        Prefix = prefix;
    }

    /// <summary>
    /// Builds a greeting for <paramref name="name"/> using <see cref="Prefix"/>.
    /// </summary>
    /// <param name="name">The recipient's display name.</param>
    /// <returns>A string of the form "<c>{Prefix}, {name}!</c>".</returns>
    public string Greet(string name)
    {
        return $"{Prefix}, {name}!";
    }
}