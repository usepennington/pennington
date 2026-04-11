using Pennington.MonorailCss;

namespace Pennington.Tests.MonorailCss;

public class MonorailCssTests
{
    [Fact]
    public async Task CssClassCollector_IsThreadSafe()
    {
        var collector = new CssClassCollector();
        var exceptions = new List<Exception>();

        // Use unique prefixes so we can verify our specific classes were added
        var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
        {
            try
            {
                collector.BeginProcessing();
                try
                {
                    collector.AddClasses($"/page-{i}", [$"threadsafe-{i}-a", $"threadsafe-{i}-b"]);
                }
                finally
                {
                    collector.EndProcessing();
                }
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        exceptions.ShouldBeEmpty();

        var classes = collector.GetClasses();
        // Verify all 20 unique classes from this test were added
        for (var i = 0; i < 10; i++)
        {
            classes.ShouldContain($"threadsafe-{i}-a");
            classes.ShouldContain($"threadsafe-{i}-b");
        }
    }

}
