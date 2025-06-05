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

            // ✅ AJOUT: S'abonner aux changements de langue
            TranslationService.Instance.CultureChanged += OnCultureChanged;
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // ✅ AJOUT: Charger les traductions en premier
            await LoadTranslationsAsync();

            // ✅ CONSERVÉ: Votre logique existante
            await LoadPickerDataAsync();
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
                System.Diagnostics.Debug.WriteLine("🌍 Mise à jour des textes CreateProduct");

                // ✅ Titre de la page
                Title = await GetTextAsync("Title", "Create a product");

                // ✅ Labels
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

                // ✅ Placeholders
                if (NameEntry != null)
                    NameEntry.Placeholder = await GetTextAsync("EnterProductName", "Enter product name");

                if (SerialNumberEntry != null)
                    SerialNumberEntry.Placeholder = await GetTextAsync("EnterSerialNumber", "Enter serial number");

                // ✅ Titre des Pickers
                if (TypePicker != null)
                    TypePicker.Title = await GetTextAsync("SelectType", "-- Select a type --");

                if (SupplierPicker != null)
                    SupplierPicker.Title = await GetTextAsync("SelectSupplier", "-- Select a supplier --");

                // ✅ Sélecteur de langue
                if (LanguageLabel != null)
                    LanguageLabel.Text = await GetTextAsync("Language", "Language");

                // ✅ Mettre à jour l'indicateur de langue
                await UpdateLanguageFlag();

                System.Diagnostics.Debug.WriteLine("✅ Textes CreateProduct mis à jour");
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
                System.Diagnostics.Debug.WriteLine($"🌍 CreateProduct - Langue changée vers: {newCulture}");
                await UpdateTextsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur changement langue: {ex.Message}");
            }
        }

        // ✅ CONSERVÉ: Votre méthode existante inchangée
        private async Task LoadPickerDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Chargement des données pour les Pickers...");

                // Afficher un indicateur de chargement si vous en avez un
                // LoadingIndicator.IsVisible = true;

                // Charger les fournisseurs
                System.Diagnostics.Debug.WriteLine("📞 Appel GetSuppliersAsync...");
                var suppliers = await _supplierService.GetSuppliersAsync();
                if (suppliers != null && suppliers.Any())
                {
                    SupplierPicker.ItemsSource = suppliers.ToList();
                    SupplierPicker.ItemDisplayBinding = new Binding("Name");
                    System.Diagnostics.Debug.WriteLine($"✅ {suppliers.Count()} fournisseurs chargés");

                    // Debug: afficher les noms des fournisseurs
                    foreach (var supplier in suppliers.Take(3))
                    {
                        System.Diagnostics.Debug.WriteLine($"   📦 Fournisseur: {supplier.Name} (ID: {supplier.Id})");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Aucun fournisseur trouvé");
                    var warningTitle = await GetTextAsync("Warning", "Attention");
                    var noSupplierMsg = await GetTextAsync("NoSupplierAvailable", "Aucun fournisseur disponible");
                    await DisplayAlert(warningTitle, noSupplierMsg, "OK");
                }

                // Charger les types
                System.Diagnostics.Debug.WriteLine("📞 Appel GetAllAsync...");
                var types = await _typeArticleService.GetAllAsync();
                if (types != null && types.Any())
                {
                    TypePicker.ItemsSource = types.ToList();
                    TypePicker.ItemDisplayBinding = new Binding("Name");
                    System.Diagnostics.Debug.WriteLine($"✅ {types.Count()} types chargés");

                    // Debug: afficher les noms des types
                    foreach (var type in types.Take(3))
                    {
                        System.Diagnostics.Debug.WriteLine($"   🏷️ Type: {type.Name} (ID: {type.Id})");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Aucun type trouvé");
                    var warningTitle = await GetTextAsync("Warning", "Attention");
                    var noTypeMsg = await GetTextAsync("NoTypeAvailable", "Aucun type disponible");
                    await DisplayAlert(warningTitle, noTypeMsg, "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur lors du chargement des données: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"📍 StackTrace: {ex.StackTrace}");
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var loadErrorMsg = await GetTextAsync("DataLoadError", "Impossible de charger les données");
                await DisplayAlert(errorTitle, $"{loadErrorMsg}: {ex.Message}", "OK");
            }
            finally
            {
                // Masquer l'indicateur de chargement
                // LoadingIndicator.IsVisible = false;
            }
        }

        // ✅ MODIFIÉ: Votre méthode existante avec traductions ajoutées
        private async void OnCreateClicked(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 Début de création du produit");

                // ✅ Validation des champs avec traductions
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

                System.Diagnostics.Debug.WriteLine("✅ Validation réussie");

                var selectedType = TypePicker.SelectedItem as TechStockMaui.Models.TypeArticle.TypeArticle;
                var selectedSupplier = SupplierPicker.SelectedItem as TechStockMaui.Models.Supplier.Supplier;

                System.Diagnostics.Debug.WriteLine($"🔍 Type sélectionné: {selectedType?.Name} (ID: {selectedType?.Id})");
                System.Diagnostics.Debug.WriteLine($"🔍 Fournisseur sélectionné: {selectedSupplier?.Name} (ID: {selectedSupplier?.Id})");

                var newProduct = new Product
                {
                    Name = NameEntry.Text,
                    SerialNumber = SerialNumberEntry.Text,
                    TypeId = selectedType?.Id ?? 0,
                    SupplierId = selectedSupplier?.Id ?? 0
                    // Pas d'objets complets - juste les IDs
                };

                System.Diagnostics.Debug.WriteLine($"🆕 Création du produit: {newProduct.Name}");
                System.Diagnostics.Debug.WriteLine($"📋 TypeId: {newProduct.TypeId}, SupplierId: {newProduct.SupplierId}");
                System.Diagnostics.Debug.WriteLine($"🔢 SerialNumber: {newProduct.SerialNumber}");

                System.Diagnostics.Debug.WriteLine("📞 Appel CreateProductAsync...");
                bool result = await _productService.CreateProductAsync(newProduct);
                System.Diagnostics.Debug.WriteLine($"📊 Résultat CreateProductAsync: {result}");

                if (result)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Produit créé avec succès");
                    var successTitle = await GetTextAsync("Success", "Succès");
                    var productCreatedMsg = await GetTextAsync("ProductCreatedSuccessfully", "Produit créé avec succès");
                    await DisplayAlert(successTitle, productCreatedMsg, "OK");

                    // SOLUTION: Envoyer un message pour rafraîchir la liste
                    MessagingCenter.Send(this, "ProductCreated");

                    await Navigation.PopAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Échec de la création");
                    var errorTitle = await GetTextAsync("Error", "Erreur");
                    var creationFailedMsg = await GetTextAsync("ProductCreationFailed", "La création du produit a échoué");
                    await DisplayAlert(errorTitle, creationFailedMsg, "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exception création produit: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"📍 StackTrace: {ex.StackTrace}");
                var errorTitle = await GetTextAsync("Error", "Erreur");
                var creationErrorMsg = await GetTextAsync("CreationError", "Erreur lors de la création");
                await DisplayAlert(errorTitle, $"{creationErrorMsg}: {ex.Message}", "OK");
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

        // ✅ AJOUT: Destructeur pour nettoyer l'événement
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