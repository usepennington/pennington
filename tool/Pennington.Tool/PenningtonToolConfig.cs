using Pennington.BlogSite;
using Pennington.DocSite;
using Pennington.UI;
using Tomlyn;
using Tomlyn.Model;

namespace Pennington.Tool;

/// <summary>
/// Reads a <c>pennington.toml</c> file and projects it onto either <see cref="DocSiteOptions"/> or
/// <see cref="BlogSiteOptions"/>. The TOML's top-level <c>template</c> key (<c>"docs"</c> or
/// <c>"blog"</c>) selects which template the tool stands up; the shared <c>[site]</c> table plus a
/// template-specific section (<c>[[area]]</c> for docs, <c>[blog]</c> for blog) supply the rest.
/// </summary>
internal sealed class PenningtonToolConfig
{
    private readonly TomlTable _root;

    private PenningtonToolConfig(TomlTable root, string template)
    {
        _root = root;
        Template = template;
    }

    /// <summary>The selected template, normalized to <c>"docs"</c> or <c>"blog"</c>.</summary>
    public string Template { get; }

    /// <summary>Parses and validates the TOML at <paramref name="path"/>.</summary>
    public static PenningtonToolConfig Load(string path)
    {
        var model = Toml.ToModel(File.ReadAllText(path), path);
        var template = (GetString(model, "template") ?? "docs").Trim().ToLowerInvariant();
        if (template is not ("docs" or "blog"))
        {
            throw new InvalidOperationException(
                $"pennington.toml: 'template' must be \"docs\" or \"blog\" (got \"{template}\").");
        }

        return new PenningtonToolConfig(model, template);
    }

    /// <summary>Projects the <c>[site]</c> table plus <c>[[area]]</c> entries onto <see cref="DocSiteOptions"/>.</summary>
    public DocSiteOptions ToDocSiteOptions()
    {
        var site = GetTable(_root, "site");
        return new DocSiteOptions
        {
            SiteTitle = RequireString(site, "title", "site.title"),
            SiteDescription = RequireString(site, "description", "site.description"),
            CanonicalBaseUrl = GetString(site, "canonical_base_url"),
            ContentRootPath = GetString(site, "content_root") ?? "Content",
            GitHubUrl = GetString(site, "github_url"),
            HeaderContent = Markup(GetString(site, "header")),
            FooterContent = Markup(GetString(site, "footer")),
            SocialImageUrl = GetString(site, "social_image_url"),
            AdditionalHtmlHeadContent = GetString(site, "additional_head"),
            ExtraStyles = GetString(site, "extra_styles"),
            Areas = ReadAreas(),
        };
    }

    /// <summary>Projects the <c>[site]</c> and <c>[blog]</c> tables onto <see cref="BlogSiteOptions"/>.</summary>
    public BlogSiteOptions ToBlogSiteOptions()
    {
        var site = GetTable(_root, "site");
        var blog = GetTable(_root, "blog");
        return new BlogSiteOptions
        {
            SiteTitle = RequireString(site, "title", "site.title"),
            SiteDescription = RequireString(site, "description", "site.description"),
            CanonicalBaseUrl = GetString(site, "canonical_base_url"),
            ContentRootPath = GetString(site, "content_root") ?? "Content",
            AdditionalHtmlHeadContent = GetString(site, "additional_head"),
            ExtraStyles = GetString(site, "extra_styles"),
            BlogContentPath = GetString(blog, "content_path") ?? "Blog",
            BlogBaseUrl = GetString(blog, "base_url") ?? "/blog",
            PostsPerPage = (int)(GetLong(blog, "posts_per_page") ?? 10),
            AuthorName = GetString(blog, "author_name"),
            AuthorBio = GetString(blog, "author_bio"),
            EnableRss = GetBool(blog, "enable_rss") ?? true,
            EnableSitemap = GetBool(blog, "enable_sitemap") ?? true,
        };
    }

    private IReadOnlyList<ContentArea> ReadAreas()
    {
        if (_root.TryGetValue("area", out var value) && value is TomlTableArray areas)
        {
            return areas
                .Select(a => new ContentArea(
                    RequireString(a, "title", "area.title"),
                    RequireString(a, "slug", "area.slug")))
                .ToList();
        }

        return [];
    }

    private static MarkupContent? Markup(string? value) => value is { } html ? (MarkupContent?)html : null;

    private static string? GetString(TomlTable table, string key)
        => table.TryGetValue(key, out var v) && v is string s ? s : null;

    private static long? GetLong(TomlTable table, string key)
        => table.TryGetValue(key, out var v) && v is long l ? l : null;

    private static bool? GetBool(TomlTable table, string key)
        => table.TryGetValue(key, out var v) && v is bool b ? b : null;

    private static TomlTable GetTable(TomlTable table, string key)
        => table.TryGetValue(key, out var v) && v is TomlTable nested ? nested : new TomlTable();

    private static string RequireString(TomlTable table, string key, string path)
        => GetString(table, key)
           ?? throw new InvalidOperationException($"pennington.toml: missing required string '{path}'.");
}
