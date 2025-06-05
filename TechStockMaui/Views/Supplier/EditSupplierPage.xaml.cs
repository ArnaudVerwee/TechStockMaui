using TechStockMaui.Services;
using TechStockMaui.Models.Supplier;

namespace TechStockMaui.Views.Supplier
{
    public partial class EditSupplierPage : ContentPage
    {
        private SupplierService _supplierService;
        private TechStockMaui.Models.Supplier.Supplier _supplier;

        public EditSupplierPage(TechStockMaui.Models.Supplier.Supplier supplier)
        {
            InitializeComponent();
            _supplierService = new SupplierService();
            _supplier = supplier;

            // ✅ AJOUT: S'abonner aux changements de langue
            TranslationService.Instance.CultureChanged += OnCultureChanged;

            // ✅ CONSERVÉ: Votre logique existante
            LoadSupplierData();
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
                System.Diagnostics.Debug.WriteLine("🌍 Mise à jour des textes EditSupplier");

                // ✅ Titre de la page
                Title = await GetTextAsync("Edit", "Edit");

                // ✅ Titre principal
                if (TitleLabel != null)
                    TitleLabel.Text = await GetTextAsync("Edit", "Edit Supplier");

                // ✅ Label du champ
                if (NameLabel != null)
                    NameLabel.Text = await GetTextAsync("Name", "Name");

                // ✅ Placeholder
                if (NameEntry != null)
                    NameEntry.Placeholder = await GetTextAsync("SupplierNamePlaceholder", "Supplier name");

                // ✅ Boutons
                if (SaveButton != null)
                    SaveButton.Text = await GetTextAsync("Save", "Save");

                if (BackButton != null)
                    BackButton.Text = await GetTextAsync("Back to List", "Back to List");

                // ✅ Sélecteur de langue
                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                // ✅ Mettre à jour l'indicateur de langue
                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("✅ Textes EditSupplier mis à jour");
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
                System.Diagnostics.Debug.WriteLine($"🌍 EditSupplier - Langue changée vers: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private void LoadSupplierData()
        {
            if (_supplier != null)
            {
                NameEntry.Text = _supplier.Name;
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // ✅ CONSERVÉ: Votre validation existante
                string supplierName = NameEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(supplierName))
                {
                    var errorTitle = await GetTextAsync("Error", "Erreur");
                    var nameRequiredMsg = await GetTextAsync("SupplierNameRequired", "Le nom du fournisseur est obligatoire.");
                    await DisplayAlert(errorTitle, nameRequiredMsg, "OK");
                    return;
                }

                // ✅ CONSERVÉ: Votre logique de mise à jour existante
                _supplier.Name = supplierName;

                // Appeler l'API pour sauvegarder les modifications
                bool success = await _supplierService.UpdateSupplierAsync(_supplier);

                if (success)
                {
                    var successTitle = await GetTextAsync("Success", "Succès");
                    var supplierModifiedMsg = await GetTextAsync("SupplierModifiedSuccessfully", "Le fournisseur '{0}' a été modifié avec succès!");
                    var message = string.Format(supplierModifiedMsg, supplierName);
                    await DisplayAlert(successTitle, message, "OK");

                    // Retourner à la page précédente
                    await Navigation.PopAsync();
                }
                else
                {
                    var errorTitle = await GetTextAsync("Error", "Erreur");
                    var modificationFailedMsg = await GetTextAsync("SupplierModificationFailed", "Échec de la modification du fournisseur");
                    await DisplayAlert(errorTitle, modificationFailedMsg, "OK");
                }
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var errorOccurredMsg = await GetTextAsync("ErrorOccurred", "Une erreur s'est produite");
                await DisplayAlert(errorTitle, $"{errorOccurredMsg}: {ex.Message}", "OK");
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        private async void OnBackToListClicked(object sender, EventArgs e)
        {
            // ✅ CONSERVÉ: Votre logique de vérification des modifications
            if (HasChanges())
            {
                var unsavedChangesTitle = await GetTextAsync("UnsavedChanges", "Modifications non sauvegardées");
                var unsavedChangesMsg = await GetTextAsync("UnsavedChangesMessage", "Vous avez des modifications non sauvegardées. Voulez-vous vraiment quitter ?");
                var yesText = await GetTextAsync("Yes", "Oui");
                var noText = await GetTextAsync("No", "Non");

                bool confirmExit = await DisplayAlert(unsavedChangesTitle, unsavedChangesMsg, yesText, noText);

                if (!confirmExit)
                    return;
            }
            await Navigation.PopAsync();
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private bool HasChanges()
        {
            return _supplier.Name != NameEntry.Text?.Trim();
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
        ~EditSupplierPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}