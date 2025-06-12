using TechStockMaui.Models;
using TechStockMaui.Services;

namespace TechStockMaui.Views
{
    public partial class ProductDetailsPage : ContentPage
    {
        private Product _product;

        public ProductDetailsPage(Product product)
        {
            InitializeComponent();
            _product = product;
            BindingContext = product;

            System.Diagnostics.Debug.WriteLine($"Product received: Name={product.Name}, TypeName={product.TypeName}, SupplierName={product.SupplierName}");

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
                System.Diagnostics.Debug.WriteLine("Updating ProductDetails texts");

                Title = await GetTextAsync("Details", "Details");

                if (TitleLabel != null)
                    TitleLabel.Text = await GetTextAsync("Details", "Product Details");

                if (NameFieldLabel != null)
                    NameFieldLabel.Text = await GetTextAsync("Name", "Name") + ":";

                if (SerialFieldLabel != null)
                    SerialFieldLabel.Text = await GetTextAsync("Serial Number", "Serial Number") + ":";

                if (TypeFieldLabel != null)
                    TypeFieldLabel.Text = await GetTextAsync("Type", "Type") + ":";

                if (SupplierFieldLabel != null)
                    SupplierFieldLabel.Text = await GetTextAsync("Supplier", "Supplier") + ":";

                if (EditButton != null)
                    EditButton.Text = await GetTextAsync("Edit", "Edit");

                if (BackButton != null)
                    BackButton.Text = await GetTextAsync("Back to List", "Back to List");

                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("ProductDetails texts updated");
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
                System.Diagnostics.Debug.WriteLine($"ProductDetails - Language changed to: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new EditProductPage(_product));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Edit navigation error: {ex.Message}");
                var errorTitle = await GetTextAsync("Error", "Error");
                var editErrorMsg = await GetTextAsync("CannotOpenEditPage", "Cannot open edit page");
                await DisplayAlert(errorTitle, editErrorMsg, "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Back navigation error: {ex.Message}");
                var errorTitle = await GetTextAsync("Error", "Error");
                var navigationErrorMsg = await GetTextAsync("NavigationError", "Navigation error");
                await DisplayAlert(errorTitle, navigationErrorMsg, "OK");
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

        ~ProductDetailsPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}