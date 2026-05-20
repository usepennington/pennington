using Microsoft.AspNetCore.Components;
using Pennington.FrontMatter;
using Pennington.Taxonomy;

namespace Pennington.Tests.Taxonomy;

public class TaxonomyOptionsTests
{
    public sealed class IndexComponent : ComponentBase { }
    public sealed class TermComponent : ComponentBase { }
    public sealed class NotAComponent { }

    private static TaxonomyOptions<DocFrontMatter, string> ValidOptions() => new()
    {
        BaseUrl = "/cuisine",
        SelectKey = _ => "italian",
        IndexPage = typeof(IndexComponent),
        TermPage = typeof(TermComponent),
    };

    [Fact]
    public void Validate_AcceptsMinimalConfig()
    {
        Should.NotThrow(() => ValidOptions().Validate());
    }

    [Fact]
    public void Validate_RequiresBaseUrl()
    {
        var opts = ValidOptions();
        opts.BaseUrl = "";

        Should.Throw<InvalidOperationException>(() => opts.Validate())
            .Message.ShouldContain("BaseUrl");
    }

    [Fact]
    public void Validate_RequiresLeadingSlashOnBaseUrl()
    {
        var opts = ValidOptions();
        opts.BaseUrl = "cuisine";

        Should.Throw<InvalidOperationException>(() => opts.Validate())
            .Message.ShouldContain("must start with");
    }

    [Fact]
    public void Validate_RequiresExactlyOneSelector()
    {
        var noneOpts = ValidOptions();
        noneOpts.SelectKey = null;
        Should.Throw<InvalidOperationException>(() => noneOpts.Validate());

        var bothOpts = ValidOptions();
        bothOpts.SelectKeys = _ => ["a", "b"];
        Should.Throw<InvalidOperationException>(() => bothOpts.Validate());
    }

    [Fact]
    public void Validate_RequiresIndexAndTermPages()
    {
        var noIndex = ValidOptions();
        noIndex.IndexPage = null;
        Should.Throw<InvalidOperationException>(() => noIndex.Validate())
            .Message.ShouldContain("IndexPage");

        var noTerm = ValidOptions();
        noTerm.TermPage = null;
        Should.Throw<InvalidOperationException>(() => noTerm.Validate())
            .Message.ShouldContain("TermPage");
    }

    [Fact]
    public void Validate_RejectsNonComponentTypes()
    {
        var opts = ValidOptions();
        opts.IndexPage = typeof(NotAComponent);

        Should.Throw<InvalidOperationException>(() => opts.Validate())
            .Message.ShouldContain("IComponent");
    }

    [Fact]
    public void ResolvedSectionLabel_DerivesFromBaseUrl()
    {
        var opts = ValidOptions();
        opts.BaseUrl = "/cuisine";
        opts.ResolvedSectionLabel.ShouldBe("Cuisine");

        opts.BaseUrl = "/tag";
        opts.ResolvedSectionLabel.ShouldBe("Tag");
    }

    [Fact]
    public void ResolvedSectionLabel_PrefersExplicitOverride()
    {
        var opts = ValidOptions();
        opts.SectionLabel = "Cuisines of the World";
        opts.ResolvedSectionLabel.ShouldBe("Cuisines of the World");
    }

    [Fact]
    public void DefaultSlug_LowercasesAndHyphenatesSpaces()
    {
        var opts = new TaxonomyOptions<DocFrontMatter, string>
        {
            BaseUrl = "/cuisine",
            SelectKey = _ => "Northern Italian",
            IndexPage = typeof(IndexComponent),
            TermPage = typeof(TermComponent),
        };

        opts.SlugFor("Northern Italian").ShouldBe("northern-italian");
    }

    [Fact]
    public void TermUrl_ComposesBaseAndSlug()
    {
        var opts = new TaxonomyOptions<DocFrontMatter, string>
        {
            BaseUrl = "/cuisine",
            SelectKey = _ => "italian",
            IndexPage = typeof(IndexComponent),
            TermPage = typeof(TermComponent),
        };

        opts.TermUrl("italian").ShouldBe("/cuisine/italian/");
        opts.IndexUrl.ShouldBe("/cuisine/");
    }
}