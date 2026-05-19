using Pennington.Infrastructure;

namespace Pennington.Tests.Infrastructure;

public class FileWatchedValueTests
{
    private static readonly FileWatchScope DataScope = new("/data", "*.*");

    [Fact]
    public void Value_DoesNotLoadUntilFirstAccess()
    {
        var loads = 0;
        _ = new FileWatchedValue<int>(DataScope, () => ++loads);

        loads.ShouldBe(0);
    }

    [Fact]
    public void Value_CachesAcrossReads()
    {
        var loads = 0;
        var watched = new FileWatchedValue<int>(DataScope, () => ++loads);

        watched.Value.ShouldBe(1);
        watched.Value.ShouldBe(1);
        loads.ShouldBe(1);
    }

    [Fact]
    public void OnFileChanged_MatchingScope_ReloadsOnNextAccess()
    {
        var loads = 0;
        var watched = new FileWatchedValue<int>(DataScope, () => ++loads);

        watched.Value.ShouldBe(1);

        var response = watched.OnFileChanged(new FileChangeNotification("/data/a.yml", WatcherChangeTypes.Changed));

        response.ShouldBe(FileWatchResponse.Refreshed);
        watched.Value.ShouldBe(2);
    }

    [Fact]
    public void OnFileChanged_NonMatchingScope_IsIgnored()
    {
        var loads = 0;
        var watched = new FileWatchedValue<int>(DataScope, () => ++loads);

        watched.Value.ShouldBe(1);

        var response = watched.OnFileChanged(new FileChangeNotification("/other/a.yml", WatcherChangeTypes.Changed));

        response.ShouldBe(FileWatchResponse.Ignore);
        watched.Value.ShouldBe(1);
    }

    [Fact]
    public void WatchScopes_ExposesItsScope()
    {
        var watched = new FileWatchedValue<int>(DataScope, () => 0);

        watched.WatchScopes.ShouldHaveSingleItem().ShouldBe(DataScope);
    }
}
