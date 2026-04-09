using Pennington.FrontMatter;

namespace Pennington.Tests.FrontMatter;

public class FrontMatterParserTests
{
    private readonly FrontMatterParser _parser = new();

    private const string DocMarkdown = """
        ---
        title: Getting Started
        description: How to get started with Pennington
        isDraft: false
        tags: [routing, setup]
        section: Documentation
        order: 1
        ---
        # Getting Started

        Welcome to Pennington.
        """;

    private const string BlogMarkdown = """
        ---
        title: Announcing Pennington
        description: Our first blog post
        date: 2026-03-15
        author: Jane Doe
        series: Launch Week
        tags: [announcement, dotnet]
        ---
        # Announcing Pennington

        We are excited to announce Pennington.
        """;

    [Fact]
    public void Parse_DocFrontMatter_DeserializesAllFields()
    {
        var result = _parser.Parse<DocFrontMatter>(DocMarkdown);

        result.Metadata.ShouldNotBeNull();
        result.Metadata.Title.ShouldBe("Getting Started");
        result.Metadata.Description.ShouldBe("How to get started with Pennington");
        result.Metadata.IsDraft.ShouldBeFalse();
        result.Metadata.Tags.ShouldBe(new[] { "routing", "setup" });
        result.Metadata.Section.ShouldBe("Documentation");
        result.Metadata.Order.ShouldBe(1);
        result.Body.ShouldContain("# Getting Started");
        result.Body.ShouldContain("Welcome to Pennington.");
    }

    [Fact]
    public void Parse_BlogFrontMatter_DeserializesDateCorrectly()
    {
        var result = _parser.Parse<BlogFrontMatter>(BlogMarkdown);

        result.Metadata.ShouldNotBeNull();
        result.Metadata.Title.ShouldBe("Announcing Pennington");
        result.Metadata.Date.ShouldBe(new DateTime(2026, 3, 15));
        result.Metadata.Author.ShouldBe("Jane Doe");
        result.Metadata.Series.ShouldBe("Launch Week");
        result.Metadata.Tags.ShouldBe(new[] { "announcement", "dotnet" });
    }

    [Fact]
    public void Parse_NoFrontMatter_ReturnsNullMetadataAndFullBody()
    {
        var markdown = """
            # Just Markdown

            No front matter here.
            """;

        var result = _parser.Parse<DocFrontMatter>(markdown);

        result.Metadata.ShouldBeNull();
        result.Body.ShouldBe(markdown);
    }

    [Fact]
    public void Parse_EmptyFrontMatter_ReturnsDefaultMetadataWithBody()
    {
        var content = "---\n---\nSome content after empty front matter.";

        var result = _parser.Parse<DocFrontMatter>(content);

        result.Metadata.ShouldNotBeNull();
        result.Metadata.Title.ShouldBe("");
        result.Metadata.IsDraft.ShouldBeFalse();
        result.Metadata.Tags.ShouldBeEmpty();
        result.Body.ShouldBe("Some content after empty front matter.");
    }

    [Fact]
    public void Parse_UnknownFieldsAreIgnored()
    {
        var content = """
            ---
            title: Test Page
            unknownField: some value
            anotherExtra: 42
            ---
            Body content.
            """;

        var result = _parser.Parse<DocFrontMatter>(content);

        result.Metadata.ShouldNotBeNull();
        result.Metadata.Title.ShouldBe("Test Page");
        result.Body.ShouldContain("Body content.");
    }

    [Fact]
    public void Parse_DraftDetection_IsDraftTrue()
    {
        var content = """
            ---
            title: Draft Post
            isDraft: true
            ---
            Work in progress.
            """;

        var result = _parser.Parse<DocFrontMatter>(content);

        result.Metadata.ShouldNotBeNull();
        result.Metadata.IsDraft.ShouldBeTrue();
    }

    [Fact]
    public void Parse_TagsArray_DeserializesBothValues()
    {
        var content = """
            ---
            title: Tagged
            tags: [csharp, dotnet]
            ---
            Content.
            """;

        var result = _parser.Parse<DocFrontMatter>(content);

        result.Metadata.ShouldNotBeNull();
        result.Metadata.Tags.Length.ShouldBe(2);
        result.Metadata.Tags.ShouldContain("csharp");
        result.Metadata.Tags.ShouldContain("dotnet");
    }

    [Fact]
    public void Parse_MultilineBodyPreserved()
    {
        var content = "---\ntitle: Full Page\n---\n# Heading\n\nFirst paragraph.\n\nSecond paragraph.\n\n```csharp\nConsole.WriteLine(\"Hello\");\n```\n\n## Another Heading\n\nMore text.";

        var result = _parser.Parse<DocFrontMatter>(content);

        result.Metadata.ShouldNotBeNull();
        result.Metadata.Title.ShouldBe("Full Page");
        result.Body.ShouldContain("# Heading");
        result.Body.ShouldContain("First paragraph.");
        result.Body.ShouldContain("Second paragraph.");
        result.Body.ShouldContain("```csharp");
        result.Body.ShouldContain("Console.WriteLine(\"Hello\");");
        result.Body.ShouldContain("## Another Heading");
        result.Body.ShouldContain("More text.");
    }

    [Fact]
    public void Parse_MissingClosingDelimiter_ReturnsNullMetadataAndFullBody()
    {
        var content = """
            ---
            title: Unclosed
            description: No closing delimiter
            This is still in the front matter block... or is it?
            """;

        var result = _parser.Parse<DocFrontMatter>(content);

        result.Metadata.ShouldBeNull();
        result.Body.ShouldBe(content);
    }

    [Fact]
    public void Parse_YamlAnchor_ThrowsToPreventBillionLaughs()
    {
        var content = "---\ntitle: &bomb payload\n---\nContent.";

        Should.Throw<YamlDotNet.Core.YamlException>(
            () => _parser.Parse<DocFrontMatter>(content));
    }

    [Fact]
    public void DeserializeYaml_YamlAnchor_ThrowsToPreventBillionLaughs()
    {
        var yaml = "title: &bomb payload";

        Should.Throw<YamlDotNet.Core.YamlException>(
            () => _parser.DeserializeYaml<DocFrontMatter>(yaml));
    }

    [Fact]
    public void Parse_ArbitraryTypeTag_ThrowsToPreventTypeInstantiation()
    {
        var content = "---\ntitle: !<tag:example.org:attack> malicious\n---\nContent.";

        Should.Throw<YamlDotNet.Core.YamlException>(
            () => _parser.Parse<DocFrontMatter>(content));
    }

    [Fact]
    public void Parse_DotNetTypeTag_ThrowsToPreventTypeInstantiation()
    {
        var content = "---\ntitle: !<!System.Diagnostics.Process> evil\n---\nContent.";

        Should.Throw<YamlDotNet.Core.YamlException>(
            () => _parser.Parse<DocFrontMatter>(content));
    }

    [Fact]
    public void Parse_QuotedSpecialCharacters_NotAffectedBySecurityRestrictions()
    {
        var content = "---\ntitle: \"Anchors & aliases * are fine in strings\"\n---\nContent.";

        var result = _parser.Parse<DocFrontMatter>(content);

        result.Metadata.ShouldNotBeNull();
        result.Metadata.Title.ShouldBe("Anchors & aliases * are fine in strings");
    }
}
