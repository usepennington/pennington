using System.Net;
using Beck;
using Beck.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pennington.Diagnostics;
using Pennington.Markdown.Extensions;
using YamlDotNet.RepresentationModel;

namespace Pennington.Beck;

/// <summary>
/// Renders a <c>```beck</c> fence to a static, self-animating inline <c>&lt;svg&gt;</c> at
/// render time via the pure-C# Beck engine — no client JavaScript.
///
/// Two fence forms are supported:
/// <list type="bullet">
///   <item><c>```beck</c> — the fence body is inline Beck YAML, rendered directly.</item>
///   <item><c>```beck:symbol</c> — the body is one file path per line (resolved against
///     <see cref="BeckOptions.ContentRoot"/>, like the tree-sitter <c>:symbol</c> embed),
///     whose YAML is read and rendered. A diagram can live in one shared <c>.beck.yaml</c>
///     and appear as both highlighted source and a live render from that single file.</item>
/// </list>
/// A comma-separated flag tail tunes the render: <c>```beck,static</c> forces the
/// fully-revealed static frame, <c>```beck,scrub</c> drives the choreography from scroll
/// position, and <c>```beck,style=sketch</c> overrides <c>meta.style</c> on the document
/// before rendering. Flags combine with the file-embed form (<c>```beck:symbol,static</c>).
///
/// <para>
/// The emitted SVG needs no wiring: it keys dark mode off the host's markers (the ancestor
/// <c>[data-theme]</c> by default; configurable via <c>SvgRenderOptions.ThemeHooks</c>)
/// and its <c>--beck-*</c> tokens fall back to the host's <c>--color-*</c> palette, so each
/// diagram adopts the live site colors. Each render lands in a
/// <c>&lt;div class="beck-embed"&gt;</c> (<c>beck-embed--error</c> on failure) for the host
/// to frame with its own CSS.
/// </para>
/// </summary>
public sealed class BeckCodeBlockPreprocessor : ICodeBlockPreprocessor
{
    private readonly BeckOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>Creates the preprocessor wired to the host's render options and the per-request diagnostics accessor.</summary>
    public BeckCodeBlockPreprocessor(BeckOptions options, IHttpContextAccessor httpContextAccessor)
    {
        _options = options;
        _httpContextAccessor = httpContextAccessor;
    }

    private DiagnosticContext? Diagnostics =>
        _httpContextAccessor.HttpContext?.RequestServices.GetService<DiagnosticContext>();

    /// <summary>Higher runs first; beats the tree-sitter preprocessor (100) so a <c>beck:symbol</c> fence is handled here, never mistaken for a source embed.</summary>
    public int Priority => 500;

    /// <inheritdoc />
    public CodeBlockPreprocessResult? TryProcess(string code, string languageId)
    {
        var fence = ParseInfo(languageId);
        if (!fence.IsBeck) return null; // defer every other fence to the next preprocessor

        // A `beck:symbol` fence body may name several files; each is its own document and
        // must render (and fail) independently — one malformed file must not drop the rest.
        List<string> yamls;
        try
        {
            yamls = fence.IsFileEmbed ? ReadEmbeddedYaml(code) : [code];
        }
        catch (Exception ex)
        {
            Diagnostics?.AddError($"beck fence render failed ({languageId}) — {ex.Message}");
            return new CodeBlockPreprocessResult(ErrorBox(code), "beck", SkipTransform: true, SkipChrome: true);
        }

        var html = string.Concat(yamls.Select(yaml => RenderOne(yaml, fence, languageId)));

        // SkipTransform + SkipChrome: the output is finished HTML, not a code block — the
        // annotation/highlight pass and the standard code-block chrome must not touch it.
        return new CodeBlockPreprocessResult(html, "beck", SkipTransform: true, SkipChrome: true);
    }

    private string RenderOne(string yaml, FenceInfo fence, string languageId)
    {
        try
        {
            if (fence.StyleName is { } style) yaml = ApplyStyle(yaml, style, languageId);
            string svg = BeckSvg.Render(yaml, OptionsFor(fence.Animation));
            return $"<div class=\"beck-embed\">{svg}{(_options.Zoom ? ZoomButton : string.Empty)}</div>";
        }
        catch (Exception ex)
        {
            // A malformed diagram (or a missing embed file) should fail loud (diagnostic +
            // a visible box), never silently vanish or crash the whole page render. Show the
            // failing document's own YAML, not the fence body (which for `:symbol` is file paths).
            Diagnostics?.AddError($"beck fence render failed ({languageId}) — {ex.Message}");
            return ErrorBox(yaml);
        }
    }

    /// <summary>
    /// Fullscreen-zoom affordance emitted after the SVG inside every successful embed when
    /// <see cref="BeckOptions.Zoom"/> is on. <see cref="BeckZoomHeadContributor"/> ships the
    /// delegated click handler that opens a <c>&lt;dialog class="beck-lightbox"&gt;</c> with a
    /// clone of the diagram, plus the CSS for both. The icon is an inline expand glyph so the
    /// button needs no asset.
    /// </summary>
    internal const string ZoomButton =
        "<button class=\"beck-zoom\" type=\"button\" aria-label=\"View diagram full screen\" title=\"View full screen\">"
        + "<svg viewBox=\"0 0 24 24\" width=\"14\" height=\"14\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\">"
        + "<path d=\"M15 3h6v6M9 21H3v-6M21 3l-7 7M3 21l7-7\"/></svg></button>";

    private static string ErrorBox(string yaml) =>
        "<div class=\"beck-embed beck-embed--error\"><pre><code>"
        + WebUtility.HtmlEncode(yaml) + "</code></pre></div>";

    /// <summary>The host's base render options with the fence's animation flag applied.</summary>
    private SvgRenderOptions OptionsFor(AnimationMode animation)
    {
        // Copies every host option except Animation (the per-fence override). When Beck grows a
        // new SvgRenderOptions member, it must be added here or fences silently drop it.
        var o = _options.RenderOptions;
        return new SvgRenderOptions
        {
            Measurer = o.Measurer,
            Font = o.Font,
            Theme = o.Theme,
            ThemeHooks = o.ThemeHooks,
            Animation = animation,
            TextLengthGuard = o.TextLengthGuard,
            Style = o.Style,
            Styles = o.Styles,
        };
    }

    /// <summary>
    /// Rewrites every document in <paramref name="yaml"/> so its <c>meta.style</c> is
    /// <paramref name="styleName"/> — the <c>style=</c> fence flag as a last-word override of
    /// whatever the document itself declares. An unknown style token warns and leaves the YAML
    /// untouched (the fence renders with its own style), matching how the rest of this
    /// preprocessor fails loud rather than silently. Editing the parsed representation graph
    /// (rather than a text splice) keeps this correct whether <c>meta</c> is block- or
    /// flow-styled, present or absent, in a single- or multi-document body.
    /// </summary>
    internal string ApplyStyle(string yaml, string styleName, string languageId)
    {
        bool known = BeckStyles.ByName.ContainsKey(styleName)
            || _options.RenderOptions.Styles?.ContainsKey(styleName) == true;
        if (!known)
        {
            Diagnostics?.AddWarning(
                $"beck: unknown style \"{styleName}\" in fence `{languageId}` — expected one of "
                + $"{string.Join(", ", BeckStyles.ByName.Keys)}. Rendering with the document's own style.");
            return yaml;
        }

        var stream = new YamlStream();
        stream.Load(new StringReader(yaml));
        if (stream.Documents.Count == 0) return yaml;

        foreach (var doc in stream.Documents)
        {
            if (doc.RootNode is not YamlMappingNode root) continue;
            var metaKey = new YamlScalarNode("meta");
            if (root.Children.TryGetValue(metaKey, out var node) && node is YamlMappingNode meta)
                meta.Children[new YamlScalarNode("style")] = new YamlScalarNode(styleName);
            else
                root.Children[metaKey] = new YamlMappingNode(
                    new YamlScalarNode("style"), new YamlScalarNode(styleName));
        }

        using var writer = new StringWriter();
        stream.Save(writer, assignAnchors: false);
        return writer.ToString();
    }

    /// <summary>
    /// Reads the YAML a <c>beck:symbol</c> fence points at. The body is one file path per
    /// line (a trailing <c>" &gt; symbol"</c> selector is ignored — whole-file YAML has no
    /// symbols), resolved against <see cref="BeckOptions.ContentRoot"/> so it matches the
    /// tree-sitter <c>:symbol</c> embed convention. Multiple paths each render as their own
    /// diagram — the caller renders every returned document independently so one bad file
    /// surfaces its own error box instead of dropping the rest.
    /// </summary>
    private List<string> ReadEmbeddedYaml(string code)
    {
        var paths = code
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(StripSelector)
            .Where(p => p.Length > 0)
            .ToList();
        if (paths.Count == 0)
            throw new InvalidOperationException("beck:symbol fence has no file path in its body.");

        return paths
            .Select(p => File.ReadAllText(Path.GetFullPath(Path.Combine(_options.ContentRoot, p))))
            .ToList();
    }

    /// <summary>Drops a <c>" &gt; member"</c> tail from a source reference, leaving the file path.</summary>
    private static string StripSelector(string line)
    {
        int cut = line.IndexOf(" > ", StringComparison.Ordinal);
        return (cut < 0 ? line : line[..cut]).Trim();
    }

    /// <summary>
    /// Parses a fence info-string such as <c>beck</c>, <c>beck:symbol</c>,
    /// <c>beck:symbol,static</c>, or <c>beck,style=sketch</c> into whether it is a Beck fence,
    /// whether the body is a file path (<c>:symbol</c>), the resolved <see cref="AnimationMode"/>,
    /// and an optional <c>style=</c> override. Non-Beck fences return <see cref="FenceInfo.IsBeck"/>
    /// false. Every token after the language is comma-separated — the <c>:symbol</c> modifier and
    /// the comma flag tail are parsed uniformly, so flags work on the inline form (<c>beck,static</c>)
    /// exactly as on the file-embed form (<c>beck:symbol,static</c>).
    /// </summary>
    private static FenceInfo ParseInfo(string languageId)
    {
        ReadOnlySpan<char> s = languageId.AsSpan().Trim();
        int ws = s.IndexOfAny(' ', '\t');
        if (ws >= 0) s = s[..ws];

        var segments = s.ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0) return FenceInfo.NotBeck;

        // The head segment is `beck` or `beck:<modifier>`; split off the colon modifier.
        ReadOnlySpan<char> head = segments[0].AsSpan();
        int colon = head.IndexOf(':');
        ReadOnlySpan<char> baseLang = colon >= 0 ? head[..colon] : head;
        if (!baseLang.Equals("beck", StringComparison.OrdinalIgnoreCase))
            return FenceInfo.NotBeck;

        // Flags = the colon modifier (if any) followed by every comma segment. Parsing them
        // through one path keeps `beck:symbol`, `beck:symbol,static`, and `beck,style=x` uniform.
        bool fileEmbed = false;
        var animation = AnimationMode.Full;
        string? styleName = null;

        if (colon >= 0) Apply(head[(colon + 1)..].ToString());
        for (int i = 1; i < segments.Length; i++) Apply(segments[i]);

        void Apply(string flag)
        {
            if (flag.Equals("symbol", StringComparison.OrdinalIgnoreCase)) fileEmbed = true;
            else if (flag.Equals("static", StringComparison.OrdinalIgnoreCase)) animation = AnimationMode.Static;
            else if (flag.Equals("scrub", StringComparison.OrdinalIgnoreCase)) animation = AnimationMode.Scrub;
            else if (flag.StartsWith("style=", StringComparison.OrdinalIgnoreCase))
                styleName = flag["style=".Length..].Trim();
            // unknown flags are ignored, matching the tree-sitter flag parser
        }

        return new FenceInfo(true, fileEmbed, animation, styleName);
    }

    private readonly record struct FenceInfo(bool IsBeck, bool IsFileEmbed, AnimationMode Animation, string? StyleName)
    {
        public static readonly FenceInfo NotBeck = new(false, false, AnimationMode.Full, null);
    }
}
