using AMESA_be.common.Extensions;
using AMESA_be.common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace AMESA_be.Caching.Redis
{
    public class RedisStringLocalizer : IStringLocalizer
    {
        private readonly ICache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RedisStringLocalizer(ICache cache, IHttpContextAccessor httpContextAccessor)
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        public LocalizedString this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                {
                    return new LocalizedString(string.Empty, string.Empty);
                }

                if (key.Split(':').Count() < 3)
                {
                    key = AddLanguage(key);
                }

                var result = _cache.GetRecord<string>(key, isGlobal: true);
                if (string.IsNullOrEmpty(result))
                {
                    key = $"languages:default:{key.Split(':')[2]}";
                    result = _cache.GetRecord<string>(key, isGlobal: true);
                }

                if (string.IsNullOrEmpty(result))
                {
                    return new LocalizedString(key, key.Split(':')[2]);
                }

                return new LocalizedString(key, result);
            }
        }

        public LocalizedString this[string key, string language] => this[$"languages:{language}:{key}"];

        public void TranslateObject(ITranslatable translatable, string? language = null)
        {
            if (translatable == null) return;
            if (string.IsNullOrEmpty(language))
            {
                language = GetLanguage();
            }

            translatable.DisplayName = this[translatable.DisplayNameKey];
            translatable.Description = this[translatable.DescriptionKey];

        }

        private string AddLanguage(string key)
        {
            return $"languages:{GetLanguage()}:{key}";
        }

        private string GetLanguage()
        {
            return _httpContextAccessor.GetUserLanguageId().ToString();
        }
    }
}
