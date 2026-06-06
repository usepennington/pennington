namespace Pennington.Head;

/// <summary>
/// Emits the llms.txt alternate-representation <c>&lt;link&gt;</c> so agents can discover the
/// machine-friendly site index. Registered only when llms.txt generation is enabled, replacing the
/// literal markup the site templates carried in their layout head.
/// </summary>
internal sealed class AlternateLinksHeadContributor : IHeadContributor
{
    /// <inheritdoc/>
    public int Order => HeadOrder.Site;

    /// <inheritdoc/>
    public bool ShouldContribute(HeadContext context) => true;

    /// <inheritdoc/>
    public Task ContributeAsync(HeadContext context, HeadBuilder head)
    {
        head.AddRepeatable(new HeadTag(new LinkTag("alternate", "/llms.txt")
        {
            Attributes = [new("type", "text/markdown"), new("title", "LLM-friendly site index")],
        }));
        return Task.CompletedTask;
    }
}
