using Microsoft.Maui.Controls;
using TechStockMaui.Services;

namespace TechStockMaui.Views
{
    public partial class DeleteProductPage : ContentPage
    {
        public DeleteProductPage()
        {
            InitializeComponent();

            // ✅ AJOUT: S'abonner aux changements de langue
            TranslationService.Instance.CultureChanged += OnCultureChanged;
        }

        // ✅ AJOUT: Charger les traductions au démarrage
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadTranslationsAsync();
        }

        // ✅ AJOUT: Charger les traductions
        private async Task LoadTranslationsAsync()
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

        // ✅ AJOUT: Helper pour récupérer une traduction
        private async Task<string> GetTextAsync(string key, string fallback = null)
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

        // ✅ AJOUT: Mettre à jour tous les textes de l'interface
        private async Task UpdateTextsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🌍 Mise à jour des textes DeleteProduct");

                // ✅ Titre de la page
                Title = await GetTextAsync("Delete", "Delete");

                // ✅ Labels principaux
                if (TitleLabel != null)
                    TitleLabel.Text = await GetTextAsync("Delete", "Delete product");

                if (ConfirmationLabel != null)
                    ConfirmationLabel.Text = await GetTextAsync("Are you sure you want to delete this?", "Are you sure you want to delete this?");

                // ✅ Labels des champs
                if (NameFieldLabel != null)
                    NameFieldLabel.Text = await GetTextAsync("Name", "Name") + ":";

                if (SerialFieldLabel != null)
                    SerialFieldLabel.Text = await GetTextAsync("Serial Number", "Serial Number") + ":";

                if (TypeFieldLabel != null)
                    TypeFieldLabel.Text = await GetTextAsync("Type", "Type") + ":";

                if (SupplierFieldLabel != null)
                    SupplierFieldLabel.Text = await GetTextAsync("Supplier", "Supplier") + ":";

                // ✅ Boutons
                if (DeleteButton != null)
                    DeleteButton.Text = await GetTextAsync("Delete", "Delete");

                if (BackButton != null)
                    BackButton.Text = await GetTextAsync("Back to List", "Back to List");

                // ✅ Sélecteur de langue
                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                // ✅ Mettre à jour l'indicateur de langue
                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("✅ Textes DeleteProduct mis à jour");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur UpdateTextsAsync: {ex.Message}");
            }
        }

        // ✅ AJOUT: Callback quand la langue change
        private async void OnCultureChanged(object sender, string newCulture)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🌍 DeleteProduct - Langue changée vers: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            // ✅ Messages traduits
            var deletedTitle = await GetTextAsync("Deleted", "Deleted");
            var productDeletedMsg = await GetTextAsync("ProductDeleted", "Product has been deleted");

            // Ici, tu peux ajouter la logique pour supprimer le produit
            await DisplayAlert(deletedTitle, productDeletedMsg, "OK");
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private void OnBackToListClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        // ✅ AJOUT: Gestion du changement de langue
        private async void OnLanguageClicked(object sender, EventArgs e)
        {
            try
            {
                var translationService = TranslationService.Instance;
                var currentCulture = translationService.GetCurrentCulture();

                var options = new List<string>();
                foreach (var culture in translationService.GetSupportedCultures())
                {
                    var flag = translationService.GetLanguageFlag(culture);
                    var name = translationService.GetLanguageDisplayName(culture);
                    var current = culture == currentCulture ? " ✓" : "";
                    options.Add($"{flag} {name}{current}");
                }

                var cancelText = await GetTextAsync("Cancel", "Cancel");
                var titleText = "🌍 " + await GetTextAsync("ChooseLanguage", "Choose language");

                var selectedOption = await DisplayActionSheet(titleText, cancelText, null, options.ToArray());

                if (!string.IsNullOrEmpty(selectedOption) && selectedOption != cancelText)
                {
                    string newCulture = null;
                    if (selectedOption.Contains("EN")) newCulture = "en";
                    else if (selectedOption.Contains("FR")) newCulture = "fr";
                    else if (selectedOption.Contains("NL")) newCulture = "nl";

                    if (newCulture != null && newCulture != currentCulture)
                    {
                        System.Diagnostics.Debug.WriteLine($"🌍 Changement vers: {newCulture}");
                        await translationService.SetCurrentCultureAsync(newCulture);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
        }

        // ✅ AJOUT: Mettre à jour le drapeau de langue
        private async Task UpdateLanguageFlag()
        {
            try
            {
                var translationService = TranslationService.Instance;
                var currentCulture = translationService.GetCurrentCulture();
                var flag = translationService.GetLanguageFlag(currentCulture);

                if (LanguageFlag != null)
                    LanguageFlag.Text = flag;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur mise à jour drapeau: {ex.Message}");
            }
        }

        // ✅ AJOUT: Nettoyage
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        // ✅ AJOUT: Destructeur
        ~DeleteProductPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}