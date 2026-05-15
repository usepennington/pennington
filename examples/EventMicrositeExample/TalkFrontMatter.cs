namespace EventMicrositeExample;

using Pennington.FrontMatter;

/// <summary>
/// Front matter for talks under <c>Content/talks/</c>. Carries both a <see cref="Topic"/>
/// (single-valued) and <see cref="Tags"/> (multi-valued) so the example can demonstrate
/// both <c>SelectKey</c> and <c>SelectKeys</c> taxonomy registrations.
/// </summary>
public record TalkFrontMatter : IFrontMatter, ITaggable
{
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool IsDraft { get; init; }
    public string Speaker { get; init; } = "";
    public string Time { get; init; } = "";
    public string Topic { get; init; } = "";
    public string[] Tags { get; init; } = [];
    public string? Uid { get; init; }
    public bool Search { get; init; } = true;
    public bool Llms { get; init; } = true;
    public bool SearchOnly { get; init; }
}

/// <summary>Single sponsor entry, populated from <c>data/sponsors.yml</c>.</summary>
public record Sponsor
{
    public string Name { get; init; } = "";
    public string Tier { get; init; } = "";
    public string Url { get; init; } = "";
}

/// <summary>Conference schedule, populated from <c>data/schedule.yml</c>.</summary>
public record Schedule
{
    public string Title { get; init; } = "";
    public string Date { get; init; } = "";
    public string Venue { get; init; } = "";
    public List<ScheduleSlot> Slots { get; init; } = [];
}

/// <summary>One row in the schedule grid.</summary>
public record ScheduleSlot
{
    public string Time { get; init; } = "";
    public string Title { get; init; } = "";
    public string Speaker { get; init; } = "";
}
