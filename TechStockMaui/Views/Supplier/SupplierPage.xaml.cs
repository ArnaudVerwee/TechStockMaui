using System.Collections.ObjectModel;
using TechStockMaui.Models.Supplier;
using TechStockMaui.Services;

namespace TechStockMaui.Views.Supplier
{
    public partial class SupplierPage : ContentPage
    {
        private SupplierService _supplierService;
        private ObservableCollection<TechStockMaui.Models.Supplier.Supplier> _suppliers;

        public SupplierPage()
        {
            InitializeComponent();
            _supplierService = new SupplierService();
            _suppliers = new ObservableCollection<TechStockMaui.Models.Supplier.Supplier>();
            SupplierList.ItemsSource = _suppliers;

            TranslationService.Instance.CultureChanged += OnCultureChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadTranslationsAsync();

            await LoadSuppliersAsync();
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
                System.Diagnostics.Debug.WriteLine("Updating Suppliers texts");

                Title = await GetTextAsync("Suppliers", "Suppliers");

                if (PageTitleLabel != null)
                    PageTitleLabel.Text = await GetTextAsync("Suppliers", "Suppliers");

                if (CreateNewButton != null)
                    CreateNewButton.Text = "➕ " + await GetTextAsync("Create New", "Create New");

                if (NoSuppliersLabel != null)
                    NoSuppliersLabel.Text = await GetTextAsync("NoSuppliersFound", "No suppliers found");

                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("Suppliers texts updated");
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
                System.Diagnostics.Debug.WriteLine($"Suppliers - Language changed to: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
            }
        }

        private async Task LoadSuppliersAsync()
        {
            try
            {
                var suppliers = await _supplierService.GetSuppliersAsync();

                _suppliers.Clear();
                foreach (var supplier in suppliers)
                {
                    _suppliers.Add(supplier);
                }

                NoSuppliersLabel.IsVisible = !_suppliers.Any();
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var loadSuppliersError = await GetTextAsync("LoadSuppliersError", "Impossible de charger les fournisseurs");
                await DisplayAlert(errorTitle, $"{loadSuppliersError}: {ex.Message}", "OK");
            }
        }

        private async void OnBackToDashboardClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var backError = await GetTextAsync("BackError", "Impossible de retourner en arrière");
                await DisplayAlert(errorTitle, $"{backError}: {ex.Message}", "OK");
            }
        }

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new CreateSupplierPage());
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var createPageError = await GetTextAsync("CreatePageError", "Impossible d'ouvrir la page de création");
                await DisplayAlert(errorTitle, $"{createPageError}: {ex.Message}", "OK");
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is TechStockMaui.Models.Supplier.Supplier supplier)
                {
                    await Navigation.PushAsync(new EditSupplierPage(supplier));
                }
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var editPageError = await GetTextAsync("EditPageError", "Impossible d'ouvrir la page de modification");
                await DisplayAlert(errorTitle, $"{editPageError}: {ex.Message}", "OK");
            }
        }

        private async void OnDetailsClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is TechStockMaui.Models.Supplier.Supplier supplier)
                {
                    await Navigation.PushAsync(new DetailsSupplierPage(supplier));
                }
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var detailsError = await GetTextAsync("DetailsError", "Impossible d'ouvrir les détails");
                await DisplayAlert(errorTitle, $"{detailsError}: {ex.Message}", "OK");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.CommandParameter is TechStockMaui.Models.Supplier.Supplier supplier)
                {
                    var confirmTitle = await GetTextAsync("Confirmation", "Confirmation");
                    var deleteQuestion = await GetTextAsync("DeleteSupplierQuestion", "Êtes-vous sûr de vouloir supprimer le fournisseur '{0}' ?");
                    var deleteMsg = string.Format(deleteQuestion, supplier.Name);
                    var yesText = await GetTextAsync("Yes", "Oui");
                    var noText = await GetTextAsync("No", "Non");

                    bool confirm = await DisplayAlert(confirmTitle, deleteMsg, yesText, noText);

                    if (confirm)
                    {
                        bool success = await _supplierService.DeleteSupplierAsync(supplier.Id);

                        if (success)
                        {
                            _suppliers.Remove(supplier);
                            var successTitle = await GetTextAsync("Success", "Succès");
                            var supplierDeletedMsg = await GetTextAsync("SupplierDeletedSuccessfully", "Fournisseur supprimé avec succès");
                            await DisplayAlert(successTitle, supplierDeletedMsg, "OK");
                        }
                        else
                        {
                            var errorTitle = await GetTextAsync("Error", "Erreur");
                            var deleteFailedMsg = await GetTextAsync("SupplierDeleteFailed", "Échec de la suppression du fournisseur");
                            await DisplayAlert(errorTitle, deleteFailedMsg, "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var deleteError = await GetTextAsync("DeleteSupplierError", "Impossible de supprimer le fournisseur");
                await DisplayAlert(errorTitle, $"{deleteError}: {ex.Message}", "OK");
            }
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

        ~SupplierPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}