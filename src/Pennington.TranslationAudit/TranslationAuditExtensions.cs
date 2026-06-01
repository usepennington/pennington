namespace Pennington.TranslationAudit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pennington.Generation;

/// <summary>DI extensions for registering the translation auditor.</summary>
public static class TranslationAuditExtensions
{
    /// <summary>
    /// Register <see cref="TranslationAuditor"/> as an <see cref="IBuildAuditor"/>. Diagnostics
    /// land in the dev overlay (per-page) and in the build report (site-wide) automatically.
    /// </summary>
    public static IServiceCollection AddTranslationAudit(this IServiceCollection services, Action<TranslationAuditOptions>? configure = null)
    {
        var options = new TranslationAuditOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddSingleton<IGitHistoryReader>(sp =>
            new LibGit2GitHistoryReader(
                options.RepositoryPath,
                sp.GetRequiredService<ILogger<LibGit2GitHistoryReader>>()));

        services.AddTransient<IBuildAuditor, TranslationAuditor>();

        return services;
    }
}