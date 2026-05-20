namespace Pennington.Tests.Tui;

using Pennington.Tui;
using Shouldly;
using Xunit;

public class RequestLogTests
{
    [Fact]
    public void Append_assigns_monotonic_sequence()
    {
        var log = new BoundedSequenceLog<RequestEntry>(100);
        for (var i = 0; i < 5; i++)
        {
            var idx = i;
            log.Append(seq => new RequestEntry(seq, DateTimeOffset.Now, "GET", $"/page/{idx}", "", 200, TimeSpan.FromMilliseconds(idx)));
        }

        var snapshot = log.Snapshot();
        snapshot.Count.ShouldBe(5);
        for (var i = 1; i < snapshot.Count; i++)
        {
            snapshot[i].Sequence.ShouldBeGreaterThan(snapshot[i - 1].Sequence);
        }
    }

    [Fact]
    public void Append_trims_to_capacity()
    {
        var log = new BoundedSequenceLog<RequestEntry>(50);
        for (var i = 0; i < 200; i++)
        {
            var idx = i;
            log.Append(seq => new RequestEntry(seq, DateTimeOffset.Now, "GET", $"/page/{idx}", "", 200, TimeSpan.Zero));
        }

        var snapshot = log.Snapshot();
        snapshot.Count.ShouldBe(50);
        // Oldest entries get trimmed first — remaining should be the last 50.
        snapshot[0].Path.ShouldBe("/page/150");
        snapshot[^1].Path.ShouldBe("/page/199");
    }

    [Fact]
    public void Capacity_enforces_minimum_floor()
    {
        // Even when the caller asks for 0, the log keeps at least 50 entries so the
        // Requests panel doesn't render empty on a busy page.
        var log = new BoundedSequenceLog<RequestEntry>(0);
        for (var i = 0; i < 60; i++)
        {
            var idx = i;
            log.Append(seq => new RequestEntry(seq, DateTimeOffset.Now, "GET", $"/page/{idx}", "", 200, TimeSpan.Zero));
        }

        log.Snapshot().Count.ShouldBe(50);
    }
}