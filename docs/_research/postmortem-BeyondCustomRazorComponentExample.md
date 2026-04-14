# Post-mortem — BeyondCustomRazorComponentExample

## Shape

Single-csproj DocSite host (`Microsoft.NET.Sdk.Web`) with a `.razor` file
authored in the example's own assembly. One top-level `_Imports.razor`
brings in `Microsoft.AspNetCore.Components`,
`Microsoft.AspNetCore.Components.Web`, and the example's `Components`
namespace so `PricingCard.razor` doesn't need per-file `@using` directives.
DocSite already calls `services.AddMdazor()` plus the eight built-in
Pennington.UI component registrations — the only new host line is
`services.AddMdazorComponent<PricingCard>()` in `Program.cs`.

## API reality

- Extension: `Mdazor.ServiceCollectionExtensions.AddMdazorComponent<T>(
  this IServiceCollection services) where T : IComponent` — generic,
  single `IServiceCollection` parameter, returns `IServiceCollection` so
  it chains. Lives in the `Mdazor` namespace, shipped in the `Mdazor`
  NuGet package (already referenced transitively through
  `Pennington.DocSite`).
- Markdown pipeline auto-wires `UseMdazor(serviceProvider)` when
  `IComponentRegistry` is present in DI — no explicit pipeline config.

## SDK + csproj

- **`Microsoft.NET.Sdk.Web` is enough.** The Web SDK imports the Razor
  SDK transitively, so `.razor` files compile without switching to
  `Microsoft.NET.Sdk.Razor`. This was the open question going in —
  verified by a clean build with only the Web SDK.
- **Only the `Pennington.DocSite` ProjectReference is needed.** Mdazor,
  Pennington.UI, MonorailCSS, etc. all flow transitively.

## Tag + parameter binding (verified)

- **Component name must start capital** (HTML custom-element rules).
  Mdazor matches the tag's type-name (case-sensitive on the name itself)
  against registered types.
- **Attribute → parameter binding is case-insensitive via reflection**
  (Mdazor README + `MdazorIntegrationTests`). `Tier="Pro"` binds to
  `[Parameter] public string Tier`.
- **Only primitive parameter types bind from markdown attributes** —
  `string`, numbers, `bool`. The example ships the feature list as a
  pipe-delimited string (`Features="A|B|C"`) and splits it inside the
  component.
- **Both self-closing and open/close forms work** — `<PricingCard ... />`
  and `<PricingCard ...></PricingCard>`. Only the open/close form can
  carry `ChildContent` (markdown between the tags is recursively
  processed).

## Verification

`dotnet build Pennington.slnx` clean (0 errors, pre-existing xUnit1051
warnings only). Dev server on `localhost:5732`: Playwright confirmed
`/pricing` renders **both** PricingCard instances — Basic at `$9` with
three bullet features and the default (non-highlighted) border, Pro at
`$49` with four features, the `border-primary-500` ring, and a "MOST
POPULAR" pill above the tier. Static `dotnet run -- build output`
produced 8 pages; `output/pricing/index.html` contains `$9`, `$49`,
`Community support`, `Unlimited projects`, `Most Popular`, and
`border-primary-500` — both cards baked in. Output cleaned.

No blockers.
