namespace TechStockMaui.Services
{
    public static class ApiConfig
    {
        public static string BaseUrl
        {
            get
            {
#if ANDROID
                // Pour Android, utiliser votre IP locale
                return  "http://10.0.2.2:7236/api/"; ;
#elif WINDOWS
                // Pour Windows, garder localhost
                return "https://localhost:7237/api/";
#else
                // Fallback par défaut
                return "https://localhost:7237/api/";
#endif
            }
        }

        // URLs spécifiques pour chaque service
        public static string AuthUrl => $"{BaseUrl}Auth/";
        public static string TranslationsUrl => $"{BaseUrl}Translations";

        // Méthode pour debug - vous verrez dans les logs quelle URL est utilisée
        public static void LogCurrentConfig()
        {
            System.Diagnostics.Debug.WriteLine($"🌐 Plateforme: {DeviceInfo.Platform}");
            System.Diagnostics.Debug.WriteLine($"🌐 Base URL: {BaseUrl}");
        }
    }
}