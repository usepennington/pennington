namespace Pennington.Tests.Tui;

using Pennington.Tui;
using Shouldly;
using Xunit;

public class FileChangeLogTests
{
    [Fact]
    public void Record_dedups_same_path_and_increments_count()
    {
        var log = new FileChangeLog();
        var t1 = DateTimeOffset.Parse("2026-04-17T10:00:00Z");
        var t2 = DateTimeOffset.Parse("2026-04-17T10:00:05Z");
        var t3 = DateTimeOffset.Parse("2026-04-17T10:00:10Z");

        log.Record("/repo/a.md", WatcherChangeTypes.Changed, t1);
        log.Record("/repo/a.md", WatcherChangeTypes.Changed, t2);
        log.Record("/repo/a.md", WatcherChangeTypes.Changed, t3);

        var snapshot = log.Snapshot();
        snapshot.Count.ShouldBe(1);
        snapshot[0].FullPath.ShouldBe("/repo/a.md");
        snapshot[0].Count.ShouldBe(3);
        snapshot[0].LastChanged.ShouldBe(t3);
    }

    [Fact]
    public void Snapshot_orders_by_last_changed_desc()
    {
        var log = new FileChangeLog();
        var t1 = DateTimeOffset.Parse("2026-04-17T10:00:00Z");
        var t2 = DateTimeOffset.Parse("2026-04-17T10:00:05Z");
        var t3 = DateTimeOffset.Parse("2026-04-17T10:00:10Z");

        log.Record("/repo/a.md", WatcherChangeTypes.Changed, t1);
        log.Record("/repo/b.md", WatcherChangeTypes.Changed, t3);
        log.Record("/repo/c.md", WatcherChangeTypes.Changed, t2);

        var snapshot = log.Snapshot();
        snapshot.Select(e => e.FullPath).ShouldBe(["/repo/b.md", "/repo/c.md", "/repo/a.md"]);
    }

    [Fact]
    public void Record_keeps_latest_change_type()
    {
        var log = new FileChangeLog();
        var t1 = DateTimeOffset.Parse("2026-04-17T10:00:00Z");
        var t2 = DateTimeOffset.Parse("2026-04-17T10:00:05Z");

        log.Record("/repo/a.md", WatcherChangeTypes.Created, t1);
        log.Record("/repo/a.md", WatcherChangeTypes.Deleted, t2);

        var snapshot = log.Snapshot();
        snapshot[0].LastChangeType.ShouldBe(WatcherChangeTypes.Deleted);
    }

    [Fact]
    public void Path_matching_is_case_insensitive()
    {
        // Windows file paths mix-case between the watcher and OS; the dedup key has to collapse them.
        var log = new FileChangeLog();
        var at = DateTimeOffset.Parse("2026-04-17T10:00:00Z");

        log.Record(@"C:\Repo\A.md", WatcherChangeTypes.Changed, at);
        log.Record(@"c:\repo\a.md", WatcherChangeTypes.Changed, at.AddSeconds(1));

        log.Snapshot().Count.ShouldBe(1);
    }
}