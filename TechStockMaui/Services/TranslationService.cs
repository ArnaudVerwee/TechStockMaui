using System.Net.Http.Json;
using System.Collections.Concurrent;

namespace TechStockMaui.Services
{
    public class TranslationService
    {
        private static TranslationService _instance;
        private static readonly object _lock = new object();

        private readonly HttpClient _httpClient;

        private static string BaseUrl
        {
            get
            {
#if ANDROID
                return "http://10.0.2.2:7236/api";
#else
                return "https://localhost:7237/api";
#endif
            }
        }

        private readonly ConcurrentDictionary<string, Dictionary<string, string>> _translationsCache;
        private string _currentCulture;
        private readonly List<string> _supportedCultures;

        public event EventHandler<string> CultureChanged;

        public static TranslationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new TranslationService();
                        }
                    }
                }
                return _instance;
            }
        }

        private TranslationService()
        {
            var handler = new HttpClientHandler();

#if ANDROID
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(15)
            };

            _translationsCache = new ConcurrentDictionary<string, Dictionary<string, string>>();
            _supportedCultures = new List<string> { "en", "fr", "nl" };
            _currentCulture = Preferences.Get("app_language", "en");

            System.Diagnostics.Debug.WriteLine($"TranslationService singleton initialized - Culture: {_currentCulture}");
            System.Diagnostics.Debug.WriteLine($"TranslationService using: {BaseUrl}");
        }

        private async Task ConfigureAuthAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auth configuration error: {ex.Message}");
            }
        }

        public async Task LoadTranslationsAsync(string culture)
        {
            try
            {
                if (_translationsCache.ContainsKey(culture))
                {
                    System.Diagnostics.Debug.WriteLine($"Translations for {culture} found in cache");
                    return;
                }

                await ConfigureAuthAsync();

                var url = $"{BaseUrl}/Translations?culture={culture}";
                System.Diagnostics.Debug.WriteLine($"Loading translations from: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var translations = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                    if (translations != null && translations.Any())
                    {
                        _translationsCache[culture] = translations;
                        System.Diagnostics.Debug.WriteLine($"{translations.Count} translations loaded for {culture}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"No translations received for {culture}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"API error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error LoadTranslationsAsync: {ex.Message}");
            }
        }

        public async Task<string> GetTranslationAsync(string key, string culture = null)
        {
            try
            {
                culture ??= _currentCulture;

                if (!_translationsCache.ContainsKey(culture))
                {
                    await LoadTranslationsAsync(culture);
                }

                if (_translationsCache.TryGetValue(culture, out var translations) &&
                    translations.TryGetValue(key, out var translation))
                {
                    System.Diagnostics.Debug.WriteLine($"Translation found for key '{key}' in culture '{culture}': {translation}");
                    return translation;
                }

                if (culture != "en" && _translationsCache.TryGetValue("en", out var enTranslations) &&
                    enTranslations.TryGetValue(key, out var enTranslation))
                {
                    System.Diagnostics.Debug.WriteLine($"Fallback to English for key '{key}': {enTranslation}");
                    return enTranslation;
                }

                System.Diagnostics.Debug.WriteLine($"No translation found for key '{key}', returning key as fallback");
                return key;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetTranslationAsync: {ex.Message}");
                return key;
            }
        }

        public async Task SetCurrentCultureAsync(string culture)
        {
            if (_supportedCultures.Contains(culture))
            {
                _currentCulture = culture;
                Preferences.Set("app_language", culture);
                System.Diagnostics.Debug.WriteLine($"Culture changed to: {culture}");

                await LoadTranslationsAsync(culture);

                CultureChanged?.Invoke(this, culture);
                System.Diagnostics.Debug.WriteLine($"Culture change event fired for: {culture}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Unsupported culture: {culture}");
            }
        }

        public string GetCurrentCulture() => _currentCulture;

        public List<string> GetSupportedCultures() => _supportedCultures;

        public string GetLanguageDisplayName(string culture)
        {
            return culture switch
            {
                "fr" => "Français",
                "en" => "English",
                "nl" => "Nederlands",
                _ => culture
            };
        }

        public string GetLanguageFlag(string culture)
        {
            return culture switch
            {
                "fr" => "FR",
                "en" => "EN",
                "nl" => "NL",
                _ => culture.ToUpper()
            };
        }

        public void ClearCache()
        {
            _translationsCache.Clear();
            System.Diagnostics.Debug.WriteLine("Translation cache cleared");
        }
    }
}