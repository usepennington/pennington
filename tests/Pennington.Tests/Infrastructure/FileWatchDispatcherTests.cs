using Pennington.Infrastructure;

namespace Pennington.Tests.Infrastructure;

public class FileWatchDispatcherTests
{
    private sealed class FakeFileWatcher : IFileWatcher
    {
        private readonly List<Action<FileChangeNotification>> _subscribers = [];

        public List<(string Path, string Pattern, bool IncludeSubdirectories)> Watches { get; } = [];

        public void AddPathWatch(string path, string filePattern,
            Action<string, WatcherChangeTypes> onFileChanged, bool includeSubdirectories = true)
            => Watches.Add((path, filePattern, includeSubdirectories));

        public void SubscribeToChanges(Action onUpdate) { }

        public void SubscribeToChanges(Action<FileChangeNotification> onUpdate) => _subscribers.Add(onUpdate);

        public void Fire(FileChangeNotification change)
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber(change);
            }
        }

        public void Dispose() { }
    }

    private sealed class FakeAware(FileWatchResponse response, params FileWatchScope[] scopes) : IFileWatchAware
    {
        public int Calls { get; private set; }

        public IReadOnlyList<FileWatchScope> WatchScopes { get; } = scopes;

        public FileWatchResponse OnFileChanged(FileChangeNotification change)
        {
            Calls++;
            return response;
        }
    }

    [Fact]
    public void Constructor_RegistersEveryDeclaredScope()
    {
        var watcher = new FakeFileWatcher();
        var aware = new FakeAware(FileWatchResponse.Refreshed, new FileWatchScope("/data", "*.yml"));

        _ = new FileWatchDispatcher([aware], watcher);

        watcher.Watches.ShouldHaveSingleItem().ShouldBe(("/data", "*.yml", false));
    }

    [Fact]
    public void Change_FansOutToEveryAware()
    {
        var watcher = new FakeFileWatcher();
        var first = new FakeAware(FileWatchResponse.Refreshed);
        var second = new FakeAware(FileWatchResponse.Ignore);

        _ = new FileWatchDispatcher([first, second], watcher);
        watcher.Fire(new FileChangeNotification("/data/a.yml", WatcherChangeTypes.Changed));

        first.Calls.ShouldBe(1);
        second.Calls.ShouldBe(1);
    }
}