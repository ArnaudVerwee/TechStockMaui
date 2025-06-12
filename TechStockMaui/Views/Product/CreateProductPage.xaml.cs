using TechStockMaui.Services;
using TechStockMaui.Models;
using System.Collections.ObjectModel;

namespace TechStockMaui.Views
{
    public partial class CreateProductPage : ContentPage
    {
        private readonly ProductService _productService;
        private readonly SupplierService _supplierService;
        private readonly TypeArticleService _typeArticleService;

        public CreateProductPage()
        {
            InitializeComponent();
            _productService = new ProductService();
            _supplierService = new SupplierService();
            _typeArticleService = new TypeArticleService();

            TranslationService.Instance.CultureChanged += OnCultureChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadTranslationsAsync();

            await LoadPickerDataAsync();
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
                System.Diagnostics.Debug.WriteLine("Updating CreateProduct texts");

                Title = await GetTextAsync("Title", "Create a product");

                if (TitleLabel != null)
                    TitleLabel.Text = await GetTextAsync("Title", "Create a product");

                if (NameLabel != null)
                    NameLabel.Text = await GetTextAsync("Name", "Name");

                if (SerialNumberLabel != null)
                    SerialNumberLabel.Text = await GetTextAsync("SerialNumber", "Serial Number");

                if (TypeLabel != null)
                    TypeLabel.Text = await GetTextAsync("TypeId", "Type");

                if (SupplierLabel != null)
                    SupplierLabel.Text = await GetTextAsync("SupplierId", "Supplier");

                if (CreateButton != null)
                    CreateButton.Text = await GetTextAsync("Create", "Create");

                if (BackButton != null)
                    BackButton.Text = await GetTextAsync("BackToList", "Back to list");

                if (NameEntry != null)
                    NameEntry.Placeholder = await GetTextAsync("EnterProductName", "Enter product name");

                if (SerialNumberEntry != null)
                    SerialNumberEntry.Placeholder = await GetTextAsync("EnterSerialNumber", "Enter serial number");

                if (TypePicker != null)
                    TypePicker.Title = await GetTextAsync("SelectType", "-- Select a type --");

                if (SupplierPicker != null)
                    SupplierPicker.Title = await GetTextAsync("SelectSupplier", "-- Select a supplier --");

                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("CreateProduct texts updated");
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
                System.Diagnostics.Debug.WriteLine($"CreateProduct - Language changed to: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Language change error: {ex.Message}");
            }
        }

        private async Task LoadPickerDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading data for Pickers...");

                System.Diagnostics.Debug.WriteLine("Calling GetSuppliersAsync...");
                var suppliers = await _supplierService.GetSuppliersAsync();
                if (suppliers != null && suppliers.Any())
                {
                    SupplierPicker.ItemsSource = suppliers.ToList();
                    SupplierPicker.ItemDisplayBinding = new Binding("Name");
                    System.Diagnostics.Debug.WriteLine($"{suppliers.Count()} suppliers loaded");

                    foreach (var supplier in suppliers.Take(3))
                    {
                        System.Diagnostics.Debug.WriteLine($"   Supplier: {supplier.Name} (ID: {supplier.Id})");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No suppliers found");
                    var warningTitle = await GetTextAsync("Warning", "Attention");
                    var noSupplierMsg = await GetTextAsync("NoSupplierAvailable", "Aucun fournisseur disponible");
                    await DisplayAlert(warningTitle, noSupplierMsg, "OK");
                }

                System.Diagnostics.Debug.WriteLine("Calling GetAllAsync...");
                var types = await _typeArticleService.GetAllAsync();
                if (types != null && types.Any())
                {
                    TypePicker.ItemsSource = types.ToList();
                    TypePicker.ItemDisplayBinding = new Binding("Name");
                    System.Diagnostics.Debug.WriteLine($"{types.Count()} types loaded");

                    foreach (var type in types.Take(3))
                    {
                        System.Diagnostics.Debug.WriteLine($"   Type: {type.Name} (ID: {type.Id})");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No types found");
                    var warningTitle = await GetTextAsync("Warning", "Attention");
                    var noTypeMsg = await GetTextAsync("NoTypeAvailable", "Aucun type disponible");
                    await DisplayAlert(warningTitle, noTypeMsg, "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Data loading error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var loadErrorMsg = await GetTextAsync("DataLoadError", "Impossible de charger les données");
                await DisplayAlert(errorTitle, $"{loadErrorMsg}: {ex.Message}", "OK");
            }
            finally
            {
            }
        }

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Starting product creation");

                if (string.IsNullOrWhiteSpace(NameEntry.Text))
                {
                    var errorTitle = await GetTextAsync("Error", "Erreur");
                    var nameRequiredMsg = await GetTextAsync("NameRequired", "Le nom du produit est requis");
                    await DisplayAlert(errorTitle, nameRequiredMsg, "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(SerialNumberEntry.Text))
                {
                    var errorTitle = await GetTextAsync("Error", "Erreur");
                    var serialRequiredMsg = await GetTextAsync("SerialNumberRequired", "Le numéro de série est requis");
                    await DisplayAlert(errorTitle, serialRequiredMsg, "OK");
                    return;
                }

                if (TypePicker.SelectedItem == null)
                {
                    var errorTitle = await GetTextAsync("Error", "Erreur");
                    var selectTypeMsg = await GetTextAsync("PleaseSelectType", "Veuillez sélectionner un type");
                    await DisplayAlert(errorTitle, selectTypeMsg, "OK");
                    return;
                }

                if (SupplierPicker.SelectedItem == null)
                {
                    var errorTitle = await GetTextAsync("Error", "Erreur");
                    var selectSupplierMsg = await GetTextAsync("PleaseSelectSupplier", "Veuillez sélectionner un fournisseur");
                    await DisplayAlert(errorTitle, selectSupplierMsg, "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("Validation successful");

                var selectedType = TypePicker.SelectedItem as TechStockMaui.Models.TypeArticle.TypeArticle;
                var selectedSupplier = SupplierPicker.SelectedItem as TechStockMaui.Models.Supplier.Supplier;

                System.Diagnostics.Debug.WriteLine($"Selected type: {selectedType?.Name} (ID: {selectedType?.Id})");
                System.Diagnostics.Debug.WriteLine($"Selected supplier: {selectedSupplier?.Name} (ID: {selectedSupplier?.Id})");

                var newProduct = new Product
                {
                    Name = NameEntry.Text,
                    SerialNumber = SerialNumberEntry.Text,
                    TypeId = selectedType?.Id ?? 0,
                    SupplierId = selectedSupplier?.Id ?? 0
                };

                System.Diagnostics.Debug.WriteLine($"Creating product: {newProduct.Name}");
                System.Diagnostics.Debug.WriteLine($"TypeId: {newProduct.TypeId}, SupplierId: {newProduct.SupplierId}");
                System.Diagnostics.Debug.WriteLine($"SerialNumber: {newProduct.SerialNumber}");

                System.Diagnostics.Debug.WriteLine("Calling CreateProductAsync...");
                bool result = await _productService.CreateProductAsync(newProduct);
                System.Diagnostics.Debug.WriteLine($"CreateProductAsync result: {result}");

                if (result)
                {
                    System.Diagnostics.Debug.WriteLine("Product created successfully");
                    var successTitle = await GetTextAsync("Success", "Succès");
                    var productCreatedMsg = await GetTextAsync("ProductCreatedSuccessfully", "Produit créé avec succès");
                    await DisplayAlert(successTitle, productCreatedMsg, "OK");

                    MessagingCenter.Send(this, "ProductCreated");

                    await Navigation.PopAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Product creation failed");
                    var errorTitle = await GetTextAsync("Error", "Erreur");
                    var creationFailedMsg = await GetTextAsync("ProductCreationFailed", "La création du produit a échoué");
                    await DisplayAlert(errorTitle, creationFailedMsg, "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Product creation exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var creationErrorMsg = await GetTextAsync("CreationError", "Erreur lors de la création");
                await DisplayAlert(errorTitle, $"{creationErrorMsg}: {ex.Message}", "OK");
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

        ~CreateProductPage()
        {
            try
            {
                TranslationService.Instance.CultureChanged -= OnCultureChanged;
            }
            catch { }
        }
    }
}