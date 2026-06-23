/**
 * Page Manager - Centralized JavaScript functionality
 * Handles theme switching, table of contents, tabs, syntax highlighting, and mobile navigation
 */
class PageManager {
    constructor() {
        this.init();
    }

    init() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.initializeComponents());
        } else {
            this.initializeComponents();
        }
    }

    initializeComponents() {
        this.themeManager = new ThemeManager();
        this.outlineManager = new OutlineManager();
        this.tabManager = new TabManager();
        this.contentTabManager = new ContentTabManager();
        this.syntaxHighlighter = new SyntaxHighlighter();
        this.mermaidManager = new MermaidManager();
        this.mobileNavManager = new MobileNavManager();
        this.mainSiteNavManager = new MainSiteNavManager();
        this.sidebarToggleManager = new SidebarToggleManager();
        this.searchManager = new SearchManager();
        this.areaNavManager = new AreaNavManager();
        this.sidebarStateManager = new SidebarStateManager(this.areaNavManager);
        this.languageSwitcherManager = new LanguageSwitcherManager();

        // Initialize all components
        this.outlineManager.init();
        this.tabManager.init();
        this.contentTabManager.init();
        this.syntaxHighlighter.init();
        this.mermaidManager.init();
        this.mobileNavManager.init();
        this.mainSiteNavManager.init();
        this.sidebarToggleManager.init();
        this.searchManager.init();
        this.areaNavManager.init();
        this.sidebarStateManager.init();
        this.languageSwitcherManager.init();
    }

    onSpaNavigating() {
        const outlineUl = document.querySelector('[data-role="page-outline"] ul');
        if (outlineUl) outlineUl.innerHTML = '';
    }

    onSpaCommit(doc) {
        // The sidebar and header live outside any data-spa-region so their
        // DOM, scroll position, focus, and user-driven state (including the
        // SearchManager's #search-input reference) survive navigation. Patch
        // their per-page state from the destination's server-rendered markup
        // so server-side selection logic stays the source of truth.
        this.sidebarStateManager?.patch(doc);
        this.languageSwitcherManager?.patch(doc);

        // Re-run content-derived JS for the freshly-swapped regions.
        this.syntaxHighlighter?.init();
        this.tabManager?.init();
        this.contentTabManager?.init();
        this.mermaidManager?.init();
        this.outlineManager?.init();
    }
}

/**
 * Theme Manager - Handles dark/light theme switching
 */
class ThemeManager {
    constructor() {
        this.bindThemeToggleEvents();
        
        // Make swapTheme globally available for backwards compatibility
        window.swapTheme = this.swapTheme.bind(this);
    }

    bindThemeToggleEvents() {
        // Find all elements with data-theme-toggle attribute
        const themeToggleButtons = document.querySelectorAll('[data-theme-toggle]');
        
        themeToggleButtons.forEach(button => {
            button.addEventListener('click', () => {
                this.swapTheme();
            });
        });
    }

    swapTheme() {
        const dark = !document.documentElement.classList.contains('dark');

        // Suppress transitions for one frame so the swap is atomic. Without
        // this, elements with `transition-colors` / `transition-all` (search
        // button, GitHub icon, toggle) lag ~150ms behind the header/body —
        // which have no transition and snap instantly — producing a flash.
        const css = document.createElement('style');
        css.appendChild(document.createTextNode(
            '*,*::before,*::after{transition:none !important}'
        ));
        document.head.appendChild(css);
        document.documentElement.classList.toggle('dark', dark);
        try { localStorage.setItem('theme', dark ? 'dark' : 'light'); } catch (e) { }
        // Re-match the browser chrome to the new theme (defined inline in each
        // site's <head> bootstrap, so it may be absent on bare hosts).
        if (window.syncThemeColor) window.syncThemeColor();
        // Force the override into the same paint as the class flip, then drop
        // it on the next frame so hover transitions keep working normally.
        window.getComputedStyle(css).opacity;
        requestAnimationFrame(function () { document.head.removeChild(css); });

        if (window.pageManager && window.pageManager.mermaidManager) {
            window.pageManager.mermaidManager.reinitializeForTheme();
        }
    }
}

/**
 * Outline Manager - Handles outline navigation and active section highlighting
 */
class OutlineManager {
    constructor() {
        this.outlineLinks = [];
        this.sectionMap = new Map();
        this.sections = [];
        this.isScrolling = false;
        this.scrollTimeout = null;
        this._scrollAbort = null;
    }

    init() {
        this.reset();
        this.setupOutline();
        if (this.outlineLinks.length > 0) {
            this.setupScrollListener();
            // Initial highlight
            this.updateActiveSection();
        }
    }

    reset() {
        if (this._scrollAbort) {
            this._scrollAbort.abort();
            this._scrollAbort = null;
        }
        this.sectionMap = new Map();
        this.sections = [];
        this.outlineLinks = [];
    }

    setupOutline() {
        const outlineContainer = document.querySelector('[data-role="page-outline"]');
        if (!outlineContainer) return;

        const contentSelector = outlineContainer.dataset.contentSelector;
        if (contentSelector) {
            // Build outline from content
            this.buildOutlineFromContent(contentSelector, outlineContainer);
        }

        // Get all outline links (either pre-rendered or dynamically generated)
        this.outlineLinks = Array.from(document.querySelectorAll('[data-role="page-outline"] ul li a'));

        // Initialize all links and build section map
        this.outlineLinks.forEach(link => {
            link.dataset.selected = 'false';

            const id = this.extractIdFromHref(link.getAttribute('href'));
            if (id) {
                const section = document.getElementById(id);
                if (section) {
                    this.sectionMap.set(section, link);
                    this.sections.push(section);
                }
            }
        });

        // Sort sections by document order
        this.sections.sort((a, b) => {
            const pos = a.compareDocumentPosition(b);
            return pos & Node.DOCUMENT_POSITION_FOLLOWING ? -1 : 1;
        });
    }

    buildOutlineFromContent(contentSelector, outlineContainer) {
        const contentElement = document.querySelector(contentSelector);
        if (!contentElement) return;

        // Extract H2 and H3 headings
        const headings = this.extractHeadings(contentElement);
        if (headings.length === 0) return;

        // Build outline structure (shallowest level as parents, deeper as children)
        const outlineStructure = this.buildOutlineStructure(headings);

        // Render the outline
        this.renderOutline(outlineStructure, outlineContainer);
    }

    extractHeadings(contentElement) {
        const headingElements = contentElement.querySelectorAll('h2, h3');
        const headings = [];

        headingElements.forEach(heading => {
            // Ensure heading has an ID
            if (!heading.id) {
                heading.id = this.generateIdFromText(heading.textContent);
            }

            headings.push({
                level: parseInt(heading.tagName[1]), // Extract 2 from H2, 3 from H3
                id: heading.id,
                text: heading.textContent.trim(),
                element: heading
            });
        });

        return headings;
    }

    generateIdFromText(text) {
        return text
            .toLowerCase()
            .trim()
            .replace(/[^\w\s-]/g, '') // Remove special characters
            .replace(/\s+/g, '-') // Replace spaces with hyphens
            .replace(/-+/g, '-') // Replace multiple hyphens with single hyphen
            .replace(/^-|-$/g, ''); // Remove leading/trailing hyphens
    }

    buildOutlineStructure(headings) {
        const structure = [];
        if (headings.length === 0) return structure;

        // Use the shallowest heading level present as the outline's top level so a
        // page that starts at H3 (no H2) still produces a populated outline. Deeper
        // headings nest under the most recent top-level entry; a deeper heading with
        // no preceding top-level entry is promoted so it is never dropped.
        const topLevel = Math.min(...headings.map(h => h.level));
        let currentParent = null;

        headings.forEach(heading => {
            if (heading.level === topLevel || !currentParent) {
                currentParent = { id: heading.id, text: heading.text, children: [] };
                structure.push(currentParent);
            } else {
                currentParent.children.push({ id: heading.id, text: heading.text });
            }
        });

        return structure;
    }

    renderOutline(outlineStructure, outlineContainer) {
        const ul = outlineContainer.querySelector('ul');
        if (!ul) return;

        // Get CSS classes from data attributes
        const linkClass = ul.dataset.outlineLinkClass || '';
        const nestedLinkClass = ul.dataset.outlineNestedLinkClass || '';

        // Clear existing content
        ul.innerHTML = '';

        // Render each entry
        outlineStructure.forEach(entry => {
            // Create parent link
            const parentLi = this.createOutlineLink(entry.id, entry.text, '', linkClass);
            ul.appendChild(parentLi);

            // Create children if any
            if (entry.children.length > 0) {
                const childrenContainer = document.createElement('li');
                const childrenUl = document.createElement('ul');

                entry.children.forEach(child => {
                    const childLi = this.createOutlineLink(child.id, child.text, nestedLinkClass, linkClass);
                    childrenUl.appendChild(childLi);
                });

                childrenContainer.appendChild(childrenUl);
                ul.appendChild(childrenContainer);
            }
        });
    }

    createOutlineLink(id, text, nestedClass, linkClass) {
        const li = document.createElement('li');
        li.className = `${nestedClass} flex`.trim();

        const a = document.createElement('a');
        a.className = `${nestedClass} ${linkClass}`.trim();
        a.href = `#${id}`;
        a.textContent = text;
        a.dataset.selected = 'false';

        li.appendChild(a);
        return li;
    }

    extractIdFromHref(href) {
        return href?.split('#')[1] || null;
    }

    setupScrollListener() {
        this._scrollAbort = new AbortController();
        window.addEventListener('scroll', () => {
            if (!this.isScrolling) {
                this.isScrolling = true;
                requestAnimationFrame(() => {
                    this.updateActiveSection();
                    this.isScrolling = false;
                });
            }
        }, { passive: true, signal: this._scrollAbort.signal });
    }

    updateActiveSection() {
        this.resetAllLinks();

        const activeSection = this.findActiveSection();
        if (activeSection) {
            const link = this.sectionMap.get(activeSection);
            if (link) {
                this.activateLink(link);
            }
        }
    }

    findActiveSection() {
        if (this.sections.length === 0) return null;

        const HEADER_OFFSET = 130; // Account for fixed header
        const READING_POSITION = HEADER_OFFSET + 50; // Slightly below header for better UX

        // Find the section that should be highlighted based on scroll position
        let activeSection = null;

        for (let i = this.sections.length - 1; i >= 0; i--) {
            const section = this.sections[i];
            const rect = section.getBoundingClientRect();

            // If section top is at or above our reading position, this is our active section
            if (rect.top <= READING_POSITION) {
                activeSection = section;
                break;
            }
        }

        // If no section is above the reading position, use the first section
        return activeSection || this.sections[0];
    }

    resetAllLinks() {
        this.outlineLinks.forEach(link => {
            link.dataset.selected = 'false';
            link.parentElement?.classList.remove('active');
        });
        this.hideHighlighter();
    }

    activateLink(link) {
        link.dataset.selected = 'true';
        link.parentElement?.classList.add('active');
        this.updateHighlighter(link);
    }

    updateHighlighter(link) {
        const highlighter = document.querySelector('[data-role="page-outline-highlighter"]');
        if (!highlighter || !link) return;

        const linkRect = link.getBoundingClientRect();
        const outlineContainer = document.querySelector('[data-role="page-outline"]');
        if (!outlineContainer) return;

        const containerRect = outlineContainer.getBoundingClientRect();
        
        // Calculate position relative to the outline container
        const top = linkRect.top - containerRect.top;
        const height = linkRect.height;

        // Update highlighter position and visibility
        highlighter.style.top = `${top}px`;
        highlighter.style.height = `${height}px`;
        highlighter.classList.remove('opacity-0');
        highlighter.classList.add('opacity-100');
    }

    hideHighlighter() {
        const highlighter = document.querySelector('[data-role="page-outline-highlighter"]');
        if (highlighter) {
            highlighter.classList.remove('opacity-100');
            highlighter.classList.add('opacity-0');
        }
    }

    destroy() {
        // Clean up scroll listener if needed
        // Note: In practice, this is rarely called as the page manager persists
    }
}

/**
 * Tab Manager - Handles tab navigation and content switching
 */
class TabManager {
    constructor() {
        this.tablists = [];
    }

    init() {
        this.tablists = Array.from(document.querySelectorAll('[role="tablist"]'));
        this.tablists.forEach(tablist => this.setupTablist(tablist));
    }

    setupTablist(tablist) {
        const tablistId = tablist.id;
        if (!tablistId) return;

        const tabs = Array.from(tablist.querySelectorAll('[role="tab"]'));
        if (tabs.length === 0) return;

        // Set up event listeners
        tabs.forEach(tab => {
            tab.addEventListener('click', () => this.activateTab(tab, tabs));
        });

        // Initialize active state
        this.initializeActiveTab(tablist, tabs);
    }

    initializeActiveTab(tablist, tabs) {
        const activeTab = tablist.querySelector('[data="true"]');

        if (!activeTab && tabs.length > 0) {
            this.activateTab(tabs[0], tabs);
        } else if (activeTab) {
            this.showTabContent(activeTab);
        }
    }

    activateTab(selectedTab, allTabs) {
        // Deactivate all tabs
        allTabs.forEach(tab => {
            tab.dataset.selected = 'false';
            tab.setAttribute('data-state', 'inactive');
            tab.setAttribute('tabindex', '-1');
        });

        // Activate the selected tab
        selectedTab.dataset.selected = 'true';
        selectedTab.setAttribute('data-state', 'active');
        selectedTab.setAttribute('tabindex', '0');

        // Show corresponding content
        this.showTabContent(selectedTab);
    }

    showTabContent(tab) {
        const contentId = tab.getAttribute('aria-controls');
        if (!contentId) return;

        const contentPanel = document.getElementById(contentId);
        if (!contentPanel) return;

        // Hide all related content panels
        this.hideRelatedContentPanels(tab);

        // Show the selected content panel
        contentPanel.removeAttribute('hidden');
        contentPanel.dataset.selected = 'true';
    }

    hideRelatedContentPanels(tab) {
        const tabId = tab.id;
        const match = tabId.match(/^tabButton(.*)-\d+$/);

        if (match) {
            const baseId = match[1];
            const allContentPanels = document.querySelectorAll(`[id^="tab-content${baseId}-"]`);

            allContentPanels.forEach(panel => {
                panel.dataset.selected = 'false';
                panel.setAttribute('hidden', '');
            });
        }
    }
}

/**
 * Content Tab Manager - Handles DocFX-style content tabs (.ctabs).
 *
 * Tab ids that appear as buttons in the same group are union-found into one
 * "set"; selecting an id syncs every group that shares the set, so picking
 * "Windows" once selects it page-wide. A panel with a data-condition is a
 * dependent tab: it shows only when its condition id is also the selected id
 * of the condition's set. Selection persists per set in localStorage.
 */
class ContentTabManager {
    constructor() {
        this.groups = [];
        this.parent = new Map();   // union-find: tab id -> parent
        this.selected = new Map(); // set root -> selected tab id
    }

    init() {
        this.groups = Array.from(document.querySelectorAll('[data-content-tabs]'));
        if (this.groups.length === 0) return;

        this.parent = new Map();
        this.selected = new Map();

        // Union every group's button ids into one set.
        for (const group of this.groups) {
            const ids = this.buttonIds(group);
            for (let i = 1; i < ids.length; i++) this.union(ids[0], ids[i]);
        }

        // Seed each set's selection from localStorage, else its first id.
        for (const group of this.groups) {
            for (const id of this.buttonIds(group)) {
                const root = this.find(id);
                if (!this.selected.has(root)) {
                    const stored = this.readStored(root);
                    this.selected.set(root, stored && this.find(stored) === root ? stored : id);
                }
            }
        }

        for (const group of this.groups) {
            for (const btn of this.buttons(group)) {
                btn.addEventListener('click', () => this.select(btn.dataset.tab));
            }
        }

        this.render();
    }

    buttons(group) {
        return Array.from(group.querySelectorAll(':scope > [role="tablist"] > .ctab-btn[data-tab]'));
    }

    buttonIds(group) {
        return this.buttons(group).map(b => b.dataset.tab);
    }

    find(id) {
        if (!this.parent.has(id)) { this.parent.set(id, id); return id; }
        let root = id;
        while (this.parent.get(root) !== root) root = this.parent.get(root);
        let cur = id;
        while (this.parent.get(cur) !== root) {
            const next = this.parent.get(cur);
            this.parent.set(cur, root);
            cur = next;
        }
        return root;
    }

    union(a, b) {
        const ra = this.find(a), rb = this.find(b);
        if (ra !== rb) this.parent.set(rb, ra);
    }

    select(tabId) {
        const root = this.find(tabId);
        this.selected.set(root, tabId);
        this.writeStored(root, tabId);
        this.render();
    }

    render() {
        for (const group of this.groups) {
            const ids = this.buttonIds(group);
            if (ids.length === 0) continue;

            const setRoot = this.find(ids[0]);
            const selectedId = this.selected.get(setRoot);
            const active = ids.includes(selectedId) ? selectedId : ids[0];

            for (const btn of this.buttons(group)) {
                const on = btn.dataset.tab === active;
                btn.dataset.active = String(on);
                btn.setAttribute('aria-selected', String(on));
            }

            for (const panel of group.querySelectorAll(':scope > .ctab-panel[data-tab]')) {
                let visible = panel.dataset.tab === active;
                const condition = panel.dataset.condition || '';
                if (visible && condition) {
                    visible = this.selected.get(this.find(condition)) === condition;
                }
                panel.dataset.active = String(visible);
            }
        }
    }

    readStored(root) {
        try { return localStorage.getItem('pn-ctab:' + root); } catch (e) { return null; }
    }

    writeStored(root, tabId) {
        try { localStorage.setItem('pn-ctab:' + root, tabId); } catch (e) { }
    }
}

/**
 * Mermaid Manager - Handles mermaid diagram rendering with theme support
 */
class MermaidManager {
    constructor() {
        this.mermaidLoaded = false;
        this.mermaidInstance = null;
        this.diagrams = [];
        this.renderedDiagrams = []; // Track rendered diagram containers
    }

    async init() {
        this.renderedDiagrams = [];
        this.diagrams = this.findMermaidDiagrams();
        if (this.diagrams.length === 0) return;

        try {
            await this.loadMermaid();
            await this.renderDiagrams();
        } catch (error) {
            console.error('Failed to initialize mermaid:', error);
        }
    }

    findMermaidDiagrams() {
        // Look for code blocks with class 'language-mermaid'
        return Array.from(document.querySelectorAll('code.language-mermaid'));
    }

    async loadMermaid() {
        if (this.mermaidLoaded) return;

        // Dynamically load mermaid from CDN
        this.mermaidInstance = await import('https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs');
        this.mermaidLoaded = true;
        
        this.initializeMermaid();
    }

    initializeMermaid() {
        if (!this.mermaidInstance) return;

        const isDark = document.documentElement.classList.contains('dark');
        const config = this.getMermaidConfig(isDark);

        // Use the correct initialization method
        this.mermaidInstance.default.initialize(config);
    }

    getMermaidConfig(isDark) {
        // Helper function to get CSS variables with fallbacks
        function getCSSVariable(variable, fallback) {
            if (typeof window === 'undefined' || typeof document === 'undefined') {
                return fallback;
            }

            const value = getComputedStyle(document.documentElement).getPropertyValue(variable).trim() || fallback;

            if (value.startsWith('oklch(')) {
                let s = oklchToHex(value);
                return s;
            }

            return value;
        }

        // Convert OKLCH string to hex (e.g. "oklch(0.881 0.061 210)" → "#hex")
        function oklchToHex(oklchStr) {
            // Parse the values from the string (handle both decimal and percentage for lightness)
            const match = oklchStr.match(/oklch\(\s*([\d.]+)%?\s+([\d.]+)\s+([\d.]+)\s*\)/);
            if (!match) return '#000000';

            let l = parseFloat(match[1]);
            const c = parseFloat(match[2]);
            const h = parseFloat(match[3]);

            // Normalize lightness: if > 1, assume percentage and divide by 100
            if (l > 1) {
                l = l / 100;
            }

            // Convert OKLCH to OKLab
            const hRad = (h * Math.PI) / 180; // Correct hue conversion (360° range)
            const a = Math.cos(hRad) * c;
            const b = Math.sin(hRad) * c;

            // Convert OKLab to LMS (cone response)
            const l_lms = l + 0.3963377774 * a + 0.2158037573 * b;
            const m_lms = l - 0.1055613458 * a - 0.0638541728 * b;
            const s_lms = l - 0.0894841775 * a - 1.2914855480 * b;

            // Cube the LMS values to get linear LMS
            const l_linear = Math.pow(l_lms, 3);
            const m_linear = Math.pow(m_lms, 3);
            const s_linear = Math.pow(s_lms, 3);

            // Convert linear LMS to linear RGB
            const r_linear = +4.0767416621 * l_linear - 3.3077115913 * m_linear + 0.2309699292 * s_linear;
            const g_linear = -1.2684380046 * l_linear + 2.6097574011 * m_linear - 0.3413193965 * s_linear;
            const b_linear = -0.0041960863 * l_linear - 0.7034186147 * m_linear + 1.7076147010 * s_linear;

            // Convert linear RGB to sRGB
            const r = srgbTransferFn(r_linear);
            const g = srgbTransferFn(g_linear);
            const b_srgb = srgbTransferFn(b_linear);

            return rgbToHex(r, g, b_srgb);
        }

        function srgbTransferFn(x) {
            // Clamp to valid range first
            x = Math.max(0, Math.min(1, x));
            
            return x <= 0.0031308
                ? 12.92 * x
                : 1.055 * Math.pow(x, 1 / 2.4) - 0.055;
        }

        function rgbToHex(r, g, b) {
            const to255 = (x) => Math.max(0, Math.min(255, Math.round(x * 255)));
            return (
                '#' +
                to255(r).toString(16).padStart(2, '0') +
                to255(g).toString(16).padStart(2, '0') +
                to255(b).toString(16).padStart(2, '0')
            );
        }

        if (isDark) {
            return {
                startOnLoad: false,
                securityLevel: 'loose',
                logLevel: 'error',
                theme: 'base',
                darkMode: true,
                themeVariables: {
                    fontFamily: getCSSVariable('--font-sans', 'ui-sans-serif, system-ui, sans-serif, "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol", "Noto Color Emoji"'),

                    // Main colors
                    primaryColor: getCSSVariable('--color-primary-600', '#BB2528'),
                    primaryTextColor: getCSSVariable('--color-primary-50', '#ffffff'),
                    
                    // Secondary colors
                    secondaryColor: getCSSVariable('--color-accent-600', '#006100'),
                    tertiaryColor: getCSSVariable('--color-accent-600', '#666666'),
                    
                    // Background colors
                    background: getCSSVariable('--color-base-950', '#0a0a0a'),
                    mainBkg: getCSSVariable('--color-base-900', '#1a1a1a'),
                    nodeBkg: getCSSVariable('--color-base-900', '#1a1a1a'),
                    secondaryBkg: getCSSVariable('--color-base-800', '#2a2a2a'),
                    tertiaryBkg: getCSSVariable('--color-base-700', '#333333'),

                    // Note colors
                    noteBorderColor: getCSSVariable('--color-base-600', '#333333'),
                    noteBkgColor: getCSSVariable('--color-base-800', '#333333'),
                    
                    // Lines and borders
                    lineColor: getCSSVariable('--color-accent-400', '#4ade80'),
                    primaryBorderColor: getCSSVariable('--color-primary-500', '#dc2626'),
                    nodeBorder: getCSSVariable('--color-primary-500', '#dc2626'),
                    secondaryBorderColor: getCSSVariable('--color-accent-500', '#22c55e'),
                    tertiaryBorderColor: getCSSVariable('--color-accent-500', '#6b7280'),
                    
                    // Text colors
                    textColor: getCSSVariable('--color-base-300', '#f3f4f6'),
                    nodeTextColor: getCSSVariable('--color-primary-50', '#ffffff'),
                    edgeLabelColor: getCSSVariable('--color-base-200', '#e5e7eb'),
                    
                    // Edge and label backgrounds
                    edgeLabelBackground: getCSSVariable('--color-base-800', '#1f2937'),
                    
                    // Additional node colors for variety
                    node0: getCSSVariable('--color-primary-600', '#dc2626'),
                    node1: getCSSVariable('--color-accent-600', '#059669'),
                    node2: getCSSVariable('--color-accent-600', '#4b5563'),
                    node3: getCSSVariable('--color-primary-600', '#7c3aed')
                }
            };
        } else {
            return {
                startOnLoad: false,
                securityLevel: 'loose',
                logLevel: 'error',
                theme: 'base',
                darkMode: false,
                themeVariables: {
                    fontFamily: getCSSVariable('--font-sans', 'ui-sans-serif, system-ui, sans-serif, "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol", "Noto Color Emoji"'),

                    // Main colors
                    primaryColor: getCSSVariable('--color-primary-700', '#BB2528'),
                    primaryTextColor: getCSSVariable('--color-base-500', '#ffffff'),
                    
                    // Secondary colors
                    secondaryColor: getCSSVariable('--color-accent-700', '#006100'),
                    tertiaryColor: getCSSVariable('--color-accent-600', '#4b5563'),
                    
                    // Background colors
                    background: getCSSVariable('--color-base-50', '#f9fafb'),
                    mainBkg: getCSSVariable('--color-base-100', '#f3f4f6'),
                    nodeBkg: getCSSVariable('--color-base-100', '#f3f4f6'),
                    secondaryBkg: getCSSVariable('--color-base-200', '#e5e7eb'),
                    tertiaryBkg: getCSSVariable('--color-base-150', '#f0f0f0'),

                    // Note colors
                    noteBorderColor: getCSSVariable('--color-base-200', '#333333'),
                    noteBkgColor: getCSSVariable('--monorail-color-base-100', '#333333'),


                    // Lines and borders
                    lineColor: getCSSVariable('--color-accent-600', '#16a34a'),
                    primaryBorderColor: getCSSVariable('--color-primary-600', '#dc2626'),
                    nodeBorder: getCSSVariable('--color-primary-600', '#dc2626'),
                    secondaryBorderColor: getCSSVariable('--color-accent-600', '#16a34a'),
                    tertiaryBorderColor: getCSSVariable('--color-accent-400', '#9ca3af'),
                    
                    // Text colors
                    textColor: getCSSVariable('--color-base-900', '#111827'),
                    nodeTextColor: getCSSVariable('--color-base-900', '#ffffff'),
                    edgeLabelColor: getCSSVariable('--color-base-700', '#374151'),
                    
                    // Edge and label backgrounds
                    edgeLabelBackground: getCSSVariable('--color-base-100', '#f3f4f6'),
                    
                    // Additional node colors for variety
                    node0: getCSSVariable('--color-primary-600', '#dc2626'),
                    node1: getCSSVariable('--color-accent-600', '#16a34a'),
                    node2: getCSSVariable('--color-accent-600', '#4b5563'),
                    node3: getCSSVariable('--color-primary-600', '#7c3aed')
                }
            };
        }
    }

    async renderDiagrams() {
        if (!this.mermaidInstance || this.diagrams.length === 0) return;

        for (let i = 0; i < this.diagrams.length; i++) {
            const codeElement = this.diagrams[i];
            const diagramText = codeElement.textContent;
            
            try {
                const {svg} = await this.mermaidInstance.default.render(`mermaid-diagram-${i}`, diagramText);
                
                // Create a div to hold the SVG
                const diagramContainer = document.createElement('div');
                diagramContainer.className = 'mermaid-diagram';
                diagramContainer.innerHTML = svg;
                diagramContainer.dataset.originalText = diagramText; // Store original text for re-rendering
                
                // Replace the code element with the rendered diagram
                codeElement.parentNode.replaceChild(diagramContainer, codeElement);
                
                // Track the rendered diagram
                this.renderedDiagrams.push(diagramContainer);
            } catch (error) {
                console.error(`Failed to render mermaid diagram ${i}:`, error);
            }
        }
    }

    async reinitializeForTheme() {
        if (!this.mermaidLoaded || this.renderedDiagrams.length === 0) return;

        // Re-initialize mermaid with new theme
        this.initializeMermaid();

        // Use timestamp to ensure unique diagram IDs force Mermaid to apply new theme
        const timestamp = Date.now();

        // Re-render all existing diagrams
        for (let i = 0; i < this.renderedDiagrams.length; i++) {
            const diagramContainer = this.renderedDiagrams[i];
            const diagramText = diagramContainer.dataset.originalText;

            if (diagramText) {
                try {
                    // Unique ID with timestamp forces fresh render with new theme
                    const {svg} = await this.mermaidInstance.default.render(
                        `mermaid-diagram-${timestamp}-${i}`,
                        diagramText
                    );
                    diagramContainer.innerHTML = svg;
                } catch (error) {
                    console.error(`Failed to re-render mermaid diagram ${i} for theme:`, error);
                }
            }
        }
    }
}

/**
 * Mobile Navigation Manager - Handles mobile menu toggle and interaction
 */
class MobileNavManager {
    constructor() {
        this.menuToggle = null;
        this.navSidebar = null;
        this.mobileOverlay = null;
        this.isInitialized = false;
    }

    init() {
        this.menuToggle = document.getElementById('menu-toggle');
        this.navSidebar = document.getElementById('nav-sidebar');
        this.mobileOverlay = document.getElementById('mobile-overlay');
        
        if (this.menuToggle && this.navSidebar) {
            this.setupEventListeners();
            this.isInitialized = true;
        }
    }

    setupEventListeners() {
        // Toggle menu on button click
        this.menuToggle.addEventListener('click', () => {
            this.toggleMenu();
        });
        
        // Close menu when clicking on a link (mobile only)
        this.navSidebar.addEventListener('click', (e) => {
            if (e.target.tagName === 'A' && window.innerWidth < 1024) {
                this.closeMenu();
            }
        });
        
        // Close menu when clicking on overlay
        if (this.mobileOverlay) {
            this.mobileOverlay.addEventListener('click', () => {
                this.closeMenu();
            });
        }
        
        // Close menu when clicking outside (mobile only)
        document.addEventListener('click', (e) => {
            if (window.innerWidth < 1024 && 
                !this.navSidebar.contains(e.target) && 
                !this.menuToggle.contains(e.target) && 
                this.isMenuOpen()) {
                this.closeMenu();
            }
        });

        // Close menu on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isMenuOpen()) {
                this.closeMenu();
            }
        });
    }

    toggleMenu() {
        if (this.isMenuOpen()) {
            this.closeMenu();
        } else {
            this.openMenu();
        }
    }

    isMenuOpen() {
        return this.navSidebar.getAttribute('aria-expanded') === 'true';
    }

    closeMenu() {
        this.navSidebar.dataset.expanded = 'false';
        
        if (this.mobileOverlay) {
            this.mobileOverlay.setAttribute('aria-hidden', 'true');
        }
        
        // Re-enable body scrolling
        document.body.setAttribute('data-mobile-menu-open', 'false');
    }

    openMenu() {
        this.navSidebar.dataset.expanded = 'true';
        
        if (this.mobileOverlay) {
            this.mobileOverlay.setAttribute('aria-hidden', 'false');
        }
        
        // Prevent body scrolling when menu is open
        document.body.setAttribute('data-mobile-menu-open', 'true');
    }
}

/**
 * Main Site Navigation Manager - Handles hamburger menu for main site links
 */
class MainSiteNavManager {
    constructor() {
        this.menuButton = null;
        this.mobileMenu = null;
    }

    init() {
        this.menuButton = document.getElementById('mobile-menu-button');
        this.mobileMenu = document.getElementById('mobile-menu');
        
        if (this.menuButton && this.mobileMenu) {
            this.setupEventListeners();
        }
    }

    setupEventListeners() {
        // Toggle menu on button click
        this.menuButton.addEventListener('click', () => {
            this.toggleMenu();
        });
        
        // Close menu when clicking on a link
        this.mobileMenu.addEventListener('click', (e) => {
            if (e.target.tagName === 'A') {
                this.closeMenu();
            }
        });
        
        // Close menu when clicking outside
        document.addEventListener('click', (e) => {
            if (!this.mobileMenu.contains(e.target) && 
                !this.menuButton.contains(e.target) && 
                this.isMenuOpen()) {
                this.closeMenu();
            }
        });

        // Close menu on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isMenuOpen()) {
                this.closeMenu();
            }
        });
        
        // Close menu when window is resized to desktop
        window.addEventListener('resize', () => {
            if (window.innerWidth >= 768) { // md breakpoint
                this.closeMenu();
            }
        });
    }

    toggleMenu() {
        if (this.isMenuOpen()) {
            this.closeMenu();
        } else {
            this.openMenu();
        }
    }

    isMenuOpen() {
        return this.mobileMenu.dataset.expanded === 'true';
    }

    openMenu() {
        this.mobileMenu.dataset.expanded = 'true';
    }

    closeMenu() {
        this.mobileMenu.dataset.expanded = 'false';
    }
}

/**
 * Sidebar Toggle Manager - Handles table of contents sidebar toggle for Spectre.Console-style layouts
 */
class SidebarToggleManager {
    constructor() {
        this.sidebarToggle = null;
        this.sidebarOverlay = null;
        this.sidebarClose = null;
        this.sidebarPanel = null;
    }

    init() {
        this.sidebarToggle = document.getElementById('sidebar-toggle');
        this.sidebarOverlay = document.getElementById('sidebar-overlay');
        this.sidebarClose = document.getElementById('sidebar-close');
        this.sidebarPanel = document.getElementById('sidebar-panel');
        
        if (this.sidebarToggle && this.sidebarOverlay) {
            this.setupEventListeners();
        }
    }

    setupEventListeners() {
        // Toggle sidebar on button click
        this.sidebarToggle.addEventListener('click', () => {
            this.toggleSidebar();
        });
        
        // Close sidebar when clicking close button
        if (this.sidebarClose) {
            this.sidebarClose.addEventListener('click', () => {
                this.closeSidebar();
            });
        }
        
        // Close sidebar when clicking on overlay (but not the panel)
        this.sidebarOverlay.addEventListener('click', (e) => {
            if (e.target === this.sidebarOverlay) {
                this.closeSidebar();
            }
        });
        
        // Close sidebar on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.isSidebarOpen()) {
                this.closeSidebar();
            }
        });

        // Close sidebar when a link inside the panel is tapped (mobile)
        if (this.sidebarPanel) {
            this.sidebarPanel.addEventListener('click', (e) => {
                if (e.target.closest('a[href]')) {
                    this.closeSidebar();
                }
            });
        }
    }

    toggleSidebar() {
        if (this.isSidebarOpen()) {
            this.closeSidebar();
        } else {
            this.openSidebar();
        }
    }

    isSidebarOpen() {
        return !this.sidebarOverlay.classList.contains('hidden');
    }

    openSidebar() {
        this.sidebarOverlay.classList.remove('hidden');
        document.body.setAttribute('data-sidebar-open', 'true');
    }

    closeSidebar() {
        this.sidebarOverlay.classList.add('hidden');
        document.body.setAttribute('data-sidebar-open', 'false');
    }
}

/**
 * Area Nav Manager - Handles client-side content area switching in the sidebar.
 * The server renders the active-only TOC via a `hidden` CSS class; JS re-applies
 * the same class for pill switching and SPA area transitions.
 */
class AreaNavManager {
    init() {
        this.nav = document.querySelector('[data-area-nav]');
        if (!this.nav) return;

        this.hideInactiveAreas();
        this.nav.addEventListener('click', (e) => this.onPillClick(e));
    }

    hideInactiveAreas() {
        const activeSlug = this.nav.dataset.activeArea;
        for (const toc of document.querySelectorAll('[data-area-toc]')) {
            toc.classList.toggle('hidden', toc.dataset.areaToc !== activeSlug);
        }
    }

    onPillClick(e) {
        const pill = e.target.closest('[data-area]');
        if (!pill) return;
        // Single-page areas have no TOC worth previewing — let the pill's <a href>
        // navigate straight to the page instead of toggling a one-item TOC.
        if (pill.dataset.areaSingle === 'true') return;
        e.preventDefault();
        e.stopPropagation(); // Prevent SPA engine from navigating
        this.switchToArea(pill.dataset.area);
    }

    switchToArea(slug) {
        if (!this.nav) return;

        // Update pills. The dot indicator's bg color is server-stamped from
        // the active-area ternary, not derived from aria-current at the CSS
        // layer, so flip its class alongside the aria attribute — otherwise
        // the dot stays the previous page's color after spa:commit.
        for (const p of this.nav.querySelectorAll('[data-area]')) {
            const isActive = p.dataset.area === slug;
            p.setAttribute('aria-current', isActive ? 'true' : 'false');
            const dot = p.querySelector('[data-area-dot]');
            if (dot) {
                dot.classList.toggle('bg-accent-500', isActive);
                dot.classList.toggle('bg-base-400', !isActive);
            }
        }

        // Show/hide TOCs
        for (const toc of document.querySelectorAll('[data-area-toc]')) {
            toc.classList.toggle('hidden', toc.dataset.areaToc !== slug);
        }

        this.nav.dataset.activeArea = slug;
    }
}

/**
 * Sidebar State Manager - Patches active state on the persistent sidebar after
 * SPA navigation. The sidebar lives outside any data-spa-region so its DOM,
 * scroll position, focus, and any user-driven state survive across navigations.
 * On spa:commit, this manager reads the destination's server-rendered sidebar
 * from the parsed doc and copies data-current values onto the live anchors by
 * href match - so server-side selection logic (locale prefixes, area resolution,
 * IsSelected stamping) stays the source of truth without re-implementing route
 * inference on the client.
 */
class SidebarStateManager {
    constructor(areaNavManager) {
        this.areaNavManager = areaNavManager;
        this.sidebar = null;
    }

    init() {
        this.sidebar = document.getElementById('nav-sidebar');
    }

    patch(doc) {
        if (!this.sidebar || !doc) return;

        const incomingSidebar = doc.getElementById('nav-sidebar');
        if (!incomingSidebar) return;

        // Build href -> data-current map from the incoming sidebar. Match by
        // the raw href attribute (not the resolved .href property) because the
        // parsed doc has no base URL, so .href would not normalize consistently.
        const incomingState = new Map();
        for (const a of incomingSidebar.querySelectorAll('a[data-current]')) {
            const href = a.getAttribute('href');
            if (href) incomingState.set(href, a.getAttribute('data-current'));
        }

        // Apply to live anchors. Anchors absent from the incoming map fall back
        // to "false" so the previously-active link does not retain its highlight.
        for (const a of this.sidebar.querySelectorAll('a[data-current]')) {
            const href = a.getAttribute('href');
            const next = href && incomingState.has(href) ? incomingState.get(href) : 'false';
            if (a.getAttribute('data-current') !== next) {
                a.setAttribute('data-current', next);
            }
        }

        // Multi-area sites: route active-area changes through AreaNavManager so
        // pill aria-current and [data-area-toc] visibility stay consistent.
        const incomingNav = doc.querySelector('[data-area-nav]');
        const liveNav = this.sidebar.querySelector('[data-area-nav]');
        if (incomingNav && liveNav) {
            const incomingArea = incomingNav.getAttribute('data-active-area');
            const liveArea = liveNav.getAttribute('data-active-area');
            if (incomingArea && incomingArea !== liveArea) {
                this.areaNavManager.switchToArea(incomingArea);
            }
        }
    }
}

/**
 * Language Switcher Manager - Patches the persistent header's language
 * switcher after SPA navigation. The header lives outside any data-spa-region
 * so the search button's handlers survive, but that means the per-page
 * alternate-language URLs and current-locale highlight no longer refresh on
 * their own. On spa:commit, this manager copies the destination's
 * server-rendered switcher contents onto the live <details> element. The
 * outer element survives so the dropdown's open/closed state is preserved.
 */
class LanguageSwitcherManager {
    constructor() {
        this.switcher = null;
    }

    init() {
        this.switcher = document.querySelector('[data-lang-switcher]');
    }

    patch(doc) {
        if (!this.switcher || !doc) return;

        const incoming = doc.querySelector('[data-lang-switcher]');
        if (!incoming) return;

        // innerHTML swap covers the current-locale display name, every
        // alternate's href and active styling, and any items added/removed
        // when the destination has different alternates available.
        this.switcher.innerHTML = incoming.innerHTML;
    }
}

/**
 * Search Manager - Handles custom search with FlexSearch
 */
class SearchManager {
    constructor() {
        this.searchInput = null;        // the header search trigger (#search-input)
        this.backdrop = null;
        this.modal = null;
        this.modalInput = null;
        this.resultsBody = null;
        this.clearBtn = null;
        this.engine = null;             // DeweySearchEngine for the loaded locale
        this.engineFailed = false;
        this.loadedLocale = null;
        this._scriptPromise = null;     // cached on-demand injection of dewey-search.js
        this.lastRanked = [];           // ranked results from the last query
        this.lastQuery = '';
        this.queryId = 0;               // monotonic id guarding against stale async renders
        this.selected = 0;              // keyboard-selected visible row index
        this.MAX_RESULTS = 30;          // record budget grouped before rendering stops
        this.MAX_PER_PAGE = 3;          // heading rows shown per page; deeper hits, the user narrows
    }

    async init() {
        this.searchInput = document.getElementById('search-input');
        if (!this.searchInput) return;

        this.createSearchModal();
        this.setupEventListeners();

        // Warm the slim DeweySearch client now (fire-and-forget) so the first
        // open is a same-tick construct rather than a network round-trip. The
        // heavier search index still waits until the user actually searches. A
        // warm failure is swallowed — ensureSearchEngineScript() drops its cache
        // on error, so openModal() re-attempts and surfaces the proper error UI.
        this.ensureSearchEngineScript().catch(() => {});
    }

    // Inline icon set (stroke-style, 24x24). Markup is built in JS, so SVGs live here.
    svg(name, cls = '') {
        const p = {
            search: '<circle cx="11" cy="11" r="7"/><path d="m21 21-4.3-4.3"/>',
            x: '<path d="M18 6 6 18M6 6l12 12"/>',
            bolt: '<path d="M13 2 3 14h9l-1 8 10-12h-9l1-8z"/>',
            chevron: '<path d="m9 18 6-6-6-6"/>',
            hash: '<path d="M9 17H7A5 5 0 0 1 7 7h2"/><path d="M15 7h2a5 5 0 1 1 0 10h-2"/><line x1="8" y1="12" x2="16" y2="12"/>',
            file: '<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/>',
            clock: '<circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>',
            nores: '<circle cx="11" cy="11" r="7"/><path d="m21 21-4.3-4.3"/><line x1="8" y1="11" x2="14" y2="11"/>',
            warn: '<path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/>',
        }[name] || '';
        return `<svg class="${cls}" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">${p}</svg>`;
    }

    createSearchModal() {
        const backdrop = document.createElement('div');
        backdrop.id = 'search-modal-backdrop';
        backdrop.className = 'search-modal-backdrop hidden';
        backdrop.innerHTML = `
            <div class="search-modal" role="dialog" aria-label="Search" aria-modal="true">
                <div class="search-input-row">
                    ${this.svg('search', 'search-input-icon')}
                    <input id="search-modal-input" class="search-input" type="text" placeholder="Search docs…" autocomplete="off" spellcheck="false" />
                    <button id="search-clear-btn" class="search-clear-btn hidden" aria-label="Clear search">${this.svg('x', 'w-3 h-3')}</button>
                    <span class="search-esc-kbd">Esc</span>
                </div>
                <div id="search-results" class="search-results-body"></div>
                <div class="search-foot">
                    <div class="search-powered">${this.svg('bolt', 'w-3 h-3')} Pennington search</div>
                </div>
            </div>`;
        document.body.appendChild(backdrop);

        this.backdrop = backdrop;
        this.modal = backdrop.querySelector('.search-modal');
        this.modalInput = backdrop.querySelector('#search-modal-input');
        this.clearBtn = backdrop.querySelector('#search-clear-btn');
        this.resultsBody = backdrop.querySelector('#search-results');
    }

    setupEventListeners() {
        this.searchInput.addEventListener('click', (e) => {
            e.preventDefault();
            this.openModal();
        });

        // Click on the scrim (outside the modal box) closes.
        this.backdrop.addEventListener('click', (e) => {
            if (e.target === this.backdrop) this.closeModal();
        });

        this.clearBtn.addEventListener('click', () => {
            this.modalInput.value = '';
            this.clearBtn.classList.add('hidden');
            this.modalInput.focus();
            this.showEmpty();
        });

        let searchTimeout;
        this.modalInput.addEventListener('input', (e) => {
            this.clearBtn.classList.toggle('hidden', !e.target.value);
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => this.performSearch(e.target.value), 250);
        });

        // Global shortcuts + keyboard navigation.
        document.addEventListener('keydown', (e) => {
            const open = !this.backdrop.classList.contains('hidden');
            if (!open) {
                if ((e.metaKey || e.ctrlKey) && e.key === 'k') { e.preventDefault(); this.openModal(); }
                return;
            }
            if (e.key === 'Escape') { this.closeModal(); return; }
            if ((e.metaKey || e.ctrlKey) && e.key === 'k') { e.preventDefault(); return; }

            const rows = this.resultsBody.querySelectorAll('.search-result');
            if (e.key === 'ArrowDown') {
                e.preventDefault();
                if (rows.length) { this.selected = (this.selected + 1) % rows.length; this.applySelection(); }
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                if (rows.length) { this.selected = (this.selected - 1 + rows.length) % rows.length; this.applySelection(); }
            } else if (e.key === 'Enter') {
                const row = rows[this.selected];
                if (row) this.openResult(row, e.metaKey || e.ctrlKey);
            }
        });

        // Result rows, recents, and state-panel actions.
        this.resultsBody.addEventListener('click', (e) => {
            const recent = e.target.closest('[data-recent]');
            if (recent) { this.modalInput.value = recent.dataset.recent; this.clearBtn.classList.remove('hidden'); this.performSearch(recent.dataset.recent); return; }
            if (e.target.closest('[data-clear-recents]')) { localStorage.removeItem('pnx-search-recents'); this.showEmpty(); return; }
            if (e.target.closest('[data-retry]')) { this.engineFailed = false; this.engine = null; this.openModal(); return; }
            const row = e.target.closest('.search-result');
            if (row) this.openResult(row, e.metaKey || e.ctrlKey);
        });
    }

    async openModal() {
        this.backdrop.classList.remove('hidden');
        document.body.style.overflow = 'hidden';
        this.modalInput.focus();
        this.modalInput.select();

        if (this.engineFailed) { this.showError(); return; }

        // If the active locale changed since the engine loaded (SPA nav across
        // locales), discard it so we re-fetch the correct locale's index.
        const activeLocale = this.getIndexLocale();
        if (this.engine && this.loadedLocale && this.loadedLocale !== activeLocale) {
            this.engine = null;
            this.loadedLocale = null;
            this.lastRanked = [];
        }

        if (!this.engine) {
            this.showLoading();
            try {
                await this.ensureSearchEngineScript();
                const engine = new DeweySearchEngine(`${this.baseUrl()}/search/${activeLocale}`);
                await engine.loadManifest();
                this.engine = engine;
                this.loadedLocale = activeLocale;
            } catch (error) {
                console.error('Failed to load search index:', error);
                this.engineFailed = true;
                this.showError();
                return;
            }
        }

        if (this.modalInput.value.trim()) this.performSearch(this.modalInput.value);
        else this.showEmpty();
    }

    closeModal() {
        this.backdrop.classList.add('hidden');
        document.body.style.overflow = '';
    }

    baseUrl() {
        let b = document.body.getAttribute('data-base-url') || '';
        if (b.endsWith('/')) b = b.slice(0, -1);
        return b;
    }

    // Pull in DeweySearch.Web's browser client (dewey-search.js) on first open,
    // rather than via a <script> tag every host must remember to add. The
    // dependency rides with the search UI: a host with no #search-input never
    // constructs this manager, so the client is never fetched. A host that still
    // loads the script itself is honored by the already-defined guard — a
    // deferred manual <script> has executed by the time the user opens search.
    // The URL is base-url prefixed (from data-base-url) because, unlike the
    // server-rendered <script src> tags, a runtime-injected element does not pass
    // through BaseUrlHtmlRewriter.
    ensureSearchEngineScript() {
        if (typeof DeweySearchEngine !== 'undefined') return Promise.resolve();
        if (this._scriptPromise) return this._scriptPromise;

        this._scriptPromise = new Promise((resolve, reject) => {
            const script = document.createElement('script');
            script.src = `${this.baseUrl()}/_content/DeweySearch.Web/dewey-search.js`;
            script.addEventListener('load', () => resolve(), { once: true });
            script.addEventListener('error', () => {
                // Drop the cache and the dead element so Retry re-attempts the load.
                this._scriptPromise = null;
                script.remove();
                reject(new Error('Failed to load dewey-search.js'));
            }, { once: true });
            document.head.appendChild(script);
        });
        return this._scriptPromise;
    }

    async performSearch(query) {
        const trimmed = query.trim();
        this.lastQuery = trimmed;
        const qid = ++this.queryId;

        if (!trimmed) { this.lastRanked = []; this.showEmpty(); return; }
        if (this.engineFailed) { this.showError(); return; }
        if (!this.engine) { this.showLoading(); return; }

        try {
            const ranked = await this.engine.search(trimmed);
            if (qid !== this.queryId) return; // a newer query started
            this.lastRanked = ranked;
            await this.renderResults(qid);
        } catch (error) {
            console.error('Search error:', error);
            if (qid === this.queryId) this.showError();
        }
    }

    // ----- area helpers -----
    areaOf(docId) {
        const doc = this.engine.docEntry(docId);
        const facets = this.engine.availableFacets();
        const ids = doc && doc.f && doc.f.area;
        if (!ids || !ids.length || !facets.area) return '';
        return facets.area[ids[0]] || '';
    }

    areaLabel(area) {
        return area ? area.charAt(0).toUpperCase() + area.slice(1) : '';
    }

    areaPill(area) {
        if (!area) return '';
        return `<span class="search-area-pill" data-area="${area}"><span class="search-dot search-pill-dot" data-area="${area}"></span>${this.areaLabel(area)}</span>`;
    }

    // ----- results -----
    async renderResults(qid) {
        const ranked = this.lastRanked;
        if (ranked.length === 0) { this.showNoResults(); return; }

        // Map docId -> the field flags the query matched in (the engine surfaces these), so a row
        // whose heading/title already matched can skip the redundant body snippet (DocSearch style).
        this._fieldsById = new Map(ranked.map(r => [r.docId, r.fields | 0]));

        // Group by page (DocSearch-style): a page-title header carrying the area pill, with that
        // page's matching headings listed beneath. The page name then reads once per group rather
        // than on every row. Capped to a record budget so a broad query can't render hundreds.
        const groups = this.groupByPage(ranked.slice(0, this.MAX_RESULTS));

        // Show only the top few heading hits per page — beyond that the user is better off
        // narrowing the query than scrolling one page's sections.
        for (const g of groups) g.items = g.items.slice(0, this.MAX_PER_PAGE);

        // Fetch fragments only for rows that will show a snippet (body-only matches); heading/title
        // matches need none, and the page-lead head never shows one.
        const ids = groups.flatMap(g => g.items.filter(id => this.showsSnippet(id)));
        const frags = await this.fetchFragments(ids);
        if (qid !== this.queryId) return;

        this.resultsBody.innerHTML = groups.map(g => this.groupHtml(g, frags)).join('');
        this.selected = 0;
        this.applySelection();
    }

    // Bucket ranked results by their page (the URL before any #anchor), keeping pages in
    // first-seen (rank) order. Each group tracks the page title, area, page URL, the page-lead
    // record (if it matched), and the matching heading records.
    groupByPage(ranked) {
        const order = [];
        const byPage = new Map();
        for (const r of ranked) {
            const doc = this.engine.docEntry(r.docId) || {};
            const url = doc.u || '';
            const pageUrl = url.split('#')[0];
            let g = byPage.get(pageUrl);
            if (!g) {
                g = { pageUrl, title: '', area: this.areaOf(r.docId), leadId: null, items: [] };
                byPage.set(pageUrl, g);
                order.push(pageUrl);
            }
            if (url.includes('#')) {
                g.items.push(r.docId);
                if (!g.title) g.title = (doc.c && doc.c[0]) || doc.t || '';
            } else {
                g.leadId = r.docId;
                g.title = doc.t || g.title;
            }
        }
        return order.map(p => byPage.get(p));
    }

    async fetchFragments(ids) {
        const uniq = [...new Set(ids)];
        const frs = await Promise.all(uniq.map(id => this.engine.loadFragment(id)));
        const map = {};
        uniq.forEach((id, i) => { map[id] = frs[i]; });
        return map;
    }

    // One page group: a clickable header (page title + area pill, → page top) followed by the
    // page's matching heading rows. A lead-only match renders as just the header.
    groupHtml(group, frags) {
        // Namespaced reference/API entries render as code (monospaced) so types read at a glance.
        const titleCls = group.area === 'reference' ? 'search-page-group-title is-mono' : 'search-page-group-title';
        const head = `<div class="search-result search-page-group-head" data-url="${this.escapeAttr(this.baseUrl() + group.pageUrl)}">
            <span class="${titleCls}">${this.highlightEsc(group.title)}</span>
            ${this.areaPill(group.area)}
        </div>`;
        const rows = group.items.map(id => this.subRowHtml(id, frags[id])).join('');
        return `<div class="search-page-group">${head}${rows}</div>`;
    }

    // One heading row under a page header: an optional within-page trail (ancestor headings only —
    // the page title is already the header), the heading, and a snippet. No leading icon.
    subRowHtml(docId, frag) {
        const doc = this.engine.docEntry(docId) || {};
        const within = (doc.c || []).slice(1);
        const breadcrumb = within.length
            ? `<div class="search-result-breadcrumb">${within.map(c => `<span>${this.highlightEsc(c)}</span>`).join('<span class="sep">›</span>')}</div>`
            : '';
        // Heading/title matches are self-explanatory — no snippet. Body-only matches show an excerpt.
        const snippet = this.showsSnippet(docId) ? this.getContentSnippet((frag && frag.body) || '', this.lastQuery) : '';
        const snippetHtml = snippet ? `<div class="search-result-snippet">${snippet}</div>` : '';
        return `<div class="search-result search-result-sub" data-url="${this.escapeAttr(this.baseUrl() + (doc.u || ''))}">
            ${breadcrumb}
            <div class="search-result-heading">${this.highlightEsc(doc.t || '')}</div>
            ${snippetHtml}
        </div>`;
    }

    // A sub-row shows a body snippet only when the match did NOT land in the heading/title — a
    // heading hit is self-explanatory (tailwind/DocSearch heading-vs-content behavior). The engine
    // reports matched fields per result; fall back to title/heading bits if an older client lacks them.
    showsSnippet(docId) {
        const FF = (typeof DeweySearchEngine !== 'undefined' && DeweySearchEngine.FieldFlags) || { Title: 1, Heading: 2 };
        const fields = (this._fieldsById && this._fieldsById.get(docId)) || 0;
        return (fields & (FF.Title | FF.Heading)) === 0;
    }

    applySelection() {
        const rows = this.resultsBody.querySelectorAll('.search-result');
        rows.forEach((r, i) => {
            if (i === this.selected) r.setAttribute('data-selected', 'true');
            else r.removeAttribute('data-selected');
        });
        rows[this.selected]?.scrollIntoView({ block: 'nearest' });
    }

    openResult(row, newTab) {
        const url = row.getAttribute('data-url');
        if (!url) return;
        this.saveRecent();
        if (newTab) { window.open(url, '_blank', 'noopener'); return; }
        this.closeModal();
        window.location.href = url;
    }

    // ----- states -----
    showLoading() {
        const row = `<div class="search-skel-row"><div class="search-skel" style="height:14px"></div><div><div class="search-skel" style="height:10px;width:55%;margin-bottom:8px"></div><div class="search-skel" style="height:14px;width:70%;margin-bottom:8px"></div><div class="search-skel" style="height:10px;width:92%"></div></div></div>`;
        this.resultsBody.innerHTML = row.repeat(4);
    }

    showError() {
        this.resultsBody.innerHTML = `<div class="search-state">
            <div class="search-state-icon search-state-icon-warn">${this.svg('warn', 'w-6 h-6')}</div>
            <div class="search-state-title">Search is temporarily unavailable</div>
            <div class="search-state-sub">The search index didn't load. This usually clears up on its own — try again in a moment.</div>
            <div class="search-state-actions"><button class="search-nr-btn search-nr-btn-primary" data-retry>Retry</button></div></div>`;
    }

    showNoResults() {
        const q = this.escapeHtml('"' + this.lastQuery + '"');
        this.resultsBody.innerHTML = `<div class="search-state">
            <div class="search-state-icon">${this.svg('nores', 'w-6 h-6')}</div>
            <div class="search-state-title">No matches for <span class="font-mono">${q}</span></div>
            <div class="search-state-sub">We couldn't find any heading or page mentioning that. Try a broader term.</div></div>`;
    }

    showEmpty() {
        const recents = this.getRecents();
        if (!recents.length) {
            this.resultsBody.innerHTML = `<div class="search-state"><div class="search-state-sub">Start typing to search the docs.</div></div>`;
            return;
        }
        const rows = recents.map(x => `<div class="search-recent-row" data-recent="${this.escapeAttr(x.q)}">
            ${this.svg('clock', 'search-recent-icon')}
            <div><div class="search-recent-title">${this.escapeHtml(x.q)}</div><div class="search-recent-sub">${this.ago(x.ts)} · ${x.count} ${x.count === 1 ? 'result' : 'results'}</div></div>
            ${this.svg('chevron', 'search-recent-chevron')}</div>`).join('');
        this.resultsBody.innerHTML = `<section class="search-empty-section">
            <div class="search-empty-head"><span>Recent searches</span><span class="search-empty-clear" data-clear-recents>Clear all</span></div>
            ${rows}</section>`;
    }

    // ----- recent searches (localStorage) -----
    getRecents() {
        try { return JSON.parse(localStorage.getItem('pnx-search-recents') || '[]'); }
        catch { return []; }
    }

    saveRecent() {
        const q = this.lastQuery.trim();
        if (!q) return;
        let r = this.getRecents().filter(x => x.q !== q);
        r.unshift({ q, ts: Date.now(), count: this.lastRanked.length });
        localStorage.setItem('pnx-search-recents', JSON.stringify(r.slice(0, 5)));
    }

    ago(ts) {
        const d = Math.floor((Date.now() - ts) / 86400000);
        if (d <= 0) return 'today';
        if (d === 1) return 'yesterday';
        if (d < 7) return `${d} days ago`;
        const w = Math.floor(d / 7);
        return w === 1 ? 'last week' : `${w} weeks ago`;
    }

    escapeAttr(s) {
        return String(s).replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;');
    }

    highlightEsc(text) {
        return this.highlightText(this.escapeHtml(text), this.lastQuery);
    }

    escapeHtml(value) {
        return String(value).replace(/[&<>"']/g, c => ({
            '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
        }[c]));
    }

    /**
     * Resolve the locale code that should be used to fetch the search index for
     * the current page. Always returns a non-empty code (falls back to
     * data-default-locale, then "en") so the per-locale URL always resolves.
     */
    getIndexLocale() {
        const locales = (document.body.getAttribute('data-locales') || '').split(',').filter(Boolean);
        const defaultLocale = document.body.getAttribute('data-default-locale') || 'en';

        // Single-locale sites emit an empty data-locales; just use the default.
        if (locales.length <= 1) return defaultLocale;

        const path = window.location.pathname.replace(/^\//, '');
        const firstSegment = path.split('/')[0];

        if (firstSegment && locales.includes(firstSegment) && firstSegment !== defaultLocale) {
            return firstSegment;
        }
        return defaultLocale;
    }

    highlightText(text, query) {
        if (!text || !query) return text;

        const words = query.toLowerCase().split(/\s+/);
        let highlightedText = text;

        words.forEach(word => {
            if (word.length > 2) {
                const regex = new RegExp(`(${word.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi');
                highlightedText = highlightedText.replace(regex, '<mark class="search-highlight">$1</mark>');
            }
        });

        return highlightedText;
    }

    getContentSnippet(content, query) {
        if (!content || !query) return '';

        const words = query.toLowerCase().split(/\s+/);
        const contentLower = content.toLowerCase();

        // Find first occurrence of any search term
        let firstIndex = -1;
        for (const word of words) {
            if (word.length > 2) {
                const index = contentLower.indexOf(word);
                if (index !== -1 && (firstIndex === -1 || index < firstIndex)) {
                    firstIndex = index;
                }
            }
        }

        if (firstIndex === -1) {
            return this.escapeHtml(content.substring(0, 150)) + (content.length > 150 ? '…' : '');
        }

        // Get snippet around the found term
        const start = Math.max(0, firstIndex - 75);
        const end = Math.min(content.length, firstIndex + 75);
        let snippet = content.substring(start, end);

        if (start > 0) snippet = '…' + snippet;
        if (end < content.length) snippet = snippet + '…';

        // Escape before highlighting so plain-text body can't inject markup.
        return this.highlightText(this.escapeHtml(snippet), query);
    }
}

/**
 * Syntax Highlighter - Handles code syntax highlighting with highlight.js
 */
class SyntaxHighlighter {
    constructor() {
        this.prefix = 'language-';
        this.hljs = null;
    }

    async init() {
        const codeNodes = this.getRelevantCodeNodes();
        if (codeNodes.length === 0) return;

        try {
            await this.setupHighlightJs();
            this.highlightCodeNodes(codeNodes);
        } catch (error) {
            console.error('Failed to initialize syntax highlighting:', error);
        }
    }

    getRelevantCodeNodes() {
        const codeNodes = Array.from(document.body.querySelectorAll('code'));
        return codeNodes.filter(node => {
            const hasLanguageClass = Array.from(node.classList).some(cls =>
                cls.startsWith(this.prefix) && cls !== this.prefix + 'mermaid' && cls !== this.prefix + 'text' && cls !== this.prefix
            );
            if (!hasLanguageClass) return false;

            // Skip blocks already highlighted server-side by TextMate
            if (node.querySelector('span[class^="hljs-"]')) return false;

            return true;
        });
    }

    async setupHighlightJs() {
        // Load highlight.js from CDN
        this.hljs = await import('https://esm.sh/highlight.js@11/lib/core');
        
        // Configure highlight.js
        this.hljs.default.configure({
            ignoreUnescapedHTML: true,
            throwUnescapedHTML: false
        });

        // Load common languages
        const languages = [
            'javascript', 'typescript', 'python', 'java', 'csharp', 'cpp', 'c',
            'css', 'xml', 'json', 'yaml', 'bash', 'shell', 'sql',
            'php', 'ruby', 'go', 'rust', 'kotlin', 'swift', 'markdown'
        ];

        for (const lang of languages) {
            try {
                const langModule = await import(`https://esm.sh/highlight.js@11/lib/languages/${lang}`);
                this.hljs.default.registerLanguage(lang, langModule.default);
            } catch (err) {
                // Language not available, skip silently
            }
        }
    }

    highlightCodeNodes(codeNodes) {
        for (const node of codeNodes) {
            try {
                this.highlightSingleNode(node);
            } catch (error) {
                console.error(`Failed to highlight code node:`, error);
            }
        }
    }

    highlightSingleNode(node) {
        const className = Array.from(node.classList)
            .find(cls => cls.startsWith(this.prefix));

        if (!className) return;

        const language = className.slice(this.prefix.length);
        
        // Map some common language aliases
        const languageMap = {
            'js': 'javascript',
            'ts': 'typescript',
            'cs': 'csharp',
            'py': 'python',
            'sh': 'bash',
            'yml': 'yaml'
        };

        const mappedLanguage = languageMap[language] || language;

        try {
            // Check if language is registered
            if (this.hljs.default.getLanguage(mappedLanguage)) {
                const result = this.hljs.default.highlight(node.textContent, { language: mappedLanguage });
                node.innerHTML = result.value;
                node.classList.add('hljs');
            } else {
                // Use auto-detection as fallback
                const result = this.hljs.default.highlightAuto(node.textContent);
                node.innerHTML = result.value;
                node.classList.add('hljs');
            }
        } catch (error) {
            console.warn(`Failed to highlight ${language}:`, error);
        }
    }
}

// Initialize the page manager
const pageManager = new PageManager();

// Make pageManager globally accessible
window.pageManager = pageManager;

// SPA lifecycle integration (no-op if spa-engine.js is not loaded)
document.addEventListener('spa:before-navigate', () => {
    window.pageManager?.onSpaNavigating();
});
document.addEventListener('spa:commit', (e) => {
    window.pageManager?.onSpaCommit(e.detail?.doc);
});

