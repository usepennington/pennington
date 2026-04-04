using Penn.FrontMatter;

namespace Penn.Tests.FrontMatter;

public class FrontMatterTests
{
    [Fact]
    public void PipelineCapabilityCheck_CastIFrontMatterToIDraftable()
    {
        IFrontMatter fm = new DocFrontMatter { IsDraft = true };
        if (fm is IDraftable draftable)
        {
            draftable.IsDraft.ShouldBeTrue();
        }
        else
        {
            throw new Xunit.Sdk.XunitException("Expected IFrontMatter to be IDraftable");
        }
    }

    [Fact]
    public void PipelineCapabilityCheck_NonDraftableReturnsFalse()
    {
        IFrontMatter fm = new MinimalFrontMatter("Test");
        (fm is IDraftable).ShouldBeFalse();
    }

    [Fact]
    public void CustomFrontMatter_MinimalImplementation()
    {
        var custom = new MinimalFrontMatter("Custom Title");
        (custom is IFrontMatter).ShouldBeTrue();
        custom.Title.ShouldBe("Custom Title");
    }

    [Fact]
    public void DocFrontMatter_RecordEquality()
    {
        var a = new DocFrontMatter { Title = "Test", Order = 5, Tags = ["a"] };
        var b = new DocFrontMatter { Title = "Test", Order = 5, Tags = ["a"] };
        // Record equality uses reference equality for arrays, so these are not equal
        a.ShouldNotBe(b);

        // But with the same array instance they are
        var tags = new[] { "a" };
        var c = new DocFrontMatter { Title = "Test", Order = 5, Tags = tags };
        var d = new DocFrontMatter { Title = "Test", Order = 5, Tags = tags };
        c.ShouldBe(d);
    }

    [Fact]
    public void BlogFrontMatter_RecordEquality()
    {
        var date = new DateTime(2026, 1, 1);
        var tags = new[] { "test" };
        var a = new BlogFrontMatter { Title = "Post", Date = date, Tags = tags };
        var b = new BlogFrontMatter { Title = "Post", Date = date, Tags = tags };
        a.ShouldBe(b);

        var c = new BlogFrontMatter { Title = "Post", Date = date, Tags = tags, Author = "X" };
        a.ShouldNotBe(c);
    }

    /// <summary>
    /// A minimal IFrontMatter implementation for testing capability checks.
    /// </summary>
    private record MinimalFrontMatter(string Title) : IFrontMatter;
}
