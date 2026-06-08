# BeyondCustomRazorComponentExample

Authoring a Razor component and rendering it inline from markdown via Mdazor. Same DocSite host shape as the tutorials — the only new line is `AddMdazorComponent<PricingCard>()`.

## Concepts

- Authoring a Razor component (`Components/PricingCard.razor`)
- `AddMdazorComponent<T>()` registering the tag with the Mdazor registry
- `<PricingCard ... />` tags inside markdown rendering as real Blazor components with parameters bound from attributes
- Reading page context (file name, URL, front matter) from `MdazorContext` via a `[CascadingParameter]` — see `Components/PageFacts.razor` and `Content/page-context.md`

## Tutorial stages

`Stage1_ComponentAuthored.cs` → `Stage2_RegisterMdazorComponent.cs`.

## Referenced from

- `docs/.../tutorials/beyond-basics/custom-razor-component.md`
