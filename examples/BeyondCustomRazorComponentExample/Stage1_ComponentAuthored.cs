namespace BeyondCustomRazorComponentExample;

/// <summary>
/// Stage 1 — the component itself has been authored at
/// <c>Components/PricingCard.razor</c> but Mdazor is not yet aware of it.
/// At this point a markdown file consuming <c>&lt;PricingCard /&gt;</c> would
/// render the tag as a literal custom element (lowercased, with an HTML
/// comment carrying the error) rather than as the styled component.
///
/// Tutorial prose extracts <see cref="Source"/> via <c>xmldocid,bodyonly</c>
/// to show the reader the exact shape of a minimal Mdazor component: a
/// single <c>.razor</c> file with <c>[Parameter]</c>-decorated properties
/// and markup that consumes them. This class is never instantiated.
/// </summary>
public static class Stage1
{
    /// <summary>
    /// Returns the canonical source of <c>PricingCard.razor</c> as a string.
    /// Kept as a method body (not a file-reference) so the tutorial page can
    /// pull it with <c>csharp:xmldocid,bodyonly</c> without needing a
    /// separate <c>razor:path</c> fence.
    /// </summary>
    public static string Source() => """
        <div class="not-prose my-6">
            <div class="@CardClasses">
                <h3 class="text-xl font-bold">@Tier</h3>
                <div class="mt-2 flex items-baseline gap-1">
                    <span class="text-4xl font-extrabold">$@Price</span>
                    <span class="text-sm">/ month</span>
                </div>
                <ul class="mt-4 space-y-2 text-sm">
                    @foreach (var feature in ParsedFeatures)
                    {
                        <li>@feature</li>
                    }
                </ul>
            </div>
        </div>

        @code {
            [Parameter] public string Tier { get; set; } = "Basic";
            [Parameter] public string Price { get; set; } = "0";
            [Parameter] public string Features { get; set; } = "";
            [Parameter] public bool Highlighted { get; set; }

            private IEnumerable<string> ParsedFeatures =>
                (Features ?? "").Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            private string CardClasses => Highlighted
                ? "rounded-xl border-2 border-primary-500 p-6"
                : "rounded-xl border border-base-200 p-6";
        }
        """;
}
