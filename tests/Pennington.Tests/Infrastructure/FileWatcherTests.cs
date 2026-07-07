using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Pennington.Infrastructure;
using Testably.Abstractions;
using Testably.Abstractions.Testing;

namespace Pennington.Tests.Infrastructure;

public class FileWatcherTests : IDisposable
{
    private readonly string _tempDir;

    public FileWatcherTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "penn-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); }
        catch { /* best effort cleanup */ }
    }

    [Fact]
    public async Task FileWatcher_NotifiesOnChange()
    {
        var ct = TestContext.Current.CancellationToken;
        var fs = new RealFileSystem();
        using var watcher = new FileWatcher(fs);
        var changedFile = "";
        var tcs = new TaskCompletionSource<bool>();

        var filePath = Path.Combine(_tempDir, "test.txt");
        await File.WriteAllTextAsync(filePath, "initial", ct);

        watcher.AddPathWatch(_tempDir, "*.txt", (path, type) =>
        {
            changedFile = path;
            tcs.TrySetResult(true);
        });

        await File.WriteAllTextAsync(filePath, "modified", ct);

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000, ct));
        completed.ShouldBe(tcs.Task, "FileWatcher callback should have fired");
        changedFile.ShouldContain("test.txt");
    }

    [Fact]
    public async Task SubscribeToChanges_NotifiesSubscribers()
    {
        var ct = TestContext.Current.CancellationToken;
        var fs = new RealFileSystem();
        using var watcher = new FileWatcher(fs);
        var subscriberCalled = false;
        var tcs = new TaskCompletionSource<bool>();

        var filePath = Path.Combine(_tempDir, "sub.txt");
        await File.WriteAllTextAsync(filePath, "initial", ct);

        watcher.AddPathWatch(_tempDir, "*.txt", (_, _) => { });
        watcher.SubscribeToChanges(() =>
        {
            subscriberCalled = true;
            tcs.TrySetResult(true);
        });

        await File.WriteAllTextAsync(filePath, "changed", ct);

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000, ct));
        completed.ShouldBe(tcs.Task, "Subscriber should have been notified");
        subscriberCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task SubscribeDuringNotification_DoesNotCorruptEnumeration()
    {
        // Regression: NotifySubscribers enumerates the subscriber list on the debounce timer
        // thread. A subscriber registering *during* that notification (AuditRunner at host start,
        // RedirectContentService on first request) used to mutate the backing List mid-foreach and
        // throw "Collection was modified" on that unobserved thread — a catastrophic, Linux-only CI
        // crash. Fire the debounce on the test thread via a fake clock so any fault surfaces here,
        // and assert the subscriber registered after the mid-notification one still runs.
        var ct = TestContext.Current.CancellationToken;
        var fs = new RealFileSystem();
        var clock = new FakeTimeProvider();
        using var watcher = new FileWatcher(fs, clock);

        var filePath = Path.Combine(_tempDir, "race.txt");
        await File.WriteAllTextAsync(filePath, "initial", ct);
        watcher.AddPathWatch(_tempDir, "*.txt", (_, _) => { });

        // This subscriber appends another from inside the notification — the exact
        // modify-during-enumeration the startup race produced, but deterministic.
        watcher.SubscribeToChanges(() => watcher.SubscribeToChanges(() => { }));
        // If that append corrupts the in-flight enumeration, this trailing subscriber never fires.
        var trailingFired = false;
        watcher.SubscribeToChanges(() => trailingFired = true);

        await File.WriteAllTextAsync(filePath, "changed", ct);

        // Wait for the OS watcher to buffer the change (it arms a debounce timer), then advance the
        // fake clock to fire that timer synchronously on this thread.
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!trailingFired && DateTime.UtcNow < deadline)
        {
            clock.Advance(TimeSpan.FromMilliseconds(150));
            if (trailingFired) { break; }
            await Task.Delay(20, ct);
        }

        trailingFired.ShouldBeTrue(
            "the subscriber registered after a mid-notification subscribe should still fire");
    }

    [Fact]
    public async Task FileWatcher_CoalescesRapidBurst_IntoSingleNotification()
    {
        // One logical save fans out into a truncate/write/close burst (plus Windows duplicate
        // events). The trailing-edge debounce must collapse the whole burst into a single
        // notification — not one per raw event.
        //
        // Drive the debounce off a fake clock so the assertion is deterministic. The raw OS events
        // still arrive in real time, but the debounce timer fires only when we advance the clock, so
        // while it stays frozen every event re-arms the same pending timer no matter how spread out
        // their delivery is. That removes the real-clock flake this replaces: a loaded runner that
        // dribbled the six events out over >100ms used to let the timer elapse mid-burst, splitting
        // it into two notifications.
        var ct = TestContext.Current.CancellationToken;
        var fs = new RealFileSystem();
        var clock = new FakeTimeProvider();
        using var watcher = new FileWatcher(fs, clock);
        var count = 0;

        var filePath = Path.Combine(_tempDir, "burst.txt");
        await File.WriteAllTextAsync(filePath, "v0", ct);

        watcher.AddPathWatch(_tempDir, "*.txt", (_, _) => { });
        watcher.SubscribeToChanges(() => System.Threading.Interlocked.Increment(ref count));

        // Tight synchronous burst.
        for (var i = 1; i <= 6; i++)
        {
            File.WriteAllText(filePath, $"v{i}");
        }

        // Let the OS watcher drain every raw event into the pending buffer. The frozen clock keeps
        // any of them from firing, so they collapse onto one re-armed timer; this wait only has to
        // outlast event *delivery*, not sit above the debounce window.
        await Task.Delay(500, ct);

        // Advance once past the 100ms debounce window: the single settled timer fires exactly once.
        clock.Advance(TimeSpan.FromMilliseconds(150));

        count.ShouldBe(1);
    }

    [Fact]
    public void AddPathWatch_NonExistentPath_DoesNotThrow()
    {
        var fs = new MockFileSystem();
        using var watcher = new FileWatcher(fs);

        // Should not throw — just logs a warning
        watcher.AddPathWatch("/nonexistent", "*.txt", (_, _) => { });
    }

    [Fact]
    public void AddPathWatch_DuplicateRegistration_IsIdempotent()
    {
        var fs = new RealFileSystem();
        using var watcher = new FileWatcher(fs);

        // Register twice with same path+pattern — second should be ignored
        watcher.AddPathWatch(_tempDir, "*.txt", (_, _) => { });
        watcher.AddPathWatch(_tempDir, "*.txt", (_, _) => { });

        // No exception means success — the duplicate was silently ignored
    }

    private static FileWatchDependencyFactory<T> CreateFactory<T>() where T : class, IFileWatchAware
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddProvider(NullLoggerProvider.Instance));
        var provider = services.BuildServiceProvider();
        return new FileWatchDependencyFactory<T>(
            provider,
            provider.GetRequiredService<ILogger<FileWatchDependencyFactory<T>>>());
    }

    [Fact]
    public void FileWatchDependencyFactory_CachesInstance()
    {
        using var factory = CreateFactory<RecreatingService>();

        factory.GetInstance().ShouldBeSameAs(factory.GetInstance());
    }

    [Fact]
    public void FileWatchDependencyFactory_DropsInstance_WhenInstanceAsksToRecreate()
    {
        using var factory = CreateFactory<RecreatingService>();

        var first = factory.GetInstance();
        factory.OnFileChanged(new FileChangeNotification("/x", WatcherChangeTypes.Changed))
            .ShouldBe(FileWatchResponse.Recreate);

        factory.GetInstance().ShouldNotBeSameAs(first);
    }

    [Fact]
    public void FileWatchDependencyFactory_KeepsInstance_WhenInstanceRefreshes()
    {
        using var factory = CreateFactory<RefreshingService>();

        var first = factory.GetInstance();
        factory.OnFileChanged(new FileChangeNotification("/x", WatcherChangeTypes.Changed))
            .ShouldBe(FileWatchResponse.Refreshed);

        factory.GetInstance().ShouldBeSameAs(first);
    }

    [Fact]
    public void FileWatchDependencyFactory_OnFileChanged_BeforeFirstResolve_ReturnsIgnore()
    {
        using var factory = CreateFactory<RecreatingService>();

        factory.OnFileChanged(new FileChangeNotification("/x", WatcherChangeTypes.Changed))
            .ShouldBe(FileWatchResponse.Ignore);
    }

    private sealed class RecreatingService : IFileWatchAware
    {
        public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Recreate;
    }

    private sealed class RefreshingService : IFileWatchAware
    {
        public FileWatchResponse OnFileChanged(FileChangeNotification change) => FileWatchResponse.Refreshed;
    }

    // --- Editor-backup / temp-file filter ---
    // FileWatcher drops common editor-transient filenames before publishing so
    // a single save no longer fans out as 6+ dispatches (the .md~ noise from
    // VS Code's atomic save, plus Windows FileSystemWatcher duplicate events).

    [Theory]
    [InlineData("doc.md~")]            // vim / nano / VS Code backup
    [InlineData("foo.swp")]            // vim swap
    [InlineData("foo.swo")]
    [InlineData("foo.SWX")]            // case-insensitive
    [InlineData("4913")]               // vim pre-write probe
    [InlineData(".#doc.md")]           // emacs lock
    [InlineData("~$report.docx")]      // Office lock
    [InlineData("scratch.tmp")]
    [InlineData("scratch.BAK")]
    public void IsEditorTempFile_RejectsKnownBackupPatterns(string fileName)
    {
        FileWatcher.IsEditorTempFile(fileName).ShouldBeTrue();
    }

    [Theory]
    [InlineData("doc.md")]
    [InlineData("_meta.yml")]
    [InlineData(".gitignore")]         // legitimate dotfile
    [InlineData("index.html")]
    [InlineData("scratch.razor")]
    [InlineData("")]
    public void IsEditorTempFile_AllowsLegitimateFiles(string fileName)
    {
        FileWatcher.IsEditorTempFile(fileName).ShouldBeFalse();
    }
}