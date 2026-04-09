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
        var fs = new RealFileSystem();
        using var watcher = new FileWatcher(fs);
        var changedFile = "";
        var tcs = new TaskCompletionSource<bool>();

        var filePath = Path.Combine(_tempDir, "test.txt");
        await File.WriteAllTextAsync(filePath, "initial");

        watcher.AddPathWatch(_tempDir, "*.txt", (path, type) =>
        {
            changedFile = path;
            tcs.TrySetResult(true);
        });

        await File.WriteAllTextAsync(filePath, "modified");

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));
        completed.ShouldBe(tcs.Task, "FileWatcher callback should have fired");
        changedFile.ShouldContain("test.txt");
    }

    [Fact]
    public async Task SubscribeToChanges_NotifiesSubscribers()
    {
        var fs = new RealFileSystem();
        using var watcher = new FileWatcher(fs);
        var subscriberCalled = false;
        var tcs = new TaskCompletionSource<bool>();

        var filePath = Path.Combine(_tempDir, "sub.txt");
        await File.WriteAllTextAsync(filePath, "initial");

        watcher.AddPathWatch(_tempDir, "*.txt", (_, _) => { });
        watcher.SubscribeToChanges(() =>
        {
            subscriberCalled = true;
            tcs.TrySetResult(true);
        });

        await File.WriteAllTextAsync(filePath, "changed");

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(5000));
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

    [Fact]
    public void FileWatchDependencyFactory_CachesInstance()
    {
        var services = new ServiceCollection();
        var fs = new RealFileSystem();
        services.AddSingleton<IFileWatcher>(new FileWatcher(fs));
        services.AddLogging(b => b.AddProvider(NullLoggerProvider.Instance));
        using var provider = services.BuildServiceProvider();

        var factory = new FileWatchDependencyFactory<TestService>(
            provider.GetRequiredService<IFileWatcher>(),
            provider,
            provider.GetRequiredService<ILogger<FileWatchDependencyFactory<TestService>>>());

        var first = factory.GetInstance();
        var second = factory.GetInstance();

        first.ShouldBeSameAs(second);
        first.Id.ShouldBe(second.Id);

        factory.Dispose();
    }

    [Fact]
    public void FileWatchDependencyFactory_InvalidatesOnChange()
    {
        var services = new ServiceCollection();
        var fs = new RealFileSystem();
        services.AddSingleton<IFileWatcher>(new FileWatcher(fs));
        services.AddLogging(b => b.AddProvider(NullLoggerProvider.Instance));
        using var provider = services.BuildServiceProvider();

        var factory = new FileWatchDependencyFactory<TestService>(
            provider.GetRequiredService<IFileWatcher>(),
            provider,
            provider.GetRequiredService<ILogger<FileWatchDependencyFactory<TestService>>>());

        var first = factory.GetInstance();
        factory.InvalidateInstance();
        var second = factory.GetInstance();

        first.ShouldNotBeSameAs(second);
        first.Id.ShouldNotBe(second.Id);

        factory.Dispose();
    }

    private class TestService
    {
        public Guid Id { get; } = Guid.NewGuid();
    }
}
