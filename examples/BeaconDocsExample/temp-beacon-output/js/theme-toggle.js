// Theme toggle for Beacon docs - demonstrates ContentPaths scanning
document.addEventListener('DOMContentLoaded', () => {
    const toggle = document.querySelector('[data-theme-toggle]');
    if (toggle) {
        toggle.classList.add('bg-base-100', 'text-base-700', 'border-primary-500');
        toggle.addEventListener('click', () => {
            document.documentElement.classList.toggle('dark');
        });
    }
});
