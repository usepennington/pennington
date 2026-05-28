using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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