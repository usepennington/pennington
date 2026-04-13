namespace YogaStudioExample.Models;

using System.Collections.Immutable;

public static class YogaComponentApplies
{
    public static ImmutableDictionary<string, string> All()
    {
        return ImmutableDictionary.Create<string, string>()
            .AddRange(new Dictionary<string, string>
            {
                // Hero section
                {
                    ".yoga-hero",
                    "relative overflow-hidden bg-gradient-to-br from-primary-50 to-accent-50 dark:from-base-900 dark:to-base-950"
                },
                {
                    ".yoga-hero-content",
                    "relative z-10 max-w-5xl mx-auto px-6 py-24 md:py-32 lg:py-40 text-center"
                },

                // Cards
                {
                    ".yoga-card",
                    "bg-white rounded-2xl border border-base-200 shadow-sm hover:shadow-md transition-shadow duration-300 overflow-hidden dark:bg-base-900 dark:border-base-700"
                },
                {
                    ".yoga-card-body",
                    "p-6"
                },

                // Schedule
                {
                    ".yoga-schedule-cell",
                    "p-3 rounded-xl border border-base-200 bg-white hover:border-primary-300 transition-colors cursor-pointer dark:bg-base-900 dark:border-base-700 dark:hover:border-primary-600"
                },
                {
                    ".yoga-schedule-time",
                    "text-xs font-medium text-base-500 dark:text-base-400"
                },

                // Tags
                {
                    ".yoga-tag",
                    "inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-primary-100 text-primary-800 dark:bg-primary-900 dark:text-primary-200"
                },
                {
                    ".yoga-tag-accent",
                    "inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-accent-100 text-accent-800 dark:bg-accent-900 dark:text-accent-200"
                },

                // Buttons
                {
                    ".yoga-btn-primary",
                    "inline-flex items-center justify-center px-6 py-3 rounded-full text-sm font-semibold bg-primary-700 text-white hover:bg-primary-800 transition-colors dark:bg-primary-600 dark:hover:bg-primary-500"
                },
                {
                    ".yoga-btn-secondary",
                    "inline-flex items-center justify-center px-6 py-3 rounded-full text-sm font-semibold border-2 border-primary-700 text-primary-700 hover:bg-primary-50 transition-colors dark:border-primary-400 dark:text-primary-400 dark:hover:bg-primary-950"
                },
                {
                    ".yoga-btn-accent",
                    "inline-flex items-center justify-center px-6 py-3 rounded-full text-sm font-semibold bg-accent-600 text-white hover:bg-accent-700 transition-colors dark:bg-accent-500 dark:hover:bg-accent-400"
                },

                // Section layouts
                {
                    ".yoga-section",
                    "py-16 md:py-24 px-6"
                },
                {
                    ".yoga-section-alt",
                    "py-16 md:py-24 px-6 bg-base-100 dark:bg-base-900"
                },
                {
                    ".yoga-container",
                    "max-w-6xl mx-auto"
                },

                // Instructor card
                {
                    ".yoga-instructor-card",
                    "group bg-white rounded-2xl border border-base-200 shadow-sm overflow-hidden hover:shadow-lg transition-all duration-300 dark:bg-base-900 dark:border-base-700"
                },
                {
                    ".yoga-instructor-avatar",
                    "w-full aspect-square bg-gradient-to-br from-primary-200 to-accent-200 flex items-center justify-center text-4xl font-bold text-primary-800 dark:from-primary-800 dark:to-accent-800 dark:text-primary-200"
                },

                // Blog
                {
                    ".yoga-blog-card",
                    "group bg-white rounded-2xl border border-base-200 shadow-sm overflow-hidden hover:shadow-md transition-shadow duration-300 dark:bg-base-900 dark:border-base-700"
                },
                {
                    ".yoga-blog-meta",
                    "flex items-center gap-3 text-sm text-base-500 dark:text-base-400"
                },

                // FAQ
                {
                    ".yoga-faq-item",
                    "border-b border-base-200 dark:border-base-700"
                },
                {
                    ".yoga-faq-question",
                    "flex items-center justify-between w-full py-5 text-left text-base font-medium text-base-900 hover:text-primary-700 transition-colors cursor-pointer dark:text-base-100 dark:hover:text-primary-400"
                },
                {
                    ".yoga-faq-answer",
                    "pb-5 text-base-600 dark:text-base-400 leading-relaxed"
                },

                // Pricing
                {
                    ".yoga-pricing-card",
                    "bg-white rounded-2xl border border-base-200 shadow-sm p-8 flex flex-col dark:bg-base-900 dark:border-base-700"
                },
                {
                    ".yoga-pricing-featured",
                    "bg-white rounded-2xl border-2 border-primary-500 shadow-lg p-8 flex flex-col relative dark:bg-base-900"
                },

                // Nav
                {
                    ".yoga-nav",
                    "sticky top-0 z-40 bg-white/90 backdrop-blur-lg border-b border-base-200 dark:bg-base-950/90 dark:border-base-800"
                },
                {
                    ".yoga-nav-link",
                    "text-sm font-medium text-base-600 hover:text-primary-700 transition-colors dark:text-base-300 dark:hover:text-primary-400"
                },
                {
                    ".yoga-nav-link-active",
                    "text-sm font-semibold text-primary-800 border-b-2 border-primary-600 pb-0.5 dark:text-primary-300 dark:border-primary-400"
                },

                // Testimonials
                {
                    ".yoga-testimonial",
                    "bg-white rounded-2xl border border-base-200 p-8 shadow-sm dark:bg-base-900 dark:border-base-700"
                },

                // Level badges
                {
                    ".yoga-level-beginner",
                    "inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-emerald-100 text-emerald-800 dark:bg-emerald-900 dark:text-emerald-200"
                },
                {
                    ".yoga-level-intermediate",
                    "inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-200"
                },
                {
                    ".yoga-level-advanced",
                    "inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-rose-100 text-rose-800 dark:bg-rose-900 dark:text-rose-200"
                },
                {
                    ".yoga-level-all",
                    "inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-sky-100 text-sky-800 dark:bg-sky-900 dark:text-sky-200"
                },
            });
    }
}