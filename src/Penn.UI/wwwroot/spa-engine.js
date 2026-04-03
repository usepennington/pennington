/**
 * SPA Engine — Generic single-page navigation for content sites.
 *
 * Declarative island system: mark DOM regions with data-spa-island="name",
 * and the engine updates them from a JSON envelope on navigation.
 *
 * JSON envelope format:
 * {
 *   "title":       "Page Title",
 *   "description": "Page description",
 *   "islands": {
 *     "content": "<html>…</html>",
 *     "sidebar": "<html>…</html>"
 *   }
 * }
 *
 * First page load: full static HTML (normal browser behaviour).
 * Subsequent in-site navigation: intercept link clicks, fetch the
 * pre-generated JSON, and swap island contents without a full reload.
 *
 * Island loading modes (via data-spa-loading attribute):
 *   "skeleton" — show shimmer placeholder (or a custom <template>)
 *   "clear"    — empty the island immediately
 *   "keep"     — leave previous content until new data arrives (default)
 *
 * Lifecycle events dispatched on `document`:
 *   spa:before-navigate  { url, slug }
 *   spa:commit           { url, slug, data }
 */
(function () {
    'use strict';

    // -----------------------------------------------------------------------
    // Configuration (from data attributes or sensible defaults)
    // -----------------------------------------------------------------------

    const _root = document.documentElement;
    const BASE_PATH = (document.body.dataset.baseUrl || '').replace(/\/$/, '');
    const DATA_PATH = _root.dataset.spaDataPath || '/_spa-data';
    const SKELETON_DELAY = parseInt(_root.dataset.spaSkeletonDelay || '100', 10);
    const MIN_SKELETON_MS = parseInt(_root.dataset.spaMinSkeleton || '250', 10);

    // Derive site title once: "SiteTitle - PageTitle" → "SiteTitle".
    const _dash = document.title.indexOf(' - ');
    const SITE_TITLE = _dash > -1 ? document.title.substring(0, _dash) : document.title;

    // -----------------------------------------------------------------------
    // Inject minimal global styles
    // -----------------------------------------------------------------------

    const _style = document.createElement('style');
    _style.textContent = [
        '@keyframes spa-shimmer{0%{background-position:200% 0}100%{background-position:-200% 0}}',
        '::view-transition-group(*){animation-duration:150ms}',
        '@media(prefers-reduced-motion:reduce){' +
            '::view-transition-group(*),::view-transition-old(*),::view-transition-new(*)' +
            '{animation:none !important}}'
    ].join('');
    document.head.appendChild(_style);

    // -----------------------------------------------------------------------
    // Accessibility — ARIA live region for page-change announcements
    // -----------------------------------------------------------------------

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

    function getSlug(url) {
        let p = url.pathname;
        if (BASE_PATH && p.startsWith(BASE_PATH)) p = p.substring(BASE_PATH.length) || '/';
        return (!p || p === '/') ? 'index' : p.replace(/^\//, '').replace(/\/$/, '');
    }

    function isSpaLink(anchor) {
        if (!anchor.href) return false;
        if ((anchor.getAttribute('href') || '').startsWith('#')) return false;
        if (anchor.hasAttribute('data-spa-reload')) return false;
        try {
            const url = new URL(anchor.href);
            if (url.origin !== location.origin) return false;
            if (url.pathname.includes(DATA_PATH)) return false;
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

    function reloadDevStylesheet() {
        if (location.hostname !== 'localhost' && location.hostname !== '127.0.0.1') return;
        const link = document.querySelector('link[rel="stylesheet"]');
        if (!link) return;
        const u = new URL(link.href);
        u.searchParams.set('_t', Date.now());
        const next = link.cloneNode();
        next.href = u.toString();
        next.onload = () => link.remove();
        link.after(next);
    }

    // -----------------------------------------------------------------------
    // Island management
    // -----------------------------------------------------------------------

    function discoverIslands() {
        const islands = {};
        document.querySelectorAll('[data-spa-island]').forEach(el => {
            const name = el.dataset.spaIsland;
            islands[name] = {
                el,
                loadingMode: el.dataset.spaLoading || 'keep',
                skeletonTpl: document.querySelector(
                    `template[data-spa-skeleton-for="${name}"]`
                ),
            };
            // Auto-assign a view-transition-name so each island animates independently.
            if (!el.style.viewTransitionName) {
                el.style.viewTransitionName = `spa-island-${name}`;
            }
        });
        return islands;
    }

    function showLoadingState(islands) {
        for (const [, island] of Object.entries(islands)) {
            switch (island.loadingMode) {
                case 'skeleton':
                    if (island.skeletonTpl) {
                        island.el.innerHTML = '';
                        island.el.appendChild(island.skeletonTpl.content.cloneNode(true));
                    } else {
                        island.el.innerHTML = defaultSkeleton();
                    }
                    break;
                case 'clear':
                    island.el.innerHTML = '';
                    break;
                // 'keep' — leave current content in place.
            }
        }
    }

    function commitIslands(islands, data) {
        const provided = data.islands || {};
        for (const [name, island] of Object.entries(islands)) {
            island.el.innerHTML = provided[name] ?? '';
        }
    }

    function announceNavigation(title, islands) {
        // Screen reader announcement.
        _announcer.textContent = '';
        // Brief delay so the live region registers a change even for repeated titles.
        requestAnimationFrame(() => {
            _announcer.textContent = title ? `Navigated to ${title}` : 'Page updated';
        });

        // Move focus to the first content island so keyboard users land in context.
        // Find a heading inside, or fall back to the island element itself.
        const first = islands[Object.keys(islands)[0]];
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
    // Meta & scroll
    // -----------------------------------------------------------------------

    function applyMeta(data, url) {
        document.title = data.title
            ? `${SITE_TITLE} - ${data.title}`
            : SITE_TITLE;

        const setMeta = (sel, val) => {
            const el = document.querySelector(sel);
            if (!el) return;
            val ? el.setAttribute('content', val) : el.removeAttribute('content');
        };

        setMeta('meta[name="description"]', data.description);
        setMeta('meta[property="og:description"]', data.description);
        setMeta('meta[property="og:title"]', data.title);
        setMeta('meta[property="og:url"]', url.href);
        setMeta('meta[name="twitter:title"]', data.title);
        setMeta('meta[name="twitter:description"]', data.description);

        const canon = document.querySelector('link[rel="canonical"]');
        if (canon) canon.href = url.href;
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

    function prefetch(slug) {
        if (_prefetchCache.has(slug)) return;
        if (_prefetchCache.size >= PREFETCH_LIMIT) {
            _prefetchCache.delete(_prefetchCache.keys().next().value);
        }
        const promise = fetch(`${BASE_PATH}${DATA_PATH}/${slug}.json`)
            .then(r => { if (!r.ok) throw new Error(r.status); return r.json(); })
            .catch(() => null);
        _prefetchCache.set(slug, promise);
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

        const slug = getSlug(url);
        const islands = discoverIslands();

        // No islands found — fall back to a full page load.
        if (Object.keys(islands).length === 0) {
            location.href = url.href;
            return;
        }

        fire('spa:before-navigate', { url, slug });

        let fetchData = null, fetchFail = false;

        // Use prefetched data if available, otherwise fetch fresh.
        const cached = _prefetchCache.get(slug);
        _prefetchCache.delete(slug);

        const dataPromise = (
            cached
                ? cached.then(d => { if (!d) throw new Error('prefetch-miss'); return d; })
                : fetch(`${BASE_PATH}${DATA_PATH}/${slug}.json`, { signal: ctrl.signal })
                    .then(r => { if (!r.ok) throw new Error(r.status); return r.json(); })
        ).then(d => { fetchData = d; })
         .catch(e => { if (e.name !== 'AbortError') fetchFail = true; });

        // Race the fetch against a threshold: fast/cached responses skip the skeleton.
        await Promise.race([dataPromise, delay(SKELETON_DELAY)]);
        if (ctrl.signal.aborted) return;

        if (fetchFail) {
            location.href = url.href;
            return;
        }

        const doCommit = (data) => {
            _currentPathname = url.pathname;
            if (pushState) history.pushState({ title: data.title }, data.title, url.href);
            applyMeta(data, url);
            commitIslands(islands, data);
            announceNavigation(data.title, islands);
            fire('spa:commit', { url, slug, data });
            restoreScrollY != null ? window.scrollTo(0, restoreScrollY) : scrollToTarget(url);
            reloadDevStylesheet();
        };

        if (fetchData) {
            // Fast path — data arrived before the threshold.
            maybeTransition(() => doCommit(fetchData));
        } else {
            // Slow path — show skeleton while we wait.
            window.scrollTo(0, 0);
            showLoadingState(islands);
            const skeletonShownAt = performance.now();

            await dataPromise;
            if (ctrl.signal.aborted) return;

            if (fetchFail) {
                location.href = url.href;
                return;
            }

            // Hold the skeleton long enough to feel intentional.
            const remaining = MIN_SKELETON_MS - (performance.now() - skeletonShownAt);
            if (remaining > 0) await delay(remaining);
            if (ctrl.signal.aborted) return;

            maybeTransition(() => doCommit(fetchData));
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
    function maybePrefetch(e) {
        const anchor = e.target.closest('a');
        if (!anchor || !isSpaLink(anchor)) return;
        prefetch(getSlug(new URL(anchor.href)));
    }
    document.addEventListener('pointerover', maybePrefetch);
    document.addEventListener('focusin', maybePrefetch);

    window.addEventListener('popstate', (e) => {
        const url = new URL(location.href);
        // Hash-only history entries share the same pathname — let the browser handle scrolling.
        if (url.pathname === _currentPathname) return;
        navigate(url, false, e.state?.scrollY);
    });

})();
