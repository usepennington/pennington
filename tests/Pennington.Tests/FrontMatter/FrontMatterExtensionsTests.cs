using Microsoft.Extensions.Time.Testing;
using Pennington.FrontMatter;

namespace Pennington.Tests.FrontMatter;

public class FrontMatterExtensionsTests
{
    private record TestFrontMatter : IFrontMatter
    {
        public string Title { get; init; } = "Test";
        public bool IsDraft { get; init; }
        public DateTime? Date { get; init; }
    }

    private static FakeTimeProvider ClockAt(DateTime localNow)
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(localNow, TimeSpan.Zero));
        clock.SetLocalTimeZone(TimeZoneInfo.Utc);
        return clock;
    }

    [Fact]
    public void IsScheduled_FutureDate_ReturnsTrue()
    {
        var clock = ClockAt(new DateTime(2030, 6, 15));
        var fm = new TestFrontMatter { Date = new DateTime(2030, 6, 16) };

        fm.IsScheduled(clock).ShouldBeTrue();
    }

    [Fact]
    public void IsScheduled_PastDate_ReturnsFalse()
    {
        var clock = ClockAt(new DateTime(2030, 6, 15));
        var fm = new TestFrontMatter { Date = new DateTime(2030, 6, 14) };

        fm.IsScheduled(clock).ShouldBeFalse();
    }

    [Fact]
    public void IsScheduled_ExactlyNow_ReturnsFalse()
    {
        var now = new DateTime(2030, 6, 15, 12, 0, 0);
        var clock = ClockAt(now);
        var fm = new TestFrontMatter { Date = now };

        fm.IsScheduled(clock).ShouldBeFalse();
    }

    [Fact]
    public void IsScheduled_NullDate_ReturnsFalse()
    {
        var clock = ClockAt(new DateTime(2030, 6, 15));
        var fm = new TestFrontMatter { Date = null };

        fm.IsScheduled(clock).ShouldBeFalse();
    }

    [Fact]
    public void IsHiddenFromBuild_Draft_ReturnsTrue()
    {
        var clock = ClockAt(new DateTime(2030, 6, 15));
        var fm = new TestFrontMatter { IsDraft = true, Date = new DateTime(2020, 1, 1) };

        fm.IsHiddenFromBuild(clock).ShouldBeTrue();
    }

    [Fact]
    public void IsHiddenFromBuild_ScheduledOnly_ReturnsTrue()
    {
        var clock = ClockAt(new DateTime(2030, 6, 15));
        var fm = new TestFrontMatter { IsDraft = false, Date = new DateTime(2030, 6, 16) };

        fm.IsHiddenFromBuild(clock).ShouldBeTrue();
    }

    [Fact]
    public void IsHiddenFromBuild_LivePublished_ReturnsFalse()
    {
        var clock = ClockAt(new DateTime(2030, 6, 15));
        var fm = new TestFrontMatter { IsDraft = false, Date = new DateTime(2020, 1, 1) };

        fm.IsHiddenFromBuild(clock).ShouldBeFalse();
    }
}
