// Views/BaseLocalizedPage.cs - Version corrigée

using TechStockMaui.Services;

namespace TechStockMaui.Views
{
    public abstract class BaseLocalizedPage : ContentPage
    {
        public BaseLocalizedPage()
        {
            // ✅ S'abonner aux changements de langue directement
            TranslationService.Instance.CultureChanged += OnCultureChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadTranslationsAsync();
        }

        protected virtual async Task LoadTranslationsAsync()
        {
            try
            {
                var currentCulture = TranslationService.Instance.GetCurrentCulture();
                await TranslationService.Instance.LoadTranslationsAsync(currentCulture);
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur chargement traductions: {ex.Message}");
            }
        }

        protected abstract Task UpdateTextsAsync();

        private async void OnCultureChanged(object sender, string newCulture)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🌍 Page {GetType().Name} - Langue changée vers: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
        }

        protected async Task<string> GetTextAsync(string key, string fallback = null)
        {
            try
            {
                var text = await TranslationService.Instance.GetTranslationAsync(key);
                return !string.IsNullOrEmpty(text) && text != key ? text : (fallback ?? key);
            }
            catch
            {
                return fallback ?? key;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        ~BaseLocalizedPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}