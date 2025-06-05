// Services/TranslationService.cs - Version globale singleton

using System.Net.Http.Json;
using System.Collections.Concurrent;

namespace TechStockMaui.Services
{
    public class TranslationService
    {
        private static TranslationService _instance;
        private static readonly object _lock = new object();

        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7237/api";

        // ✅ Variables manquantes ajoutées
        private readonly ConcurrentDictionary<string, Dictionary<string, string>> _translationsCache;
        private string _currentCulture;
        private readonly List<string> _supportedCultures;

        // ✅ Événement pour notifier les pages du changement de langue
        public event EventHandler<string> CultureChanged;

        // ✅ Singleton pattern pour partager entre toutes les pages
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
            _httpClient = new HttpClient();
            _translationsCache = new ConcurrentDictionary<string, Dictionary<string, string>>();
            _supportedCultures = new List<string> { "en","fr" , "nl" };
            _currentCulture = Preferences.Get("app_language", "en");

            System.Diagnostics.Debug.WriteLine($"🌍 TranslationService singleton initialisé - Culture: {_currentCulture}");
        }

        // Configurer l'authentification
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
                System.Diagnostics.Debug.WriteLine($"⚠️ Erreur config auth: {ex.Message}");
            }
        }

        // Charger toutes les traductions pour une culture
        public async Task LoadTranslationsAsync(string culture)
        {
            try
            {
                // Vérifier le cache d'abord
                if (_translationsCache.ContainsKey(culture))
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Traductions {culture} en cache");
                    return;
                }

                await ConfigureAuthAsync();

                // TODO: Adapter cette URL selon votre API existante
                var url = $"{BaseUrl}/Translations?culture={culture}";
                System.Diagnostics.Debug.WriteLine($"🌐 Chargement traductions: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var translations = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

                    if (translations != null && translations.Any())
                    {
                        _translationsCache[culture] = translations;
                        System.Diagnostics.Debug.WriteLine($"✅ {translations.Count} traductions chargées pour {culture}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Erreur API: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur LoadTranslationsAsync: {ex.Message}");
            }
        }

        // Récupérer une traduction
        public async Task<string> GetTranslationAsync(string key, string culture = null)
        {
            try
            {
                culture ??= _currentCulture;

                // Charger si pas en cache
                if (!_translationsCache.ContainsKey(culture))
                {
                    await LoadTranslationsAsync(culture);
                }

                // Récupérer la traduction
                if (_translationsCache.TryGetValue(culture, out var translations) &&
                    translations.TryGetValue(key, out var translation))
                {
                    return translation;
                }

                // Fallback vers français
                if (culture != "en" && _translationsCache.TryGetValue("en", out var frTranslations) &&
                    frTranslations.TryGetValue(key, out var enTranslation))
                {
                    return enTranslation;
                }

                // Retourner la clé si pas trouvé
                return key;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur GetTranslationAsync: {ex.Message}");
                return key;
            }
        }

        // Changer la culture
        public async Task SetCurrentCultureAsync(string culture)
        {
            if (_supportedCultures.Contains(culture))
            {
                _currentCulture = culture;
                Preferences.Set("app_language", culture);
                System.Diagnostics.Debug.WriteLine($"🌍 Culture changée vers: {culture}");

                // ✅ Précharger les traductions
                await LoadTranslationsAsync(culture);

                // ✅ Notifier toutes les pages du changement
                CultureChanged?.Invoke(this, culture);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Culture non supportée: {culture}");
            }
        }

        public string GetCurrentCulture() => _currentCulture;

        public List<string> GetSupportedCultures() => _supportedCultures;

        // ✅ Méthodes helper pour l'UI
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

        // ✅ Vider le cache si nécessaire
        public void ClearCache()
        {
            _translationsCache.Clear();
            System.Diagnostics.Debug.WriteLine("🗑️ Cache traductions vidé");
        }
    }
}