namespace BeyondCookFormatExample;

using Pennington.FrontMatter;

/// <summary>
/// Front matter for a <c>.cook</c> recipe. Implements <see cref="IFrontMatter"/> (every page needs a
/// <c>Title</c>) and <see cref="ITaggable"/> (recipe tags feed navigation and the search facet). The
/// property names are camelCase-matched to the YAML keys (<c>prepTime</c>, <c>cookTime</c>, …), so they
/// bind without extra attributes and don't trip the build's strict unknown-key check.
/// </summary>
public sealed record CookFrontMatter : IFrontMatter, ITaggable
{
    /// <inheritdoc/>
    public string Title { get; init; } = "";

    /// <summary>Short recipe description, shown under the title.</summary>
    public string? Description { get; init; }

    /// <summary>Number of servings the recipe yields.</summary>
    public string? Servings { get; init; }

    /// <summary>Preparation time (e.g. "15 minutes").</summary>
    public string? PrepTime { get; init; }

    /// <summary>Active cooking time (e.g. "25 minutes").</summary>
    public string? CookTime { get; init; }

    /// <summary>Total time from start to finish.</summary>
    public string? TotalTime { get; init; }

    /// <summary>Resting time, when the recipe calls for it.</summary>
    public string? RestTime { get; init; }

    /// <inheritdoc/>
    public string[] Tags { get; init; } = [];
}
