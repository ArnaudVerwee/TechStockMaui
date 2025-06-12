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

            TranslationService.Instance.CultureChanged += OnCultureChanged;

            LoadSupplierData();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadTranslationsAsync();
        }

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
                System.Diagnostics.Debug.WriteLine($"Translations loading error: {ex.Message}");
            }
        }

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

        private async Task UpdateTextsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Updating EditSupplier texts");

                Title = await GetTextAsync("Edit", "Edit");

                if (TitleLabel != null)
                    TitleLabel.Text = await GetTextAsync("Edit", "Edit Supplier");

                if (NameLabel != null)
                    NameLabel.Text = await GetTextAsync("Name", "Name");

                if (NameEntry != null)
                    NameEntry.Placeholder = await GetTextAsync("SupplierNamePlaceholder", "Supplier name");

                if (SaveButton != null)
                    SaveButton.Text = await GetTextAsync("Save", "Save");

                if (BackButton != null)
                    BackButton.Text = await GetTextAsync("Back to List", "Back to List");

                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("EditSupplier texts updated");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTextsAsync error: {ex.Message}");
            }
        }

        private async void OnCultureChanged(object sender, string newCulture)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"EditSupplier - Language changed to: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
            }
        }

        private void LoadSupplierData()
        {
            if (_supplier != null)
            {
                NameEntry.Text = _supplier.Name;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                string supplierName = NameEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(supplierName))
                {
                    var errorTitle = await GetTextAsync("Error", "Erreur");
                    var nameRequiredMsg = await GetTextAsync("SupplierNameRequired", "Le nom du fournisseur est obligatoire.");
                    await DisplayAlert(errorTitle, nameRequiredMsg, "OK");
                    return;
                }

                _supplier.Name = supplierName;

                bool success = await _supplierService.UpdateSupplierAsync(_supplier);

                if (success)
                {
                    var successTitle = await GetTextAsync("Success", "Succès");
                    var supplierModifiedMsg = await GetTextAsync("SupplierModifiedSuccessfully", "Le fournisseur '{0}' a été modifié avec succès!");
                    var message = string.Format(supplierModifiedMsg, supplierName);
                    await DisplayAlert(successTitle, message, "OK");

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

        private async void OnBackToListClicked(object sender, EventArgs e)
        {
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

        private bool HasChanges()
        {
            return _supplier.Name != NameEntry.Text?.Trim();
        }

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
                        System.Diagnostics.Debug.WriteLine($"Changing to: {newCulture}");
                        await translationService.SetCurrentCultureAsync(newCulture);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
            }
        }

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
                System.Diagnostics.Debug.WriteLine($"Flag update error: {ex.Message}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

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