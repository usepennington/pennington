using System.Collections.Immutable;
using MonorailCss;
using MonorailCss.Parser.Custom;

namespace Pennington.MonorailCss.Internal;

/// <summary>
/// Default <c>@apply</c> blocks and custom utilities Pennington layers on top of the MonorailCSS
/// framework. Authored as static dictionaries so the literal selectors and class strings are
/// visible to source review and to the discovery pipeline's IL scan.
/// </summary>
internal static class PenningtonApplies
{
    /// <summary>
    /// Returns every <c>@apply</c> block Pennington registers, parameterised by the
    /// <see cref="SyntaxTheme"/> in effect (only the <c>.hljs-*</c> rules vary).
    /// </summary>
    public static ImmutableDictionary<string, string> All(SyntaxTheme syntax) =>
        ImmutableDictionary<string, string>.Empty
            .AddRange(ProsePseudoApplies)
            .AddRange(CodeBlockApplies)
            .AddRange(TabApplies)
            .AddRange(MarkdownAlertApplies)
            .AddRange(StepApplies)
            .AddRange(HljsApplies(syntax))
            .AddRange(SearchModalApplies);

    // Prose pseudo-elements live here rather than in PenningtonProseRules because
    // MonorailCSS's prose customization currently strips pseudo-element selectors.
    // The H2 accent bar and the bullet dot are visual rhythm chrome; they must
    // render to land the doc-site article aesthetic.
    private static readonly ImmutableDictionary<string, string> ProsePseudoApplies =
        ImmutableDictionary.CreateRange(new Dictionary<string, string>
        {
            {
                ".prose h2",
                "before:content-[''] before:absolute before:left-0 before:top-0 before:bottom-0 before:w-[4px] before:rounded-sm before:bg-gradient-to-b before:from-primary-500 before:to-primary-700 dark:before:from-primary-300 dark:before:to-primary-500"
            },
            {
                ".prose ul > li",
                "before:content-[''] before:absolute before:left-[0.35rem] before:top-[0.7em] before:w-[5px] before:h-[5px] before:rounded-full before:bg-primary-500/55 dark:before:bg-primary-300/55"
            },
        });

    /// <summary>
    /// Custom utilities (scrollbar styling) registered with the MonorailCSS framework.
    /// </summary>
    public static readonly ImmutableList<UtilityDefinition> ScrollbarUtilities = ImmutableList.Create(
        new UtilityDefinition
        {
            Pattern = "scrollbar-thin",
            Declarations = ImmutableList.Create(
                new CssDeclaration("scrollbar-width", "thin")),
        },
        new UtilityDefinition
        {
            Pattern = "scrollbar-thumb-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-thumb-color", "--value(--color-*)")),
        },
        new UtilityDefinition
        {
            Pattern = "scrollbar-track-*",
            IsWildcard = true,
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-track-color", "--value(--color-*)")),
        },
        new UtilityDefinition
        {
            Pattern = "scrollbar-track-transparent",
            Declarations = ImmutableList.Create(
                new CssDeclaration("--tw-scrollbar-track-color", "transparent")),
        },
        new UtilityDefinition
        {
            Pattern = "scrollbar-color",
            Declarations = ImmutableList.Create(
                new CssDeclaration(
                    "scrollbar-color",
                    "var(--tw-scrollbar-thumb-color) var(--tw-scrollbar-track-color)")),
        });

    private static readonly ImmutableDictionary<string, string> CodeBlockApplies =
        ImmutableDictionary.CreateRange(new Dictionary<string, string>
        {
            // Outer chrome — rounded card with subtle border and surface tint.
            // Dark-mode surface uses base-900 so code reads against a slightly
            // raised plane against the page's base-950 background.
            {
                ".code-highlight-wrapper",
                "my-6 rounded-xl border border-base-200 bg-base-50 dark:bg-base-900 dark:border-base-800 overflow-hidden"
            },
            {
                ".code-highlight-wrapper .standalone-code-container",
                "overflow-x-auto"
            },

            // Head bar — language label sits above the body. Standalone blocks only;
            // tabbed children inherit their own head from the surrounding tab list.
            {
                ".code-highlight-wrapper .codeblock-head",
                "flex items-center justify-between px-4 py-1.5 border-b border-base-200 dark:border-base-800 bg-base-100 dark:bg-base-900 font-mono text-[12px]"
            },
            {
                ".code-highlight-wrapper .codeblock-lang",
                "text-base-500 dark:text-base-400"
            },

            {
                ".code-highlight-wrapper pre ",
                "py-4 px-4 md:px-5 overflow-x-auto font-mono text-[13px] leading-[1.6] w-full dark:scheme-dark"
            },
            {
                ".code-highlight-wrapper pre code",
                "font-mono inline-block min-w-full"
            },

            // Code transformation line containers
            {
                ".code-highlight-wrapper .line",
                "inline-block transition-all duration-300 lg:py-[1px] px-5 -mx-5 w-[calc(100%+2.5rem)] relative"
            },
            {
                ".code-highlight-wrapper pre.has-focused .line",
                "blur-[0.05rem] opacity-75"
            },
            {
                ".code-highlight-wrapper pre.has-focused:hover .line",
                "blur-[0] opacity-100"
            },

            // Line highlighting
            {
                ".code-highlight-wrapper .line.highlight",
                "bg-primary-700/15 dark:bg-primary-500/15"
            },

            // Diff notation — GitHub-style green/red lines with inset gutter bar
            // and a +/- glyph in the left margin. Colors chosen to match GitHub's
            // accessibility palette so the diff is legible in both themes.
            {
                ".code-highlight-wrapper .line.diff-add",
                "bg-emerald-500/10 dark:bg-emerald-500/15 shadow-[inset_2px_0_0_var(--color-emerald-600)] dark:shadow-[inset_2px_0_0_var(--color-emerald-400)] before:absolute before:left-1 before:font-semibold before:content-['+'] before:text-emerald-700 dark:before:text-emerald-400"
            },
            {
                ".code-highlight-wrapper .line.diff-remove",
                "bg-rose-500/10 dark:bg-rose-500/15 shadow-[inset_2px_0_0_var(--color-rose-600)] dark:shadow-[inset_2px_0_0_var(--color-rose-400)] before:absolute before:left-1 before:font-semibold before:content-['-'] before:text-rose-700 dark:before:text-rose-400"
            },
            {
                ".code-highlight-wrapper .line.diff-remove > *",
                "opacity-60"
            },

            // Focus and blur
            {
                ".code-highlight-wrapper pre.has-focused  .line.focused",
                "blur-[0] opacity-100"
            },

            // Error and warning states
            {
                ".code-highlight-wrapper .line.error",
                "bg-red-300/50 dark:bg-red-500/20"
            },
            {
                ".code-highlight-wrapper .line.warning",
                "bg-amber-300/50 dark:bg-amber-400/20"
            },

            // Word highlighting
            {
                ".code-highlight-wrapper .word-highlight",
                "border border-primary-600 dark:border-primary-300/25 rounded px-0.5 py-0 bg-primary-100/25 dark:bg-primary-500/10"
            },
            {
                ".code-highlight-wrapper .word-highlight-with-message",
                "border border-b border-primary-600 dark:border-primary-300/25 rounded px-1 py-1 bg-primary-100/25 dark:bg-primary-500/10 relative "
            },
            {
                ".code-highlight-wrapper .word-highlight-wrapper",
                "relative inline-block"
            },
            {
                ".code-highlight-wrapper .word-highlight-message",
                "font-sans font-semilight tracking-loose absolute top-full left-0 mt-3 px-2 py-1 text-xs text-base-800 bg-base-200/25 dark:bg-primary-700/20 dark:text-primary-200 rounded border border-primary-500/50 whitespace-nowrap z-10 select-none pointer-events-none"
            },
            {
                ".code-highlight-wrapper .word-highlight-message::selection",
                "bg-transparent"
            },
            {
                ".code-highlight-wrapper .line:has(.word-highlight-wrapper)",
                "mb-12"
            },
        });

    private static readonly ImmutableDictionary<string, string> TabApplies =
        ImmutableDictionary.CreateRange(new Dictionary<string, string>
        {
            {
                ".tab-container",
                "flex flex-col rounded-xl overflow-hidden border border-base-200 bg-base-50 dark:bg-base-900 dark:border-base-800"
            },
            {
                ".tab-list",
                "flex flex-row flex-nowrap overflow-x-auto px-2 border-b border-base-200 dark:border-base-800 bg-base-100 dark:bg-base-900 [scrollbar-width:none] [&::-webkit-scrollbar]:hidden"
            },
            {
                ".tab-button",
                "whitespace-nowrap border-b-2 border-transparent -mb-px px-3 py-2.5 text-[12px] font-mono font-medium text-base-500 dark:text-base-400 transition-colors hover:text-base-800 dark:hover:text-base-200 disabled:pointer-events-none disabled:opacity-50 data-[selected=true]:text-primary-700 data-[selected=true]:border-primary-600 dark:data-[selected=true]:text-primary-300 dark:data-[selected=true]:border-primary-300"
            },
            {
                ".tab-panel",
                "hidden data-[selected=true]:block"
            },
        });

    private static readonly ImmutableDictionary<string, string> StepApplies =
        ImmutableDictionary.CreateRange(new Dictionary<string, string>
        {
            // Container — the vertical thread line lives in a ::before pseudo at left:1.05rem
            // so the medallion (positioned at left:0 of the step body) lands centered on it.
            {
                ".steps-thread",
                "relative my-8 pl-10 before:content-[''] before:absolute before:left-4 before:top-3 before:bottom-3 before:w-0.5 before:rounded-full before:bg-base-200 dark:before:bg-base-800"
            },
            {
                ".steps-thread .step",
                "relative pb-8 last:pb-1"
            },
            {
                ".steps-thread .step-num",
                "absolute -left-10 -top-0.5 inline-flex items-center justify-center w-8 h-8 rounded-full font-display font-bold text-[13px] text-primary-700 dark:text-primary-300 bg-base-50 dark:bg-base-900 border-[1.5px] border-primary-300 dark:border-primary-300/40 ring-4 ring-base-50 dark:ring-base-950"
            },
            {
                ".steps-thread .step-title",
                "font-display font-semibold text-base text-base-900 dark:text-base-50 mb-2"
            },
        });

    private static readonly ImmutableDictionary<string, string> MarkdownAlertApplies =
        BuildMarkdownAlertApplies();

    private static ImmutableDictionary<string, string> BuildMarkdownAlertApplies()
    {
        // Per-flavor coloring: tinted background + matching border, body text in
        // base-* (the alert palette only stains the chrome, not the body copy).
        // Title color is one shade darker than the chrome tint so it reads as the
        // anchor of the box.
        const string alertFormatString =
            "bg-{0}-500/8 border-{0}-500/22 dark:bg-{0}-500/10 dark:border-{0}-500/25 fill-{0}-700 dark:fill-{0}-300";
        const string titleFormatString =
            "text-{0}-800 dark:text-{0}-200";

        // `markdown-alert-checkpoint` shares the primary-tinted chrome of
        // `important`. The chrome string can't go through the format helper
        // because `primary` is theme-keyed (--color-primary-*) and the helper
        // is for static palette colors.
        const string primaryChrome =
            "bg-primary-500/8 border-primary-500/22 dark:bg-primary-500/10 dark:border-primary-500/25 fill-primary-700 dark:fill-primary-300";
        const string primaryTitle = "text-primary-800 dark:text-primary-200";

        return ImmutableDictionary.CreateRange(new Dictionary<string, string>
        {
            // Box anatomy — rounded rectangle with tinted bg and border, body text inherits base color.
            { ".markdown-alert", "my-6 p-4 rounded-[10px] border text-[14.5px] leading-[1.6] text-base-700 dark:text-base-300" },

            // Title — visible, sentence-case, with the icon (Markdig alerts) or
            // text label (<Checkpoint>) inline as a flex child.
            { ".markdown-alert-title", "flex items-center gap-2 font-display font-semibold text-[14px]" },
            { ".markdown-alert-title svg", "w-4 h-4 shrink-0" },

            // Body typography — re-established because the alert carries
            // `not-prose` so page-prose rules (list bullets, link color,
            // paragraph margins) do not bleed inside the box. Direct-child
            // selectors keep these scoped to top-level body content; nested
            // structures fall through to defaults.
            { ".markdown-alert > p", "m-0" },
            { ".markdown-alert > ul, .markdown-alert > ol", "m-0 pl-5 list-outside" },
            { ".markdown-alert > ul", "list-disc" },
            { ".markdown-alert > ol", "list-decimal" },
            {
                ".markdown-alert > p + p, .markdown-alert > p + ul, .markdown-alert > p + ol, .markdown-alert > ul + p, .markdown-alert > ol + p",
                "mt-2"
            },
            { ".markdown-alert li", "marker:text-base-500/60 dark:marker:text-base-400/60 my-0.5" },
            { ".markdown-alert li > p", "m-0" },
            { ".markdown-alert code", "font-mono text-[0.86em] px-1 py-px rounded border border-base-200 dark:border-base-700 bg-base-100 dark:bg-base-800 text-base-700 dark:text-base-200" },
            { ".markdown-alert strong", "font-semibold text-base-900 dark:text-base-50" },
            { ".markdown-alert em", "italic" },
            { ".markdown-alert a", "underline underline-offset-[3px]" },

            // Flavor remap — design fidelity: note=sky, tip=emerald, important=primary,
            // warning=amber, caution=orange (kept distinct from warning),
            // checkpoint=primary (Mdazor component with a text label, not an SVG icon).
            { ".markdown-alert-note", string.Format(alertFormatString, "sky") },
            { ".markdown-alert-note .markdown-alert-title", string.Format(titleFormatString, "sky") },
            { ".markdown-alert-tip", string.Format(alertFormatString, "emerald") },
            { ".markdown-alert-tip .markdown-alert-title", string.Format(titleFormatString, "emerald") },
            { ".markdown-alert-important", primaryChrome },
            { ".markdown-alert-important .markdown-alert-title", primaryTitle },
            { ".markdown-alert-warning", string.Format(alertFormatString, "amber") },
            { ".markdown-alert-warning .markdown-alert-title", string.Format(titleFormatString, "amber") },
            { ".markdown-alert-caution", string.Format(alertFormatString, "orange") },
            { ".markdown-alert-caution .markdown-alert-title", string.Format(titleFormatString, "orange") },
            { ".markdown-alert-checkpoint", primaryChrome },
            { ".markdown-alert-checkpoint .markdown-alert-title", primaryTitle },
        });
    }

    private static ImmutableDictionary<string, string> HljsApplies(SyntaxTheme syntax)
    {
        string Token(ColorName c) => $"text-{c.Value}-800 dark:text-{c.Value}-300";
        string Soft(ColorName c) => $"text-{c.Value}-700 dark:text-{c.Value}-300";

        var keyword = Token(syntax.Keyword);
        var @string = Token(syntax.String);
        var variable = Token(syntax.Variable);
        var function = Token(syntax.Function);

        return ImmutableDictionary.CreateRange(new Dictionary<string, string>
        {
            { ".hljs", "text-base-900 dark:text-base-200" },

            // Comments
            { ".hljs-comment", $"text-{syntax.Comment.Value}-600 italic dark:text-{syntax.Comment.Value}-400" },
            { ".hljs-quote", $"text-{syntax.Comment.Value}-800/50 italic dark:text-{syntax.Comment.Value}-300" },

            // Keywords and control flow
            { ".hljs-keyword", keyword },
            { ".hljs-selector-tag", Soft(syntax.Keyword) },
            { ".hljs-literal", keyword },
            { ".hljs-type", "text-base-700 dark:text-base-300" },

            // Strings and characters
            { ".hljs-string", @string },
            { ".hljs-number", @string },
            { ".hljs-regexp", @string },

            // Functions and methods
            { ".hljs-function", function },
            { ".hljs-title", function },
            { ".hljs-params", function },

            // Variables and identifiers
            { ".hljs-variable", variable },
            { ".hljs-name", variable },
            { ".hljs-attr", variable },
            { ".hljs-symbol", variable },

            // Operators and punctuation
            { ".hljs-operator", "text-base-800 dark:text-base-300" },
            { ".hljs-punctuation", "text-base-800 dark:text-base-300" },

            // Special elements
            { ".hljs-built_in", Soft(syntax.Function) },
            { ".hljs-class", keyword },
            { ".hljs-meta", "text-base-800 dark:text-base-300" },
            { ".hljs-tag", keyword },
            { ".hljs-attribute", variable },
            { ".hljs-addition", "text-green-800 dark:text-green-300" },
            { ".hljs-deletion", "text-red-800 dark:text-red-300" },
            { ".hljs-link", "text-blue-800 dark:text-blue-300" },
        });
    }

    private static readonly ImmutableDictionary<string, string> SearchModalApplies =
        ImmutableDictionary.CreateRange(new Dictionary<string, string>
        {
            // Modal backdrop and container
            { ".search-modal-backdrop", "fixed inset-0 bg-base-950/50 backdrop-blur z-50 p-4 md:p-16" },
            {
                ".search-modal-content",
                " top-16 mx-auto w-full mt-8 max-w-2xl bg-base-100 dark:bg-base-900 rounded-lg shadow-xl border border-base-200 dark:border-base-700"
            },

            // Modal header and input
            { ".search-modal-header", "p-4 border-b border-base-200 dark:border-base-700" },
            { ".search-modal-input-container", "relative" },
            {
                ".search-modal-input",
                "w-full px-4 py-2 pl-10 bg-base-50 dark:bg-base-800 border border-base-300 dark:border-base-600 rounded-md text-base-900 dark:text-base-100 placeholder-base-500 dark:placeholder-base-400 focus:outline-none focus:ring-1 focus:ring-primary-500/50 focus:border-primary-500"
            },
            { ".search-modal-icon", "absolute left-3 top-2.5 h-4 w-4 text-base-400 dark:text-base-500" },

            // Results container
            { ".search-modal-results", "max-h-96 overflow-y-auto px-4 dark:scheme-dark" },

            // Status messages
            { ".search-modal-placeholder", "text-center text-base-600 dark:text-base-400 py-4" },
            { ".search-modal-loading", "text-center text-base-600 dark:text-base-400 py-4" },
            { ".search-modal-no-results", "text-center text-base-600 dark:text-base-400 py-4" },
            { ".search-modal-error", "text-center text-red-600 dark:text-red-400 py-4" },

            // Search result items
            { ".search-result-item", "border-b border-base-200 dark:border-base-800 py-4 last:border-b-0" },
            {
                ".search-result-link",
                "block hover:bg-base-50 dark:hover:bg-base-800 rounded-md p-2 -m-2 transition-colors"
            },
            { ".search-result-header", "flex items-start justify-between mb-1" },
            { ".search-result-title", "text-sm font-medium text-primary-700 dark:text-primary-400 flex-1" },
            { ".search-result-score", "text-xs text-base-500 dark:text-base-500 ml-2" },
            { ".search-result-description", "text-sm text-base-600 dark:text-base-400 mb-2" },
            { ".search-result-snippet", "text-xs text-base-700 dark:text-base-500" },
            { ".search-result-url", "text-xs text-base-500 dark:text-base-500 mt-2" },

            // Search highlighting
            { ".search-result-title .search-highlight", "text-primary-500 dark:text-primary-100 bg-inherit" },
            { ".search-highlight", "text-base-500 dark:text-base-50 bg-inherit" },
        });
}
