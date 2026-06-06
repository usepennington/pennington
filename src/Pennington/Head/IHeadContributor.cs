namespace Pennington.Head;

/// <summary>
/// Contributes tags to the document <c>&lt;head&gt;</c>. Every contributor feeds a single
/// <see cref="HeadBuilder"/>; the composed result is reconciled into the DOM once by the head
/// composition rewriter. Contributors never touch the DOM directly — they emit typed
/// <see cref="HeadTag"/>s, and dedup/ordering is handled centrally.
/// </summary>
public interface IHeadContributor
{
    /// <summary>
    /// Ascending priority (use the <see cref="HeadOrder"/> bands). Contributors run lowest-first,
    /// and on a <see cref="HeadTagKey"/> collision the lowest order wins.
    /// </summary>
    int Order { get; }

    /// <summary>Cheap gate. Return <c>false</c> to skip <see cref="ContributeAsync"/> entirely.</summary>
    bool ShouldContribute(HeadContext context);

    /// <summary>Pushes tags into the builder for this request.</summary>
    Task ContributeAsync(HeadContext context, HeadBuilder head);
}
