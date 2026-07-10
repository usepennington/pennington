using Pennington.Head;

namespace Pennington.Beck;

/// <summary>
/// Ships the client half of the diagram fullscreen zoom: one inline <c>&lt;style&gt;</c> +
/// <c>&lt;script&gt;</c> block contributed to every page head while <see cref="BeckOptions.Zoom"/>
/// is on. The script is a single delegated click listener on <c>document</c> (so it survives
/// Blazor re-renders and covers every embed on the page) that opens a native
/// <c>&lt;dialog class="beck-lightbox"&gt;</c> holding a clone of the clicked diagram's SVG on an
/// opaque theme-surface panel —
/// cloning is safe because the SVG's animation and theming are hash-scoped CSS classes that apply
/// to the copy too, and its <c>url(#id)</c> refs still resolve to the original's defs, which stay
/// in the document. The dialog is built per-open and removed on close.
/// </summary>
internal sealed class BeckZoomHeadContributor : IHeadContributor
{
    private readonly BeckOptions _options;

    public BeckZoomHeadContributor(BeckOptions options) => _options = options;

    /// <summary>Site-level asset — no per-page variance, nothing to win ties against.</summary>
    public int Order => HeadOrder.Site;

    public bool ShouldContribute(HeadContext context) => _options.Zoom;

    public Task ContributeAsync(HeadContext context, HeadBuilder head)
    {
        head.Add(new HeadTagKey("beck:zoom"), new HeadTag(new RawTag(Assets)));
        return Task.CompletedTask;
    }

    /// <summary>
    /// The zoom button/lightbox CSS and the dialog-opening script, emitted verbatim. Colors ride
    /// the host's <c>--color-base-*</c> ramp with slate fallbacks, and dark mode keys off
    /// <c>[data-theme="dark"]</c> — the same signal the Beck SVG itself uses. The backdrop washes
    /// the page out (blur + desaturate) behind a scrim mixed from the ramp's ends rather than the
    /// SVG's own surface token, which is scoped to the per-diagram class inside each SVG and
    /// doesn't resolve on a <c>&lt;dialog&gt;</c> in <c>&lt;body&gt;</c>. <c>@starting-style</c>
    /// gives the <c>::backdrop</c> — newly rendered on every open — a from-state to transition
    /// out of; the dialog is removed on close, so the cue is entrance-only.
    /// </summary>
    internal const string Assets = """
        <style data-head="beck:zoom">
        .beck-embed{position:relative}
        .beck-zoom{position:absolute;right:10px;bottom:10px;display:flex;align-items:center;justify-content:center;width:30px;height:30px;padding:0;cursor:pointer;border:1px solid var(--color-base-300,#cbd5e1);border-radius:8px;background:color-mix(in srgb,var(--color-base-50,#f8fafc) 88%,transparent);color:var(--color-base-500,#64748b);opacity:0;transition:opacity .15s,color .15s,border-color .15s}
        .beck-embed:hover .beck-zoom,.beck-zoom:focus-visible{opacity:1}
        .beck-zoom:hover{color:var(--color-base-900,#0f172a);border-color:var(--color-base-400,#94a3b8)}
        [data-theme="dark"] .beck-zoom{border-color:var(--color-base-700,#334155);background:color-mix(in srgb,var(--color-base-900,#0f172a) 88%,transparent);color:var(--color-base-400,#94a3b8)}
        [data-theme="dark"] .beck-zoom:hover{color:var(--color-base-50,#f8fafc);border-color:var(--color-base-600,#475569)}
        @media (hover:none){.beck-zoom{opacity:1}}
        .beck-lightbox{border:0;padding:0;margin:auto;background:transparent;width:100vw;height:100vh;max-width:100vw;max-height:100vh;overflow:hidden;cursor:zoom-out}
        .beck-lightbox[open]{display:flex;align-items:center;justify-content:center}
        .beck-lightbox::backdrop{background-color:color-mix(in srgb,var(--color-base-50,#ffffff) 55%,transparent);backdrop-filter:blur(12px) saturate(0.1);-webkit-backdrop-filter:blur(12px) saturate(0.1);transition:background-color .18s ease-out,backdrop-filter .18s ease-out,-webkit-backdrop-filter .18s ease-out}
        @starting-style{.beck-lightbox[open]::backdrop{background-color:transparent;backdrop-filter:blur(0) saturate(1);-webkit-backdrop-filter:blur(0) saturate(1)}}
        [data-theme="dark"] .beck-lightbox::backdrop{background-color:color-mix(in srgb,var(--color-base-950,#0d1117) 60%,transparent)}
        .beck-lightbox-panel{display:flex;padding:24px;border-radius:16px;background:var(--color-base-50,#f8fafc);border:1px solid var(--color-base-200,#e2e8f0);box-shadow:0 25px 80px -12px rgb(0 0 0/.35)}
        [data-theme="dark"] .beck-lightbox-panel{background:var(--color-base-950,#0d1117);border-color:var(--color-base-800,#1e293b)}
        .beck-lightbox .beck-svg{width:auto;height:auto;max-width:calc(94vw - 48px);max-height:calc(90vh - 48px)}
        .beck-lightbox-close{position:fixed;top:18px;right:18px;display:flex;align-items:center;justify-content:center;width:38px;height:38px;padding:0;cursor:pointer;border:1px solid var(--color-base-300,#cbd5e1);border-radius:9999px;background:color-mix(in srgb,var(--color-base-50,#f8fafc) 88%,transparent);color:var(--color-base-500,#64748b);transition:color .15s,border-color .15s}
        .beck-lightbox-close:hover{color:var(--color-base-900,#0f172a);border-color:var(--color-base-400,#94a3b8)}
        [data-theme="dark"] .beck-lightbox-close{border-color:var(--color-base-700,#334155);background:color-mix(in srgb,var(--color-base-900,#0f172a) 88%,transparent);color:var(--color-base-400,#94a3b8)}
        [data-theme="dark"] .beck-lightbox-close:hover{color:var(--color-base-50,#f8fafc);border-color:var(--color-base-600,#475569)}
        </style>
        <script data-head="beck:zoom">
        (function () {
          if (window.__beckZoom) return; window.__beckZoom = true;
          function openLightbox(embed) {
            var svg = embed.querySelector('.beck-svg');
            if (!svg || document.querySelector('.beck-lightbox')) return;
            var dialog = document.createElement('dialog');
            dialog.className = 'beck-lightbox';
            dialog.setAttribute('aria-label', 'Diagram, full screen');
            var clone = svg.cloneNode(true);
            // Drop the engine's inline sizing (max-width cap + height:auto) so the lightbox
            // CSS controls the box: natural size, shrunk only when it exceeds the viewport.
            clone.removeAttribute('style');
            // The panel gives the diagram an opaque theme surface so it reads solid against
            // the washed-out backdrop; the surface lives here, not on the <svg>, because
            // padding on an auto-sized SVG box skews its intrinsic-ratio sizing.
            var panel = document.createElement('div');
            panel.className = 'beck-lightbox-panel';
            panel.appendChild(clone);
            dialog.appendChild(panel);
            var close = document.createElement('button');
            close.type = 'button';
            close.className = 'beck-lightbox-close';
            close.setAttribute('aria-label', 'Close full screen view');
            close.innerHTML = '<svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" aria-hidden="true"><path d="M18 6 6 18M6 6l12 12"/></svg>';
            dialog.appendChild(close);
            // Any click closes: the backdrop, the diagram, or the X; Esc is native <dialog>.
            dialog.addEventListener('click', function () { dialog.close(); });
            dialog.addEventListener('close', function () { dialog.remove(); });
            document.body.appendChild(dialog);
            dialog.showModal();
          }
          document.addEventListener('click', function (e) {
            var zoom = e.target.closest('.beck-zoom');
            if (!zoom) return;
            var embed = zoom.closest('.beck-embed');
            if (embed) openLightbox(embed);
          });
        })();
        </script>
        """;
}
