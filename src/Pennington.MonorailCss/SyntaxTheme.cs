namespace Pennington.MonorailCss;

/// <summary>
/// Color palette used by <c>.hljs-*</c> syntax-highlight token classes.
/// Each slot is a Tailwind color name whose shades (300-800) are consumed by light/dark theme rules.
/// </summary>
public sealed record SyntaxTheme
{
    /// <summary>Keywords, class names, literals, selector tags.</summary>
    public required ColorName Keyword { get; init; }

    /// <summary>String literals, numbers, regular expressions.</summary>
    public required ColorName String { get; init; }

    /// <summary>Variables, attribute names, symbols.</summary>
    public required ColorName Variable { get; init; }

    /// <summary>Function/method titles, parameters, built-ins.</summary>
    public required ColorName Function { get; init; }

    /// <summary>Comments and quotes. Usually the site's base color.</summary>
    public required ColorName Comment { get; init; }

    /// <summary>Default palette: Sky keywords, Emerald strings, Rose variables, Amber functions, Slate comments.</summary>
    public static SyntaxTheme Default { get; } = new()
    {
        Keyword = ColorName.Sky,
        String = ColorName.Emerald,
        Variable = ColorName.Rose,
        Function = ColorName.Amber,
        Comment = ColorName.Slate,
    };
}
