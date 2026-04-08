/**
 * Search integration for 937 Yoga
 * Uses Fuse.js for client-side fuzzy search against Penn's /search-index.json
 */
(function () {
    'use strict';

    let fuseInstance = null;
    let searchData = null;
    let isLoading = false;
    let selectedIndex = -1;

    const modal = document.getElementById('search-modal');
    const input = document.getElementById('search-input');
    const results = document.getElementById('search-results');

    if (!modal || !input || !results) return;

    // Expose global open function for the nav button
    window.openSearch = openModal;

    // Keyboard shortcut: Ctrl+K or Cmd+K
    document.addEventListener('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            openModal();
        }
        if (e.key === 'Escape' && modal.style.display !== 'none') {
            closeModal();
        }
    });

    // Close on backdrop click
    modal.addEventListener('click', function (e) {
        if (e.target === modal) closeModal();
    });

    // Search on input
    input.addEventListener('input', debounce(function () {
        performSearch(input.value.trim());
    }, 200));

    // Keyboard navigation in results
    input.addEventListener('keydown', function (e) {
        const items = results.querySelectorAll('.search-result-item');
        if (!items.length) return;

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            selectedIndex = Math.min(selectedIndex + 1, items.length - 1);
            updateSelection(items);
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            selectedIndex = Math.max(selectedIndex - 1, 0);
            updateSelection(items);
        } else if (e.key === 'Enter' && selectedIndex >= 0) {
            e.preventDefault();
            const link = items[selectedIndex].querySelector('a');
            if (link) window.location.href = link.href;
        }
    });

    function openModal() {
        modal.style.display = '';
        input.value = '';
        input.focus();
        selectedIndex = -1;
        results.innerHTML = '<p class="search-modal-placeholder">Start typing to search...</p>';
        loadSearchIndex();
    }

    function closeModal() {
        modal.style.display = 'none';
        input.value = '';
    }

    async function loadSearchIndex() {
        if (searchData || isLoading) return;
        isLoading = true;

        try {
            const response = await fetch('/search-index.json');
            searchData = await response.json();

            // Load Fuse.js from CDN
            if (typeof Fuse === 'undefined') {
                await loadScript('https://cdn.jsdelivr.net/npm/fuse.js@7.0.0/dist/fuse.min.js');
            }

            fuseInstance = new Fuse(searchData, {
                keys: [
                    { name: 'title', weight: 3 },
                    { name: 'body', weight: 1 },
                    { name: 'section', weight: 0.5 }
                ],
                threshold: 0.4,
                includeScore: true,
                includeMatches: true,
                minMatchCharLength: 2,
                ignoreLocation: true
            });
        } catch (err) {
            results.innerHTML = '<p class="search-modal-error">Failed to load search index.</p>';
        } finally {
            isLoading = false;
        }
    }

    function performSearch(query) {
        selectedIndex = -1;

        if (!query) {
            results.innerHTML = '<p class="search-modal-placeholder">Start typing to search...</p>';
            return;
        }

        if (!fuseInstance) {
            results.innerHTML = '<p class="search-modal-loading">Loading search index...</p>';
            return;
        }

        const matches = fuseInstance.search(query, { limit: 10 });

        if (matches.length === 0) {
            results.innerHTML = '<p class="search-modal-no-results">No results found.</p>';
            return;
        }

        results.innerHTML = matches.map(function (match) {
            const item = match.item;
            const score = Math.round((1 - match.score) * 100);
            const snippet = getSnippet(item.body, query, 120);

            return '<div class="search-result-item">' +
                '<a href="' + escapeHtml(item.url) + '" class="search-result-link" onclick="closeSearch()">' +
                '<div class="search-result-header">' +
                '<span class="search-result-title">' + highlightMatch(escapeHtml(item.title), query) + '</span>' +
                (item.section ? '<span class="search-result-score">' + escapeHtml(item.section) + '</span>' : '') +
                '</div>' +
                (snippet ? '<p class="search-result-snippet">' + highlightMatch(escapeHtml(snippet), query) + '</p>' : '') +
                '<span class="search-result-url">' + escapeHtml(item.url) + '</span>' +
                '</a></div>';
        }).join('');
    }

    function updateSelection(items) {
        items.forEach(function (item, i) {
            if (i === selectedIndex) {
                item.querySelector('a').classList.add('bg-base-50', 'dark:bg-base-800');
                item.scrollIntoView({ block: 'nearest' });
            } else {
                item.querySelector('a').classList.remove('bg-base-50', 'dark:bg-base-800');
            }
        });
    }

    function getSnippet(text, query, maxLen) {
        if (!text) return '';
        var lower = text.toLowerCase();
        var qi = lower.indexOf(query.toLowerCase());
        if (qi === -1) return text.substring(0, maxLen) + (text.length > maxLen ? '...' : '');
        var start = Math.max(0, qi - 40);
        var end = Math.min(text.length, qi + query.length + maxLen - 40);
        return (start > 0 ? '...' : '') + text.substring(start, end) + (end < text.length ? '...' : '');
    }

    function highlightMatch(text, query) {
        if (!query) return text;
        var regex = new RegExp('(' + escapeRegex(query) + ')', 'gi');
        return text.replace(regex, '<span class="search-highlight">$1</span>');
    }

    function escapeHtml(str) {
        var div = document.createElement('div');
        div.textContent = str || '';
        return div.innerHTML;
    }

    function escapeRegex(str) {
        return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }

    function debounce(fn, delay) {
        var timer;
        return function () {
            clearTimeout(timer);
            timer = setTimeout(fn, delay);
        };
    }

    function loadScript(src) {
        return new Promise(function (resolve, reject) {
            var script = document.createElement('script');
            script.src = src;
            script.onload = resolve;
            script.onerror = reject;
            document.head.appendChild(script);
        });
    }

    // Global close for inline onclick
    window.closeSearch = closeModal;
})();
