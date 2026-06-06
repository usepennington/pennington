using Pennington.Head;

namespace Pennington.Tests.Head;

/// <summary>Unit tests for <see cref="HeadBuilder"/> dedup and ordering semantics.</summary>
public class HeadBuilderTests
{
    [Fact]
    public void Title_FirstAddWins()
    {
        var builder = new HeadBuilder();
        builder.Title("First");
        builder.Title("Second");

        var entry = builder.Build().ShouldHaveSingleItem();
        (entry.Tag.Value as TitleTag).ShouldNotBeNull().Text.ShouldBe("First");
    }

    [Fact]
    public void Meta_DedupesByName()
    {
        var builder = new HeadBuilder();
        builder.Meta("description", "one");
        builder.Meta("description", "two");
        builder.Meta("keywords", "k");

        builder.Build().Count.ShouldBe(2);
    }

    [Fact]
    public void Repeatable_TagsAllAppend()
    {
        var builder = new HeadBuilder();
        builder.AddRepeatable(new HeadTag(new LinkTag("alternate", "/a")));
        builder.AddRepeatable(new HeadTag(new LinkTag("alternate", "/b")));

        builder.Build().Count.ShouldBe(2);
    }

    [Fact]
    public void KeyedTags_OrderBeforeKeylessTags()
    {
        var builder = new HeadBuilder();
        builder.AddRepeatable(new HeadTag(new LinkTag("alternate", "/feed")));
        builder.Title("Title");

        var entries = builder.Build();
        entries[0].Key.ShouldNotBeNull();
        entries[1].Key.ShouldBeNull();
    }
}
