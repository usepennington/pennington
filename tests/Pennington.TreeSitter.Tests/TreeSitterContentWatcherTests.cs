namespace Pennington.TreeSitter.Tests;

using Pennington.Infrastructure;
using Pennington.TreeSitter;

public sealed class TreeSitterContentWatcherTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "pennington-ts-watch-" + Guid.NewGuid().ToString("N"));

    private string Root
    {
        get
        {
            Directory.CreateDirectory(_root);
            return Path.GetFullPath(_root);
        }
    }

    [Fact]
    public void Watches_each_source_glob_recursively_under_content_root()
    {
        var watcher = new TreeSitterContentWatcher(new TreeSitterOptions { ContentRoot = Root });

        watcher.WatchScopes.ShouldAllBe(s => s.Path == Root && s.IncludeSubdirectories);
        watcher.WatchScopes.Select(s => s.Pattern).ShouldBe(
            new[]
            {
                "*.cs", "*.py", "*.ts", "*.js", "*.jsx", "*.mjs", "*.cjs", "*.java", "*.rs", "*.go", "*.rb", "*.php",
                "*.html", "*.htm", "*.css", "*.json", "*.razor", "*.cshtml",
            },
            ignoreOrder: true);
    }

    [Fact]
    public void Markup_and_data_files_are_watched_for_whole_file_reload()
    {
        var watcher = new TreeSitterContentWatcher(new TreeSitterOptions { ContentRoot = Root });

        foreach (var file in new[] { "page.html", "styles.css", "data.json", "Counter.razor", "Index.cshtml" })
        {
            var change = new FileChangeNotification(Path.Combine(Root, "snippets", file), WatcherChangeTypes.Changed);
            watcher.WatchScopes.ShouldContain(s => s.Matches(change), $"expected a watch scope to cover {file}");
        }
    }

    [Fact]
    public void Source_file_matches_a_scope_at_any_depth()
    {
        var watcher = new TreeSitterContentWatcher(new TreeSitterOptions { ContentRoot = Root });

        var py = new FileChangeNotification(Path.Combine(Root, "proj", "snippets", "calc.py"), WatcherChangeTypes.Changed);
        watcher.WatchScopes.ShouldContain(s => s.Matches(py));
    }

    [Fact]
    public void Non_source_build_output_does_not_match_any_scope()
    {
        var watcher = new TreeSitterContentWatcher(new TreeSitterOptions { ContentRoot = Root });

        var dll = new FileChangeNotification(Path.Combine(Root, "proj", "bin", "App.dll"), WatcherChangeTypes.Changed);
        var pdb = new FileChangeNotification(Path.Combine(Root, "proj", "obj", "App.pdb"), WatcherChangeTypes.Created);
        var objDir = new FileChangeNotification(Path.Combine(Root, "proj", "obj"), WatcherChangeTypes.Changed);

        watcher.WatchScopes.ShouldNotContain(s => s.Matches(dll));
        watcher.WatchScopes.ShouldNotContain(s => s.Matches(pdb));
        watcher.WatchScopes.ShouldNotContain(s => s.Matches(objDir)); // a directory entry never matches a *.ext glob
    }

    [Fact]
    public void Json_under_build_output_still_matches_documented_tradeoff()
    {
        // *.json (and *.cs) are watched and also appear under obj/. A recursive watch can't exclude them;
        // the guidance is a focused content root (a snippets folder) when pointing at a source tree.
        var watcher = new TreeSitterContentWatcher(new TreeSitterOptions { ContentRoot = Root });

        var assets = new FileChangeNotification(Path.Combine(Root, "proj", "obj", "project.assets.json"), WatcherChangeTypes.Created);
        watcher.WatchScopes.ShouldContain(s => s.Matches(assets));
    }

    [Fact]
    public void Custom_patterns_are_honored()
    {
        var options = new TreeSitterOptions { ContentRoot = Root };
        options.WatchFilePatterns.Add("*.java");

        var watcher = new TreeSitterContentWatcher(options);

        var java = new FileChangeNotification(Path.Combine(Root, "Greeter.java"), WatcherChangeTypes.Changed);
        watcher.WatchScopes.ShouldContain(s => s.Matches(java));
    }

    [Fact]
    public void Missing_content_root_yields_no_scopes()
    {
        var watcher = new TreeSitterContentWatcher(
            new TreeSitterOptions { ContentRoot = Path.Combine(_root, "does-not-exist") });

        watcher.WatchScopes.ShouldBeEmpty();
    }

    [Fact]
    public void OnFileChanged_ignores_because_there_is_no_cache_to_refresh()
    {
        var watcher = new TreeSitterContentWatcher(new TreeSitterOptions { ContentRoot = Root });

        watcher.OnFileChanged(new FileChangeNotification(Path.Combine(Root, "x.py"), WatcherChangeTypes.Changed))
            .ShouldBe(FileWatchResponse.Ignore);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
