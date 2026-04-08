/**
 * FAQ accordion behavior for 937 Yoga
 */
(function () {
    'use strict';

    document.addEventListener('click', function (e) {
        var toggle = e.target.closest('[data-faq-toggle]');
        if (!toggle) return;

        var item = toggle.closest('[data-faq-item]');
        if (!item) return;

        var content = item.querySelector('[data-faq-content]');
        var icon = item.querySelector('[data-faq-icon]');
        if (!content) return;

        var isOpen = !content.classList.contains('hidden');

        if (isOpen) {
            content.classList.add('hidden');
            if (icon) icon.style.transform = '';
        } else {
            content.classList.remove('hidden');
            if (icon) icon.style.transform = 'rotate(180deg)';
        }
    });
})();
