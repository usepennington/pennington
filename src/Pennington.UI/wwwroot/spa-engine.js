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
 * Navigation is instantaneous: old content stays on screen until the new
 * document is fetched and its stylesheets are loaded, then the swap, scroll
 * reset, and head update all happen in a single synchronous block so the
 * browser paints them as one frame. No animation, no skeleton, no flash.
 *
 * Scroll preservation (via data-spa-region-key):
 *   The browser keeps scrollTop on inner overflow scrollers across an
 *   innerHTML swap, which is what you want when navigating between pages
 *   that share a region's content (e.g. clicking another doc page in the
 *   same section). When the content changes meaningfully — switching doc
 *   sections, tabs, etc. — set data-spa-region-key on the region element
 *   to a value that identifies the content set. If the incoming element's
 *   key differs from the current one, the region and its closest
 *   scrollable ancestor have scrollTop reset to 0 after the swap.
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
    const PROGRESS_DELAY = parseInt(_root.dataset.spaProgressDelay || '100', 10);
    const REGION_SELECTOR = '[data-spa-region]';

    // Per-process server fingerprint emitted in dev mode. Lets a still-open
    // tab detect that port 5000 now hosts a different example and refuse to
    // commit cached prefetches against it.
    const _hostMeta = document.querySelector('meta[name="x-pennington-host"]');
    const _hostFingerprint = _hostMeta ? _hostMeta.getAttribute('content') : null;

    // Progress bar styles. No view-transition or shimmer rules — the engine
    // swaps synchronously and never shows placeholders.
    const _style = document.createElement('style');
    _style.textContent = [
        '.spa-progress{position:fixed;top:0;left:0;right:0;height:2px;z-index:9999;' +
            'pointer-events:none;opacity:0;transition:opacity 200ms ease-out}',
        '.spa-progress.is-active{opacity:1}',
        '.spa-progress-bar{height:100%;width:100%;transform:scaleX(0);' +
            'transform-origin:left center;background:var(--color-primary-500,#3b82f6);' +
            'transition:transform 200ms ease-out;will-change:transform}',
        '@media(prefers-reduced-motion:reduce){' +
            '.spa-progress,.spa-progress-bar{transition:none}}'
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

    // Top-of-viewport progress bar for slow navigations. The PROGRESS_DELAY
    // gate keeps fast/cached navigations silent. The trickle is fake — the
    // engine has no body-progress signal, just an atomic fetch — but a
    // diminishing-return curve plus a 100% snap on commit reads honestly at
    // varying durations (e.g. 100ms vs a 5–10s Roslyn cold-start).
    const progressBar = {
        _outer: null,
        _bar: null,
        _value: 0,
        _showTimer: 0,
        _trickleTimer: 0,
        _fadeTimer: 0,
        _ensure() {
            if (this._outer) return;
            const outer = document.createElement('div');
            outer.className = 'spa-progress';
            const bar = document.createElement('div');
            bar.className = 'spa-progress-bar';
            outer.appendChild(bar);
            document.body.appendChild(outer);
            this._outer = outer;
            this._bar = bar;
        },
        _set(v) {
            this._value = v;
            if (this._bar) this._bar.style.transform = 'scaleX(' + v + ')';
        },
        _resetInstant() {
            if (!this._bar) return;
            this._bar.style.transition = 'none';
            this._set(0);
            // Force layout so the next transform animates from 0 instead of
            // collapsing into a single transition.
            void this._bar.offsetWidth;
            this._bar.style.transition = '';
        },
        _trickle() {
            const v = this._value;
            let inc;
            if (v < 0.2) inc = 0.10;
            else if (v < 0.5) inc = 0.04;
            else if (v < 0.8) inc = 0.02;
            else if (v < 0.99) inc = 0.005;
            else return;
            this._set(Math.min(v + inc, 0.994));
        },
        start() {
            // Re-entrant before the fade completes: cancel it and wipe back to
            // 0 so trickle restarts cleanly. complete()'s scaleX(1) snap would
            // otherwise leave the bar stuck at full.
            if (this._fadeTimer) {
                clearTimeout(this._fadeTimer);
                this._fadeTimer = 0;
                if (this._outer) this._outer.classList.remove('is-active');
                this._resetInstant();
            }
            if (this._showTimer) return;
            if (this._outer && this._outer.classList.contains('is-active')) return;
            this._showTimer = setTimeout(() => {
                this._showTimer = 0;
                this._ensure();
                this._set(0.08);
                this._outer.classList.add('is-active');
                this._trickleTimer = setInterval(() => this._trickle(), 400);
            }, PROGRESS_DELAY);
        },
        complete() {
            if (this._showTimer) {
                clearTimeout(this._showTimer);
                this._showTimer = 0;
                return;
            }
            if (!this._outer) return;
            if (this._trickleTimer) {
                clearInterval(this._trickleTimer);
                this._trickleTimer = 0;
            }
            this._set(1);
            // Hold at 100% briefly so the fill is visible, then fade opacity,
            // then snap transform back to 0 off-screen for the next start.
            this._fadeTimer = setTimeout(() => {
                this._outer.classList.remove('is-active');
                this._fadeTimer = setTimeout(() => {
                    this._fadeTimer = 0;
                    this._resetInstant();
                }, 220);
            }, 220);
        }
    };

    // -----------------------------------------------------------------------
    // Utilities
    // -----------------------------------------------------------------------

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

    // -----------------------------------------------------------------------
    // Region management
    // -----------------------------------------------------------------------

    function discoverRegions() {
        const regions = {};
        document.querySelectorAll(REGION_SELECTOR).forEach(el => {
            regions[el.dataset.spaRegion] = { el };
        });
        return regions;
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

    function commitRegions(regions, doc) {
        for (const [name, region] of Object.entries(regions)) {
            const incoming = doc.querySelector(`[data-spa-region="${cssEscape(name)}"]`);
            const oldKey = region.el.dataset.spaRegionKey;
            const newKey = incoming ? incoming.dataset.spaRegionKey : undefined;

            region.el.innerHTML = incoming ? incoming.innerHTML : '';
            if (incoming) {
                if (newKey === undefined) delete region.el.dataset.spaRegionKey;
                else region.el.dataset.spaRegionKey = newKey;
            }
            executeScripts(region.el);

            // When the content set changes (e.g. switching doc sections),
            // reset scroll on the region and its closest scrollable ancestor.
            // The browser preserves scrollTop across innerHTML swaps, which is
            // the right default for same-section navigation but leaves the
            // user mid-list in unrelated content otherwise.
            if (oldKey !== newKey) {
                if (region.el.scrollTop) region.el.scrollTop = 0;
                const scroller = findScrollableAncestor(region.el);
                if (scroller) scroller.scrollTop = 0;
            }
        }
    }

    function findScrollableAncestor(el) {
        let p = el.parentElement;
        while (p && p !== document.body) {
            const o = getComputedStyle(p).overflowY;
            if (o === 'auto' || o === 'scroll') return p;
            p = p.parentElement;
        }
        return null;
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

    function scrollToHash(url) {
        if (!url.hash) return;
        const t = document.querySelector(url.hash);
        if (t) t.scrollIntoView({ behavior: 'instant' });
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
        // Abort/fail paths intentionally don't tear this down: a re-entrant
        // navigate() calls start() again (idempotent), and fetchFail bails to
        // location.href which discards the DOM.
        progressBar.start();

        const cached = _prefetchCache.get(url.href);
        _prefetchCache.delete(url.href);

        let doc;
        try {
            doc = cached
                ? await cached.then(d => { if (!d) throw new Error('prefetch-miss'); return d; })
                : await fetchDoc(url.href, ctrl.signal);
        } catch (e) {
            if (e && e.name === 'AbortError') return;
            location.href = url.href;
            return;
        }
        if (ctrl.signal.aborted) return;

        // Host fingerprint mismatch — the port is now serving a different
        // process (e.g. a new example replaced the previous one). Drop the
        // prefetch cache and force a full reload so the new app's chrome wins.
        if (_hostFingerprint) {
            const incomingMeta = doc.querySelector('meta[name="x-pennington-host"]');
            const incoming = incomingMeta ? incomingMeta.getAttribute('content') : null;
            if (incoming && incoming !== _hostFingerprint) {
                _prefetchCache.clear();
                location.href = url.href;
                return;
            }
        }

        // Layout switch — current page has regions the new page doesn't. Full reload.
        if (!regionsAlign(regions, doc)) {
            location.href = url.href;
            return;
        }

        // Preload new stylesheets before the swap so the first paint of the
        // new DOM is already styled — no FOUC.
        await syncStylesheets(doc);
        if (ctrl.signal.aborted) return;

        // Synchronous swap block: DOM replacement, scroll reset, head update,
        // and event dispatch all happen before the browser gets a chance to
        // paint, so the user sees a single atomic frame change.
        _currentPathname = url.pathname;
        if (pushState) history.pushState({ title: doc.title }, doc.title, url.href);
        applyHead(doc);
        commitRegions(regions, doc);
        window.scrollTo({ top: restoreScrollY != null ? restoreScrollY : 0, left: 0, behavior: 'instant' });
        if (url.hash) scrollToHash(url);
        announceNavigation(doc.title, regions);
        fire('spa:commit', { url, slug: url.pathname, doc });
        progressBar.complete();

        const diagnostics = readDiagnostics(doc);
        if (diagnostics) fire('spa:diagnostics', diagnostics);
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
