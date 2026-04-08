namespace Penn.Localization;

using Microsoft.Extensions.Localization;
using LocalizationOptions = Penn.Infrastructure.LocalizationOptions;

/// <summary>
/// An <see cref="IStringLocalizerFactory"/> that returns <see cref="PennStringLocalizer"/>
/// instances backed by <see cref="TranslationOptions"/>. All localizer instances share
/// the same translation dictionary — the type/location parameters are ignored.
/// </summary>
public sealed class PennStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly PennStringLocalizer _localizer;

    public PennStringLocalizerFactory(TranslationOptions translations, LocalizationOptions localization)
    {
        _localizer = new PennStringLocalizer(translations, localization);
    }

    public IStringLocalizer Create(Type resourceSource) => _localizer;

    public IStringLocalizer Create(string baseName, string location) => _localizer;
}
