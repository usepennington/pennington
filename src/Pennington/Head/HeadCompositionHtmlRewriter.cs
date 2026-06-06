namespace Pennington.Head;

using AngleSharp.Dom;
using Content;
using Infrastructure;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Composes every <see cref="IHeadContributor"/> and reconciles the result into the already-parsed
/// document head — the single place head tags are finalized. Runs inside the shared
/// <see cref="HtmlResponseRewritingProcessor"/> pass (no extra parse/serialize cycle).
/// <para>
/// Order 25 places it after locale rewriting (20) but before base-URL prefixing (30), so emitted
/// root-relative asset/alternate hrefs are sub-path prefixed by <see cref="BaseUrlHtmlRewriter"/>
/// exactly as literal <c>&lt;head&gt;</c> markup is. Tags whose keys a page (via <c>HeadOutlet</c>)
/// already authored with content are left alone; contributors only fill gaps.
/// </para>
/// </summary>
internal sealed class HeadCompositionHtmlRewriter : IHtmlResponseRewriter
{
    private readonly IReadOnlyList<IHeadContributor> _contributors;
    private readonly ContentRecordRegistry _records;

    /// <summary>Creates the rewriter from the registered contributors and the record registry.</summary>
    public HeadCompositionHtmlRewriter(IEnumerable<IHeadContributor> contributors, ContentRecordRegistry records)
    {
        _contributors = contributors.OrderBy(c => c.Order).ToArray();
        _records = records;
    }

    /// <inheritdoc/>
    public int Order => 25;

    /// <inheritdoc/>
    public bool ShouldApply(HttpContext context) => _contributors.Count > 0;

    /// <inheritdoc/>
    public async Task ApplyAsync(IDocument document, HttpContext context)
    {
        var head = document.Head;
        if (head is null)
        {
            return;
        }

        var fullPath = (context.Request.PathBase + context.Request.Path).ToString();
        var ctx = new HeadContext
        {
            HttpContext = context,
            FullPath = fullPath,
            Record = await ResolveRecordAsync(fullPath),
        };

        var builder = new HeadBuilder();
        foreach (var contributor in _contributors)
        {
            if (contributor.ShouldContribute(ctx))
            {
                await contributor.ContributeAsync(ctx, builder);
            }
        }

        foreach (var entry in builder.Build())
        {
            Reconcile(document, head, entry);
        }

        // Stamp page-authored head tags (HeadContent / literal markup) the engine keeps in sync, so
        // the generalized [data-head] SPA sweep covers them too — the single normalization point for
        // the whole head, regardless of whether a tag came from a contributor or a Razor page.
        NormalizeExisting(head);
    }

    // Page-authored head tags kept in sync across SPA navigation (formerly the spa-engine.js swap()
    // allowlist; now expressed as data-head stamps reconciled by one generic client sweep).
    private static readonly string[] ManagedSelectors =
    [
        "meta[name=\"description\"]",
        "meta[property^=\"og:\"]",
        "meta[name^=\"twitter:\"]",
        "link[rel=\"canonical\"]",
        "link[rel=\"alternate\"][hreflang]",
        "script[type=\"application/ld+json\"]",
    ];

    private static void NormalizeExisting(IElement head)
    {
        foreach (var selector in ManagedSelectors)
        {
            foreach (var element in head.QuerySelectorAll(selector))
            {
                if (!element.HasAttribute("data-head"))
                {
                    element.SetAttribute("data-head", MarkerFor(element));
                }
            }
        }
    }

    private static string MarkerFor(IElement element) => element.LocalName switch
    {
        "meta" => element.GetAttribute("property") is { } property
            ? $"meta:prop:{property}"
            : $"meta:name:{element.GetAttribute("name")}",
        "link" => $"link:rel:{element.GetAttribute("rel")}",
        "script" => $"script:{element.GetAttribute("type")}",
        _ => "head",
    };

    private async Task<ContentRecord?> ResolveRecordAsync(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
        {
            return null;
        }

        var snapshot = await _records.GetSnapshotAsync();
        return snapshot.TryGetValue(fullPath.Trim('/'), out var record) ? record : null;
    }

    private static void Reconcile(IDocument document, IElement head, HeadEntry entry)
    {
        // Repeatable (keyless) tags always append.
        if (entry.Key is not { } key)
        {
            AppendTag(document, head, entry.Tag, GroupMarker(entry.Tag));
            return;
        }

        var selector = SelectorFor(key);
        if (selector is not null && head.QuerySelector(selector) is { } existing)
        {
            if (HasContent(existing))
            {
                // Page (HeadOutlet) or an earlier rewriter authored this key with content — it wins.
                // Stamp it so the SPA engine syncs it across soft navigation like our own tags.
                existing.SetAttribute("data-head", key.Value);
                return;
            }

            // Existing tag is an empty placeholder — fill it in.
            if (CreateElement(document, entry.Tag) is { } replacement)
            {
                replacement.SetAttribute("data-head", key.Value);
                existing.Replace(replacement);
            }

            return;
        }

        AppendTag(document, head, entry.Tag, key.Value);
    }

    private static void AppendTag(IDocument document, IElement head, HeadTag tag, string dataHead)
    {
        if (tag.Value is RawTag raw)
        {
            head.Insert(AdjacentPosition.BeforeEnd, raw.Html);
            return;
        }

        if (CreateElement(document, tag) is { } element)
        {
            element.SetAttribute("data-head", dataHead);
            head.AppendChild(element);
        }
    }

    private static IElement? CreateElement(IDocument document, HeadTag tag) => tag.Value switch
    {
        TitleTag t => Title(document, t),
        MetaNameTag m => Meta(document, "name", m.Name, m.Content),
        MetaPropertyTag m => Meta(document, "property", m.Property, m.Content),
        LinkTag l => Link(document, l),
        ScriptTag s => Script(document, s),
        _ => null,
    };

    private static IElement Title(IDocument document, TitleTag tag)
    {
        var element = document.CreateElement("title");
        element.TextContent = tag.Text;
        return element;
    }

    private static IElement Meta(IDocument document, string keyAttr, string keyValue, string content)
    {
        var element = document.CreateElement("meta");
        element.SetAttribute(keyAttr, keyValue);
        element.SetAttribute("content", content);
        return element;
    }

    private static IElement Link(IDocument document, LinkTag tag)
    {
        var element = document.CreateElement("link");
        element.SetAttribute("rel", tag.Rel);
        element.SetAttribute("href", tag.Href);
        foreach (var (name, value) in tag.Attributes)
        {
            element.SetAttribute(name, value ?? "");
        }

        return element;
    }

    private static IElement Script(IDocument document, ScriptTag tag)
    {
        var element = document.CreateElement("script");
        if (tag.Type is { } type)
        {
            element.SetAttribute("type", type);
        }

        if (tag.Src is { } src)
        {
            element.SetAttribute("src", src);
            if (tag.Defer)
            {
                element.SetAttribute("defer", "");
            }
        }
        else if (tag.InlineBody is { } body)
        {
            element.TextContent = body;
        }

        return element;
    }

    private static bool HasContent(IElement element) => element.LocalName switch
    {
        "title" => !string.IsNullOrWhiteSpace(element.TextContent),
        "meta" => !string.IsNullOrEmpty(element.GetAttribute("content")),
        _ => true,
    };

    private static string? SelectorFor(HeadTagKey key)
    {
        var value = key.Value;
        if (value == "title")
        {
            return "title";
        }

        if (value.StartsWith("meta:name:", StringComparison.Ordinal))
        {
            return $"meta[name=\"{value["meta:name:".Length..]}\"]";
        }

        if (value.StartsWith("meta:prop:", StringComparison.Ordinal))
        {
            return $"meta[property=\"{value["meta:prop:".Length..]}\"]";
        }

        if (value.StartsWith("link:rel:", StringComparison.Ordinal))
        {
            return $"link[rel=\"{value["link:rel:".Length..]}\"]";
        }

        return null;
    }

    private static string GroupMarker(HeadTag tag) => tag.Value switch
    {
        TitleTag => "title",
        MetaNameTag m => $"meta:name:{m.Name}",
        MetaPropertyTag m => $"meta:prop:{m.Property}",
        LinkTag l => $"link:rel:{l.Rel}",
        ScriptTag s => s.Type is { } type ? $"script:{type}" : "script",
        _ => "raw",
    };
}
