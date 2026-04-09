namespace Pennington.Localization;

using Microsoft.Extensions.Localization;
using LocalizationOptions = Pennington.Infrastructure.LocalizationOptions;

/// <summary>
/// An <see cref="IStringLocalizerFactory"/> that returns <see cref="PenningtonStringLocalizer"/>
/// instances backed by <see cref="TranslationOptions"/>. All localizer instances share
/// the same translation dictionary — the type/location parameters are ignored.
/// </summary>
public sealed class PenningtonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly PenningtonStringLocalizer _localizer;

    public PenningtonStringLocalizerFactory(TranslationOptions translations, LocalizationOptions localization)
    {
        _localizer = new PenningtonStringLocalizer(translations, localization);
    }

    public IStringLocalizer Create(Type resourceSource) => _localizer;

    public IStringLocalizer Create(string baseName, string location) => _localizer;
}
