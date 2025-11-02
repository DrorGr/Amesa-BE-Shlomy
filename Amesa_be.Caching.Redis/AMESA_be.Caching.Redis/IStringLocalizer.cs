using AMESA_be.common.Interfaces;
using Microsoft.Extensions.Localization;

namespace AMESA_be.Caching.Redis
{
    public interface IStringLocalizer
    {
        /// <summary>
        /// Translates ITranslatable object.
        /// </summary>
        /// <param name="translatable"></param>
        /// <param name="language">Can be taken from Content-Language automatically</param>
        void TranslateObject(ITranslatable translatable, string? language = null);
        /// <summary>
        /// Gets the string resource with the given name. <b>Language taken from Content-Language</b>
        /// </summary>
        /// <param name="key">The key of the string resource.</param>
        /// <returns>The string resource as a <see cref="LocalizedString"/>.</returns>
        LocalizedString this[string key] { get; }
        /// <summary>
        /// Gets the string resource with the given name and language.
        /// </summary>
        /// <param name="key">The key of the string resource.</param>
        /// <param name="language">The language to translate given key.</param>
        /// <returns>The formatted string resource as a <see cref="LocalizedString"/>.</returns>
        LocalizedString this[string key, string language] { get; }
    }
}
