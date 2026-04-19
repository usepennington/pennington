namespace Pennington.Localization;

using Microsoft.Extensions.Localization;
using LocalizationOptions = Infrastructure.LocalizationOptions;

/// <summary>
/// An <see cref="IStringLocalizerFactory"/> that returns <see cref="PenningtonStringLocalizer"/>
/// instances backed by <see cref="TranslationOptions"/>. All localizer instances share
/// the same translation dictionary — the type/location parameters are ignored.
/// </summary>
public sealed class PenningtonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly PenningtonStringLocalizer _localizer;

    /// <summary>Creates the factory with the shared localizer instance.</summary>
    public PenningtonStringLocalizerFactory(TranslationOptions translations, LocalizationOptions localization)
    {
        _localizer = new PenningtonStringLocalizer(translations, localization);
    }

    /// <summary>Returns the shared localizer; <paramref name="resourceSource"/> is ignored.</summary>
    public IStringLocalizer Create(Type resourceSource) => _localizer;

    /// <summary>Returns the shared localizer; <paramref name="baseName"/> and <paramref name="location"/> are ignored.</summary>
    public IStringLocalizer Create(string baseName, string location) => _localizer;
}