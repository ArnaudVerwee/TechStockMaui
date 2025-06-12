using TechStockMaui.Services;
using TechStockMaui.Models.Supplier;

namespace TechStockMaui.Views.Supplier
{
    public partial class CreateSupplierPage : ContentPage
    {
        private SupplierService _supplierService;

        public CreateSupplierPage()
        {
            InitializeComponent();
            _supplierService = new SupplierService();

            TranslationService.Instance.CultureChanged += OnCultureChanged;
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
                System.Diagnostics.Debug.WriteLine("Updating CreateSupplier texts");

                Title = await GetTextAsync("Create", "Create");

                if (TitleLabel != null)
                    TitleLabel.Text = await GetTextAsync("Create", "Create Supplier");

                if (NameLabel != null)
                    NameLabel.Text = await GetTextAsync("Name", "Name");

                if (NameEntry != null)
                    NameEntry.Placeholder = await GetTextAsync("EnterSupplierName", "Enter supplier name");

                if (CreateButton != null)
                    CreateButton.Text = await GetTextAsync("Create", "Create");

                if (BackButton != null)
                    BackButton.Text = await GetTextAsync("Back to List", "Back to List");

                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("CreateSupplier texts updated");
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
                System.Diagnostics.Debug.WriteLine($"CreateSupplier - Language changed to: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
            }
        }

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            try
            {
                string name = NameEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    var errorTitle = await GetTextAsync("Error", "Erreur");
                    var nameRequiredMsg = await GetTextAsync("SupplierNameRequired", "Le nom du fournisseur est obligatoire.");
                    await DisplayAlert(errorTitle, nameRequiredMsg, "OK");
                    return;
                }

                var newSupplier = new TechStockMaui.Models.Supplier.Supplier
                {
                    Name = name
                };

                bool success = await _supplierService.CreateSupplierAsync(newSupplier);

                if (success)
                {
                    var successTitle = await GetTextAsync("Success", "Succès");
                    var supplierCreatedMsg = await GetTextAsync("SupplierCreatedSuccessfully", "Fournisseur '{0}' créé avec succès!");
                    var message = string.Format(supplierCreatedMsg, name);
                    await DisplayAlert(successTitle, message, "OK");

                    await Navigation.PopAsync();
                }
                else
                {
                    var errorTitle = await GetTextAsync("Error", "Erreur");
                    var creationFailedMsg = await GetTextAsync("SupplierCreationFailed", "Échec de la création du fournisseur");
                    await DisplayAlert(errorTitle, creationFailedMsg, "OK");
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
            await Navigation.PopAsync();
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

        ~CreateSupplierPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}