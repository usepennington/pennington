/**
 * SPA Engine — Generic single-page navigation for content sites.
 *
 * One render path: navigation fetches the actual destination URL, parses the
 * full HTML response with DOMParser, swaps regions marked
 * `data-spa-region="name"` from the new document into the current one, and
 * merges head deltas (title, meta, canonical, hreflang, stylesheets, JSON-LD).
 *
 * No JSON envelope. No parallel server endpoint. The static `.html` produced
 * for each page is the SPA fragment source.
 *
 * Markup contract:
 *   <main data-spa-region="content">…</main>
 *   <aside data-spa-region="sidebar">…</aside>
 *
 * Region loading modes (via data-spa-loading attribute):
 *   "skeleton" — show shimmer placeholder (or a custom <template>)
 *   "clear"    — empty the region immediately
 *   "keep"     — leave previous content until new HTML arrives (default)
 *
 * Opting out of view transitions (via data-spa-no-transition):
 *   A region marked data-spa-no-transition is not assigned a
 *   view-transition-name and is swapped before startViewTransition runs.
 *   With no name, the region's content falls under the root snapshot —
 *   and because the swap happens before the browser captures, both old
 *   and new root snapshots already show the new content, so nothing
 *   animates there. Use this for regions inside sticky/overflow ancestors
 *   where a per-region snapshot would escape the parent's clip and flash
 *   through pinned chrome.
 *
 * Stylesheet handling:
 *   - New <link rel="stylesheet"> hrefs are appended to <head> before swap.
 *   - Any stylesheet tagged `data-spa-reload` is re-fetched with a cache
 *     buster on every navigation (use for JIT stylesheets like MonorailCSS in
 *     dev where the URL doesn't change but the content does).
 *
 * Layout switching:
 *   - If the new document is missing a region the current document marks,
 *     fall back to a full page load. Explicit boundary, not a silent break.
 *
 * Lifecycle events dispatched on `document`:
 *   spa:before-navigate { url, slug }
 *   spa:commit          { url, slug, doc }
 *   spa:diagnostics     [...]   (when a diagnostics script block is present)
 */
(function () {
    'use strict';

    const _root = document.documentElement;
    const SKELETON_DELAY = parseInt(_root.dataset.spaSkeletonDelay || '100', 10);
    const MIN_SKELETON_MS = parseInt(_root.dataset.spaMinSkeleton || '250', 10);
    const REGION_SELECTOR = '[data-spa-region]';

    // Inject minimal global styles (shimmer + view-transition timing).
    const _style = document.createElement('style');
    _style.textContent = [
        '@keyframes spa-shimmer{0%{background-position:200% 0}100%{background-position:-200% 0}}',
        '::view-transition-group(*){animation-duration:150ms}',
        // Pinned elements (sticky headers, etc.) stack above region groups
        // so a named region animating from a scrolled-up position to its
        // resting position cannot visually slide through the pinned bar.
        '[data-spa-pin]{view-transition-name:var(--spa-pin-name,spa-pin)}',
        '::view-transition-group(spa-pin){z-index:9999}',
        '@media(prefers-reduced-motion:reduce){' +
            '::view-transition-group(*),::view-transition-old(*),::view-transition-new(*)' +
            '{animation:none !important}}'
    ].join('');
    document.head.appendChild(_style);

    // ARIA live region for page-change announcements.
    const _announcer = document.createElement('div');
    _announcer.setAttribute('role', 'status');
    _announcer.setAttribute('aria-live', 'polite');
    _announcer.setAttribute('aria-atomic', 'true');
    _announcer.style.cssText =
        'position:absolute;width:1px;height:1px;padding:0;margin:-1px;' +
        'overflow:hidden;clip:rect(0,0,0,0);white-space:nowrap;border:0';
    document.body.appendChild(_announcer);

    // -----------------------------------------------------------------------
    // Utilities
    // -----------------------------------------------------------------------

    const delay = (ms) => new Promise(r => setTimeout(r, ms));

    function isSpaLink(anchor) {
        if (!anchor.href) return false;
        if ((anchor.getAttribute('href') || '').startsWith('#')) return false;
        if (anchor.hasAttribute('data-spa-reload')) return false;
        try {
            const url = new URL(anchor.href);
            if (url.origin !== location.origin) return false;
            if (url.pathname === location.pathname && url.hash) return false;
            return !(anchor.target === '_blank' || anchor.hasAttribute('download'));
        } catch { return false; }
    }

    function fire(name, detail) {
        document.dispatchEvent(new CustomEvent(name, { detail }));
    }

    function maybeTransition(fn) {
        document.startViewTransition ? document.startViewTransition(fn) : fn();
    }

    // -----------------------------------------------------------------------
    // Region management
    // -----------------------------------------------------------------------

    function discoverRegions() {
        const regions = {};
        document.querySelectorAll(REGION_SELECTOR).forEach(el => {
            const name = el.dataset.spaRegion;
            const noTransition = el.hasAttribute('data-spa-no-transition');
            regions[name] = {
                el,
                loadingMode: el.dataset.spaLoading || 'keep',
                noTransition,
                skeletonTpl: document.querySelector(
                    `template[data-spa-skeleton-for="${name}"]`
                ),
            };
            // Auto-assign a view-transition-name so each region animates
            // independently. Opted-out regions stay nameless — they swap
            // before startViewTransition and are not captured per-region.
            if (!noTransition && !el.style.viewTransitionName) {
                el.style.viewTransitionName = `spa-region-${name}`;
            }
        });
        return regions;
    }

    function showLoadingState(regions) {
        for (const region of Object.values(regions)) {
            switch (region.loadingMode) {
                case 'skeleton':
                    region.el.innerHTML = '';
                    if (region.skeletonTpl) {
                        region.el.appendChild(region.skeletonTpl.content.cloneNode(true));
                    } else {
                        region.el.innerHTML = defaultSkeleton();
                    }
                    break;
                case 'clear':
                    region.el.innerHTML = '';
                    break;
                // 'keep' — leave current content in place.
            }
        }
    }

    /**
     * Returns true when the set of region names in the current document
     * exactly matches the set in the incoming document. Any divergence is
     * treated as a layout boundary and triggers a full page load.
     */
    function regionsAlign(regions, doc) {
        const currentNames = new Set(Object.keys(regions));
        const incomingNames = new Set(
            Array.from(doc.querySelectorAll(REGION_SELECTOR)).map(el => el.dataset.spaRegion)
        );
        if (currentNames.size !== incomingNames.size) return false;
        for (const name of currentNames) if (!incomingNames.has(name)) return false;
        return true;
    }

    function commitRegions(regions, doc, filter) {
        for (const [name, region] of Object.entries(regions)) {
            if (filter && !filter(region)) continue;
            const incoming = doc.querySelector(`[data-spa-region="${cssEscape(name)}"]`);
            region.el.innerHTML = incoming ? incoming.innerHTML : '';
            executeScripts(region.el);
        }
    }

    /**
     * Re-create every <script> in a swapped region so the parser actually runs
     * it. Scripts assigned via innerHTML are inert by spec; cloning into a fresh
     * element re-enters the parser. Skips JSON-LD and SPA diagnostics blocks
     * (data scripts handled elsewhere or not meant to execute).
     */
    function executeScripts(root) {
        const scripts = root.querySelectorAll('script');
        for (const old of scripts) {
            const t = (old.type || '').toLowerCase();
            if (t === 'application/ld+json' || t === 'application/spa-diagnostics+json') continue;
            const next = document.createElement('script');
            for (const { name, value } of old.attributes) next.setAttribute(name, value);
            if (!old.src) next.textContent = old.textContent;
            old.replaceWith(next);
        }
    }

    function cssEscape(value) {
        return (window.CSS && CSS.escape) ? CSS.escape(value) : value.replace(/"/g, '\\"');
    }

    function announceNavigation(title, regions) {
        _announcer.textContent = '';
        // Brief delay so the live region registers a change even for repeated titles.
        requestAnimationFrame(() => {
            _announcer.textContent = title ? `Navigated to ${title}` : 'Page updated';
        });

        // Move focus to the first content region's heading so keyboard users land in context.
        const first = regions[Object.keys(regions)[0]];
        if (!first) return;
        const target = first.el.querySelector('h1, h2, h3') || first.el;
        if (!target.hasAttribute('tabindex')) target.setAttribute('tabindex', '-1');
        target.focus({ preventScroll: true });
    }

    function defaultSkeleton() {
        const line = (w) =>
            `<div style="height:.875rem;width:${w}%;border-radius:.375rem;margin-bottom:.75rem;` +
            `background:linear-gradient(90deg,rgba(128,128,128,.1) 25%,rgba(128,128,128,.2) 50%,` +
            `rgba(128,128,128,.1) 75%);background-size:200% 100%;` +
            `animation:spa-shimmer 1.4s ease-in-out infinite"></div>`;
        return line(88) + line(72) + line(80) +
            `<div style="height:1.5rem"></div>` +
            line(92) + line(65) + line(78) + line(55);
    }

    // -----------------------------------------------------------------------
    // Head merging
    // -----------------------------------------------------------------------

    /**
     * Replace the current document head's title and managed meta/link tags
     * with the incoming document's. Tags handled: title, description meta,
     * og:* / twitter:* meta, canonical, hreflang alternates, JSON-LD scripts.
     */
    function applyHead(doc) {
        if (doc.title) document.title = doc.title;

        const swap = (sel) => {
            const incoming = doc.head.querySelector(sel);
            const current = document.head.querySelector(sel);
            if (incoming && current) {
                current.replaceWith(incoming.cloneNode(true));
            } else if (incoming) {
                document.head.appendChild(incoming.cloneNode(true));
            } else if (current) {
                current.remove();
            }
        };

        swap('meta[name="description"]');
        swap('meta[property="og:title"]');
        swap('meta[property="og:description"]');
        swap('meta[property="og:url"]');
        swap('meta[name="twitter:title"]');
        swap('meta[name="twitter:description"]');
        swap('link[rel="canonical"]');

        // Hreflang alternates: replace the whole set.
        document.head.querySelectorAll('link[rel="alternate"][hreflang]').forEach(n => n.remove());
        doc.head.querySelectorAll('link[rel="alternate"][hreflang]').forEach(n => {
            document.head.appendChild(n.cloneNode(true));
        });

        // JSON-LD: replace the whole set.
        document.head.querySelectorAll('script[type="application/ld+json"]').forEach(n => n.remove());
        doc.head.querySelectorAll('script[type="application/ld+json"]').forEach(n => {
            document.head.appendChild(n.cloneNode(true));
        });
    }

    /**
     * Bring stylesheets into sync. New hrefs are appended (and we wait for
     * load before resolving). Stylesheets tagged data-spa-reload re-fetch
     * with a cache buster on every navigation — opt-in workaround for JIT
     * stylesheets that rebuild content but keep the same URL.
     */
    function syncStylesheets(doc) {
        const currentHrefs = new Set(
            Array.from(document.head.querySelectorAll('link[rel="stylesheet"]'))
                .map(l => l.href)
        );
        const additions = [];
        doc.head.querySelectorAll('link[rel="stylesheet"]').forEach(link => {
            if (!currentHrefs.has(link.href)) additions.push(link.cloneNode(true));
        });

        const reloadable = document.head.querySelectorAll('link[rel="stylesheet"][data-spa-reload]');

        if (additions.length === 0 && reloadable.length === 0) return Promise.resolve();

        const waits = [];
        for (const link of additions) {
            waits.push(new Promise(resolve => {
                link.addEventListener('load', resolve, { once: true });
                link.addEventListener('error', resolve, { once: true });
                document.head.appendChild(link);
            }));
        }
        for (const link of reloadable) {
            const u = new URL(link.href);
            u.searchParams.set('_spa', Date.now());
            const next = link.cloneNode();
            next.href = u.toString();
            waits.push(new Promise(resolve => {
                next.addEventListener('load', () => { link.remove(); resolve(); }, { once: true });
                next.addEventListener('error', () => { link.remove(); resolve(); }, { once: true });
                link.after(next);
            }));
        }
        return Promise.all(waits);
    }

    function readDiagnostics(doc) {
        const block = doc.querySelector('script[type="application/spa-diagnostics+json"]');
        if (!block) return null;
        try { return JSON.parse(block.textContent || '[]'); }
        catch { return null; }
    }

    function scrollToTarget(url) {
        if (url.hash) {
            const t = document.querySelector(url.hash);
            if (t) { t.scrollIntoView(); return; }
        }
        window.scrollTo(0, 0);
    }

    // -----------------------------------------------------------------------
    // Prefetch cache
    // -----------------------------------------------------------------------

    const _prefetchCache = new Map();
    const PREFETCH_LIMIT = 20;
    const _parser = new DOMParser();

    function fetchDoc(href, signal) {
        return fetch(href, { signal, headers: { 'Accept': 'text/html' } })
            .then(r => {
                if (!r.ok) throw new Error(r.status);
                return r.text();
            })
            .then(html => _parser.parseFromString(html, 'text/html'));
    }

    function prefetch(href) {
        if (_prefetchCache.has(href)) return;
        // Respect the user's data-saving preference.
        if (navigator.connection && navigator.connection.saveData) return;
        if (_prefetchCache.size >= PREFETCH_LIMIT) {
            _prefetchCache.delete(_prefetchCache.keys().next().value);
        }
        _prefetchCache.set(href, fetchDoc(href).catch(() => null));
    }

    // -----------------------------------------------------------------------
    // Navigation
    // -----------------------------------------------------------------------

    let _activeNav = null;   // AbortController for the in-flight navigation
    let _currentPathname = location.pathname;

    async function navigate(url, pushState, restoreScrollY) {
        // Cancel any in-flight navigation — the latest click wins.
        if (_activeNav) _activeNav.abort();
        const ctrl = new AbortController();
        _activeNav = ctrl;

        // Snapshot scroll position into the current history entry before leaving.
        if (pushState) {
            const cur = history.state || {};
            history.replaceState({ ...cur, scrollY: window.scrollY }, '');
        }

        const regions = discoverRegions();

        // No regions found — fall back to a full page load.
        if (Object.keys(regions).length === 0) {
            location.href = url.href;
            return;
        }

        fire('spa:before-navigate', { url, slug: url.pathname });

        let fetchedDoc = null, fetchFail = false;

        const cached = _prefetchCache.get(url.href);
        _prefetchCache.delete(url.href);

        const docPromise = (
            cached
                ? cached.then(d => { if (!d) throw new Error('prefetch-miss'); return d; })
                : fetchDoc(url.href, ctrl.signal)
        ).then(d => { fetchedDoc = d; })
         .catch(e => { if (e.name !== 'AbortError') fetchFail = true; });

        // Race the fetch against a threshold: fast/cached responses skip the skeleton.
        await Promise.race([docPromise, delay(SKELETON_DELAY)]);
        if (ctrl.signal.aborted) return;

        if (fetchFail) {
            location.href = url.href;
            return;
        }

        const commit = async (doc) => {
            // Layout switch — current page has regions the new page doesn't. Full reload.
            if (!regionsAlign(regions, doc)) {
                location.href = url.href;
                return;
            }

            // Stylesheets first so newly-required CSS is parsed before any swap.
            await syncStylesheets(doc);

            // Opted-out regions swap before the transition starts. Without a
            // view-transition-name they fall under the root snapshot, and
            // because the swap completes before the browser captures, both
            // old and new root snapshots show the new content — no animation
            // runs there and the snapshot can't escape a sticky/overflow
            // ancestor to flash through pinned chrome.
            commitRegions(regions, doc, r => r.noTransition);

            // The browser preserves scrollTop on inner overflow scrollers
            // across the innerHTML swap in commitRegions, so a region's
            // view-transition bounds match in old and new snapshots without
            // any explicit reset. Sticky elements that must stay put during
            // the transition opt in via data-spa-pin.
            maybeTransition(() => {
                _currentPathname = url.pathname;
                if (pushState) history.pushState({ title: doc.title }, doc.title, url.href);
                applyHead(doc);
                commitRegions(regions, doc, r => !r.noTransition);
                announceNavigation(doc.title, regions);
                fire('spa:commit', { url, slug: url.pathname, doc });

                const diagnostics = readDiagnostics(doc);
                if (diagnostics) fire('spa:diagnostics', diagnostics);

                restoreScrollY != null ? window.scrollTo(0, restoreScrollY) : scrollToTarget(url);
            });
        };

        if (fetchedDoc) {
            // Fast path — data arrived before the threshold.
            await commit(fetchedDoc);
        } else {
            // Slow path — show skeleton while we wait.
            window.scrollTo(0, 0);
            showLoadingState(regions);
            const skeletonShownAt = performance.now();

            await docPromise;
            if (ctrl.signal.aborted) return;
            if (fetchFail) { location.href = url.href; return; }

            // Hold the skeleton long enough to feel intentional.
            const remaining = MIN_SKELETON_MS - (performance.now() - skeletonShownAt);
            if (remaining > 0) await delay(remaining);
            if (ctrl.signal.aborted) return;

            await commit(fetchedDoc);
        }
    }

    // -----------------------------------------------------------------------
    // Event listeners
    // -----------------------------------------------------------------------

    document.addEventListener('click', (e) => {
        if (e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
        const anchor = e.target.closest('a');
        if (!anchor || !isSpaLink(anchor)) return;
        e.preventDefault();
        navigate(new URL(anchor.href), true);
    });

    // Prefetch on hover and keyboard focus for near-instant navigation.
    // Hover requires a brief dwell so links the cursor merely grazes past
    // (e.g. while traveling toward another target) don't trigger fetches.
    // Focus is high-intent — fire immediately.
    const HOVER_PREFETCH_MS = 65;
    let _hoverTimer = 0;
    let _hoverAnchor = null;

    function hoverPrefetch(e) {
        const anchor = e.target.closest('a');
        if (anchor === _hoverAnchor) return;
        cancelHoverPrefetch();
        if (!anchor || !isSpaLink(anchor)) return;
        _hoverAnchor = anchor;
        _hoverTimer = setTimeout(() => {
            prefetch(new URL(anchor.href).href);
            _hoverAnchor = null;
        }, HOVER_PREFETCH_MS);
    }
    function cancelHoverPrefetch() {
        if (_hoverTimer) { clearTimeout(_hoverTimer); _hoverTimer = 0; }
        _hoverAnchor = null;
    }
    function focusPrefetch(e) {
        const anchor = e.target.closest('a');
        if (!anchor || !isSpaLink(anchor)) return;
        prefetch(new URL(anchor.href).href);
    }
    document.addEventListener('pointerover', hoverPrefetch);
    document.addEventListener('pointerout', cancelHoverPrefetch);
    document.addEventListener('focusin', focusPrefetch);

    window.addEventListener('popstate', (e) => {
        const url = new URL(location.href);
        // Hash-only history entries share the same pathname — let the browser handle scrolling.
        if (url.pathname === _currentPathname) return;
        navigate(url, false, e.state?.scrollY);
    });

})();
