using Pennington.Infrastructure;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Infrastructure;

public class FileWatchedValueTests
{
    [Fact]
    public void Value_DoesNotLoadUntilFirstAccess()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");

        var loads = 0;
        _ = new FileWatchedValue<int>(new FileWatcher(fs), "/data", "*.*", () => ++loads);

        loads.ShouldBe(0);
    }

    [Fact]
    public void Value_CachesAcrossReads()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");

        var loads = 0;
        var watched = new FileWatchedValue<int>(new FileWatcher(fs), "/data", "*.*", () => ++loads);

        watched.Value.ShouldBe(1);
        watched.Value.ShouldBe(1);
        loads.ShouldBe(1);
    }

    [Fact]
    public async Task Value_ReloadsAfterWatchedChange()
    {
        var fs = new MockFileSystem();
        fs.Directory.CreateDirectory("/data");

        var loads = 0;
        var watched = new FileWatchedValue<int>(new FileWatcher(fs), "/data", "*.*", () => ++loads);

        watched.Value.ShouldBe(1);

        fs.File.WriteAllText("/data/a.yml", "changed");

        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (watched.Value == 1 && DateTime.UtcNow < deadline)
            await Task.Delay(50, TestContext.Current.CancellationToken);

        watched.Value.ShouldBe(2);
    }
}
