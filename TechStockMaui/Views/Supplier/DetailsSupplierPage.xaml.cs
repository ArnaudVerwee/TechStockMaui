using TechStockMaui.Services;
using TechStockMaui.Models.Supplier;

namespace TechStockMaui.Views.Supplier
{
    public partial class DetailsSupplierPage : ContentPage
    {
        private SupplierService _supplierService;
        private TechStockMaui.Models.Supplier.Supplier _supplier;

        public DetailsSupplierPage(TechStockMaui.Models.Supplier.Supplier supplier)
        {
            InitializeComponent();
            _supplierService = new SupplierService();
            _supplier = supplier;

            // ✅ AJOUT: S'abonner aux changements de langue
            TranslationService.Instance.CultureChanged += OnCultureChanged;

            // Charger les données du fournisseur
            LoadSupplierDetails();
        }

        // ✅ AJOUT: Charger les traductions au démarrage
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadTranslationsAsync();
            // Recharger les données du fournisseur depuis l'API au cas où elles auraient été modifiées
            await RefreshSupplierDetails();
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
                System.Diagnostics.Debug.WriteLine("🌍 Mise à jour des textes DetailsSupplier");

                // ✅ Titre de la page (utilise les clés exactes de vos fichiers .resx)
                var detailsText = await GetTextAsync("Details", "Details");
                var supplierText = await GetTextAsync("Supplier", "Supplier");
                Title = $"{detailsText} {supplierText}";

                // ✅ Titre principal
                if (TitleLabel != null)
                    TitleLabel.Text = $"{detailsText} {supplierText}";

                // ✅ Label nom
                if (NameHeaderLabel != null)
                    NameHeaderLabel.Text = await GetTextAsync("Name", "Name");

                // ✅ Boutons
                if (EditButton != null)
                    EditButton.Text = await GetTextAsync("Edit", "Edit");

                if (BackToListButton != null)
                    BackToListButton.Text = await GetTextAsync("Back to List", "Back to List");

                // ✅ Sélecteur de langue
                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                // ✅ Mettre à jour l'indicateur de langue
                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("✅ Textes DetailsSupplier mis à jour");
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
                System.Diagnostics.Debug.WriteLine($"🌍 DetailsSupplier - Langue changée vers: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private void LoadSupplierDetails()
        {
            if (_supplier != null)
            {
                NameLabel.Text = _supplier.Name ?? "Non défini";
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private async Task RefreshSupplierDetails()
        {
            try
            {
                var updatedSupplier = await _supplierService.GetSupplierByIdAsync(_supplier.Id);
                if (updatedSupplier != null)
                {
                    _supplier = updatedSupplier;
                    LoadSupplierDetails();
                }
            }
            catch (Exception ex)
            {
                // ✅ Message d'erreur traduit (avec fallback appropriés)
                var errorTitle = await GetTextAsync("Error", "Error");
                var errorMessage = await GetTextAsync("Unable to reload details", "Unable to reload details");
                await DisplayAlert(errorTitle, $"{errorMessage}: {ex.Message}", "OK");
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec messages traduits
        private async void OnEditClicked(object sender, EventArgs e)
        {
            try
            {
                // Naviguer vers la page de modification en passant le fournisseur
                await Navigation.PushAsync(new EditSupplierPage(_supplier));
            }
            catch (Exception ex)
            {
                // ✅ Messages traduits (avec fallback en anglais)
                var errorTitle = await GetTextAsync("Error", "Error");
                var errorMessage = await GetTextAsync("Unable to open edit page", "Unable to open edit page");
                await DisplayAlert(errorTitle, $"{errorMessage}: {ex.Message}", "OK");
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private async void OnBackToListClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
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
        ~DetailsSupplierPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}