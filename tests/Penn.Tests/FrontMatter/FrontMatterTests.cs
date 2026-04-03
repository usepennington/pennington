using Penn.FrontMatter;

namespace Penn.Tests.FrontMatter;

public class FrontMatterTests
{
    [Fact]
    public void DocFrontMatter_ImplementsIFrontMatter()
    {
        var doc = new DocFrontMatter { Title = "Hello" };
        (doc is IFrontMatter).ShouldBeTrue();
        ((IFrontMatter)doc).Title.ShouldBe("Hello");
    }

    [Fact]
    public void DocFrontMatter_ImplementsAllCapabilityInterfaces()
    {
        var doc = new DocFrontMatter();
        (doc is IDraftable).ShouldBeTrue();
        (doc is ITaggable).ShouldBeTrue();
        (doc is ISectionable).ShouldBeTrue();
        (doc is ICrossReferenceable).ShouldBeTrue();
        (doc is IOrderable).ShouldBeTrue();
        (doc is IDescribable).ShouldBeTrue();
    }

    [Fact]
    public void DocFrontMatter_DefaultValues()
    {
        var doc = new DocFrontMatter();
        doc.Title.ShouldBe("");
        doc.Description.ShouldBeNull();
        doc.IsDraft.ShouldBeFalse();
        doc.Tags.ShouldBeEmpty();
        doc.Section.ShouldBeNull();
        doc.Uid.ShouldBeNull();
        doc.Order.ShouldBe(int.MaxValue);
    }

    [Fact]
    public void DocFrontMatter_WithInit_AllPropertiesReadBack()
    {
        var doc = new DocFrontMatter
        {
            Title = "Getting Started",
            Description = "A guide to getting started",
            IsDraft = true,
            Tags = ["guide", "intro"],
            Section = "tutorials",
            Uid = "getting-started",
            Order = 1
        };

        doc.Title.ShouldBe("Getting Started");
        doc.Description.ShouldBe("A guide to getting started");
        doc.IsDraft.ShouldBeTrue();
        doc.Tags.ShouldBe(new[] { "guide", "intro" });
        doc.Section.ShouldBe("tutorials");
        doc.Uid.ShouldBe("getting-started");
        doc.Order.ShouldBe(1);
    }

    [Fact]
    public void BlogFrontMatter_ImplementsIFrontMatter()
    {
        var blog = new BlogFrontMatter { Title = "My Post" };
        (blog is IFrontMatter).ShouldBeTrue();
        ((IFrontMatter)blog).Title.ShouldBe("My Post");
    }

    [Fact]
    public void BlogFrontMatter_ImplementsAllCapabilityInterfaces()
    {
        var blog = new BlogFrontMatter();
        (blog is IDraftable).ShouldBeTrue();
        (blog is ITaggable).ShouldBeTrue();
        (blog is IDescribable).ShouldBeTrue();
        (blog is IDateable).ShouldBeTrue();
        (blog is ICrossReferenceable).ShouldBeTrue();
    }

    [Fact]
    public void BlogFrontMatter_DefaultValues()
    {
        var blog = new BlogFrontMatter();
        blog.Title.ShouldBe("");
        blog.Description.ShouldBeNull();
        blog.IsDraft.ShouldBeFalse();
        blog.Tags.ShouldBeEmpty();
        blog.Date.ShouldBeNull();
        blog.Author.ShouldBeNull();
        blog.Series.ShouldBeNull();
        blog.Uid.ShouldBeNull();
    }

    [Fact]
    public void BlogFrontMatter_WithInit_AllPropertiesReadBack()
    {
        var date = new DateTime(2026, 3, 15);
        var blog = new BlogFrontMatter
        {
            Title = "Hello World",
            Description = "My first post",
            IsDraft = true,
            Tags = ["csharp", "dotnet"],
            Date = date,
            Author = "Jane Doe",
            Series = "Getting Started",
            Uid = "hello-world"
        };

        blog.Title.ShouldBe("Hello World");
        blog.Description.ShouldBe("My first post");
        blog.IsDraft.ShouldBeTrue();
        blog.Tags.ShouldBe(new[] { "csharp", "dotnet" });
        blog.Date.ShouldBe(date);
        blog.Author.ShouldBe("Jane Doe");
        blog.Series.ShouldBe("Getting Started");
        blog.Uid.ShouldBe("hello-world");
    }

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
